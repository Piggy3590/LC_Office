using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Components
{
    public class ElevatorSystem : NetworkBehaviour
    {
        public static int elevatorFloor;
        public bool elevatorMoving { get; private set; } = false;
        public bool elevatorIdle { get; private set; } = true;

        public Animator animator;

        public bool hasPower = true;
        public LungProp inserted = null;
        public BoxCollider interactTrigger;
        public Animator lungParent;
        public AudioSource socketSource;
        public AudioClip insertClip;
        public AudioClip removeClip;

        public static ElevatorSystem System { get; private set; }

        [HideInInspector] public List<ElevatorPannel> controlPannels = new();
        [HideInInspector] public ElevatorController controller;

        // Pannel Font
        [HideInInspector] public TMP_FontAsset textFont;
        [HideInInspector] public Material textMaterial;
        [HideInInspector] public Color textColor;

        private GameObject SparkParticle;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            controller = FindObjectOfType<ElevatorController>();
            elevatorFloor = 1;

            TMP_Text text = GameObject.Find("doorHydraulics").GetComponent<TMP_Text>();
            textFont = text.font;
            textMaterial = text.material;
            textColor = new Color(1, 0.3444f, 0, 1);

            System = this;
            transform.position = controller.startPos + new Vector3(4.55f, -21.5f, -2.8f);
            for (int i = 0; i < controller.floorIndicators.Length; i++)
            {
                controller.floorIndicators[i].SetActive(i == 1);
            }
        }

        public static void Spawn(Transform parent)
        {
            if (System != null) return;
            if (RoundManager.Instance.IsServer || RoundManager.Instance.IsHost)
            {
                Plugin.mls.LogInfo("Spawning Elevator On Host");
                GameObject elevator = Instantiate(Plugin.ActualElevator, parent);
                elevator.transform.localRotation = Quaternion.Euler(0, -90, 0);
                elevator.GetComponent<NetworkObject>().Spawn();

                Instantiate(Plugin.StoragePrefab).GetComponent<NetworkObject>().Spawn();
            }
        }

        public static void Despawn()
        {
            if (System != null)
            {
                Plugin.mls.LogInfo("Despawning Elevator");
                System.NetworkObject.Despawn();
                FindObjectOfType<ElevatorStorage>()?.GetComponent<NetworkObject>().Despawn();
            }
        }

        public override void OnDestroy()
        {
            System = null;
            base.OnDestroy();
        }

        void LateUpdate()
        {
            if (inserted != null)
            {
                inserted.transform.position = lungParent.transform.position;
                inserted.transform.rotation = lungParent.transform.rotation *= Quaternion.Euler(Vector3.up * 90);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ElevatorTriggerServerRpc(int floor)
        {
            if ((hasPower || inserted != null) && elevatorFloor != floor && elevatorIdle) ElevatorTriggerClientRpc(floor, false);
        }

        [ClientRpc]
        private void ElevatorTriggerClientRpc(int floor, bool emergency)
            => StartCoroutine(TriggerElevator(floor, emergency));

        IEnumerator TriggerElevator(int floor, bool emergency)
        {
            bool goingUp = floor > elevatorFloor;
            controller.CloseDoor();
            elevatorIdle = false;

            foreach (ElevatorPannel pannel in controlPannels)
            {
                pannel.display.text = "Processing...";
            }

            yield return new WaitForSeconds(2f);

            controller.floorIndicators[floor].SetActive(true);
            bool shortDistance = Mathf.Abs(elevatorFloor - floor) == 1;
            int animHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            controller.animator.SetInteger("floor", floor);
            animator.SetInteger("floor", floor);
            controller.source.PlayOneShot(goingUp ? (shortDistance ? controller.upShortClip : controller.upClip) : (shortDistance ? controller.downShortClip : controller.downClip));

            string directionLabel = goingUp ? "Ascending" : "Descending";
            if (emergency) directionLabel = $"Emergency {directionLabel}";
            Plugin.mls.LogInfo($"Elevator {directionLabel} from {elevatorFloor} to {floor}");

            GameObject oldIndicator = controller.floorIndicators[elevatorFloor];
            elevatorFloor = floor;
            elevatorMoving = true;

            foreach (ElevatorPannel pannel in controlPannels)
            {
                pannel.display.text =  directionLabel;
                pannel.lightAnimator.SetInteger("sta", floor);
            }

            yield return 0;
            yield return new WaitUntil(() =>
            {
                var animInfo = animator.GetCurrentAnimatorStateInfo(0);
                return animHash != animInfo.shortNameHash && animInfo.normalizedTime >= 0.98f;
            });

            elevatorMoving = false;
            controller.OpenDoor(elevatorFloor);
            oldIndicator.SetActive(false);

            yield return 0;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.2f);

            elevatorIdle = true;

            bool powered = hasPower || inserted != null;
            string text = powered ? "Idle" : "\"No Power\"";

            foreach (ElevatorPannel pannel in controlPannels)
            {
                pannel.display.text = text;
                pannel.SetInteractable(powered);
            }

            yield break;
        }

        public void OnInteract(PlayerControllerB player)
        {
            if (inserted == null)
            {
                if (player.currentlyHeldObjectServer != null && player.currentlyHeldObjectServer is LungProp prop)
                {
                    player.DiscardHeldObject(true, placePosition: lungParent.transform.position);
                    interactTrigger.enabled = false;
                    PlaceLungServerRpc(prop.NetworkObject);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaceLungServerRpc(NetworkObjectReference lungRef)
        {
            if (inserted == null && lungRef.TryGet(out NetworkObject netobj) && netobj.TryGetComponent(out LungProp lung))
            {
                PlaceLungClientRpc(lungRef);
            }
        }

        [ClientRpc]
        public void PlaceLungClientRpc(NetworkObjectReference lungRef)
        {
            if (lungRef.TryGet(out NetworkObject netobj) && netobj.TryGetComponent(out LungProp lung))
            {
                lung.grabbableToEnemies = false;
                interactTrigger.enabled = false;
                inserted = lung;
                lungParent.SetTrigger("LungPlaced");
                socketSource.PlayOneShot(insertClip);
                controller.ToggleLights(true);
                controller.music.ToggleMusic(true);
                foreach (ElevatorPannel pannel in controlPannels)
                {
                    pannel.display.text = "Idle";
                    pannel.SetInteractable(true);
                }
                Plugin.mls.LogInfo($"Inserted Elevator Apparatus {inserted == null}");
            }
        }

        internal void OnRemoveApparatus()
        {
            if (IsServer || IsHost)
            {
                PullLungClientRpc(false);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void PullLungServerRpc()
        {
            if (inserted == null) return;
            PullLungClientRpc(true);
        }

        IEnumerator DropElevator()
        {
            yield return new WaitForSeconds(4);
            ElevatorTriggerClientRpc(0, true);
        }

        [ClientRpc]
        public void PullLungClientRpc(bool elevatorSocket)
        {
            if (elevatorSocket)
            {
                if (inserted.sparkParticle != null)
                {
                    if (SparkParticle != null)
                    {
                        Destroy(SparkParticle);
                    }
                    SparkParticle = Instantiate(inserted.sparkParticle, lungParent.transform.position, Quaternion.identity, null);
                }
                inserted = null;

                socketSource.PlayOneShot(removeClip);
                lungParent.ResetTrigger("LungPlaced");
                interactTrigger.enabled = true;
            }
            else
            {
                hasPower = false;
            }

            if (!hasPower && inserted == null)
            {
                foreach (ElevatorPannel pannel in controlPannels)
                {
                    pannel.display.text = "\"No Power\"";
                    pannel.SetInteractable(false);
                }
                controller.ToggleLights(false);
                controller.music.ToggleMusic(false);

                if ((IsHost || IsServer) && elevatorFloor != 0) StartCoroutine(DropElevator());
            }
            Plugin.mls.LogInfo($"Removed Apparatus (Elevator Socker: {elevatorSocket}) (Has Inserted: {inserted == null})");
        }
    }
}
