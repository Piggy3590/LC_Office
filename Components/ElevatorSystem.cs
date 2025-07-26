using GameNetcodeStuff;
using PathfindingLib.API.SmartPathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LCOffice.Components
{
    public class ElevatorSystem : NetworkBehaviour, IElevator
    {
        public static int elevatorFloor;
        public bool elevatorMoving { get; private set; } = false;
        public bool elevatorIdle { get; private set; } = true;

        public Animator animator;

        public bool hasPower = true;
        public LungProp inserted = null;
        public bool Operational => hasPower || inserted != null;

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

        private List<ElevatorFloor> elevatorFloors = null;
        private bool registeredPathfinding = false;

        public OffMeshLink elevatorLink;
        public Color CurrentColor = Color.cyan;
        public Color MovingColor = Color.yellow;
        public Color OffColor = Color.black;

        public Transform InsideButtonNavMeshNode => controller.insidePos;
        public ElevatorFloor ClosestFloor
        {
            get
            {
                int closest = 0;
                float distance = 1000000f;
                for (int i = 0; i < elevatorFloors.Count; i++)
                {
                    float d = Vector3.Distance(controller.floorPos[i].position, controller.insidePos.position);
                    if (d < distance)
                    {
                        closest = i;
                        distance = d;
                    }
                }
                return elevatorFloors[closest];
            }
        }
        public bool DoorsAreOpen => elevatorIdle;
        public ElevatorFloor TargetFloor => elevatorFloors[elevatorFloor];

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
            for (int i = 0; i < controller.notices.Length; i++)
            {
                controller.notices[i].elevatorLight.color = i == 1 ? CurrentColor : OffColor;
            }

            // PathfindingLib

            elevatorFloors = controller.floorPos.Select((x) => new ElevatorFloor(this, x)).ToList();

            RegisterPathfinding(true);
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
            RegisterPathfinding(false);
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

        private void RegisterPathfinding(bool active)
        {
            if (elevatorFloors == null)
            {
                Plugin.mls.LogWarning("Elevator tried to register before initialization!!! Bad!");
                return;
            }

            // this is for registering the elevator only
            active = active && Operational;
            if (active == registeredPathfinding) return;

            registeredPathfinding = active;
            elevatorFloors.ForEach(active ? SmartPathfinding.RegisterElevatorFloor : SmartPathfinding.UnregisterElevatorFloor);
        }

        public void GoToFloor(ElevatorFloor floor)
        {
            for (int i = 0; i < elevatorFloors.Count; i++)
            {
                ElevatorFloor target = elevatorFloors[i];
                if (floor == target)
                {
                    ElevatorTriggerServerRpc(i);
                    return;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ElevatorTriggerServerRpc(int floor)
        {
            if (Operational && elevatorFloor != floor && elevatorIdle) ElevatorTriggerClientRpc(floor, false);
        }

        [ClientRpc]
        private void ElevatorTriggerClientRpc(int floor, bool emergency)
        {
            StartCoroutine(TriggerElevator(floor, emergency));
        }

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

            elevatorLink.activated = false;

            SpriteRenderer currentNotice = controller.notices[floor].elevatorLight;
            SpriteRenderer oldNotice = controller.notices[elevatorFloor].elevatorLight;
            oldNotice.color = MovingColor;

            bool shortDistance = Mathf.Abs(elevatorFloor - floor) == 1;
            int animHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            controller.animator.SetInteger("floor", floor);
            animator.SetInteger("floor", floor);
            controller.source.PlayOneShot(goingUp ? (shortDistance ? controller.upShortClip : controller.upClip) : (shortDistance ? controller.downShortClip : controller.downClip));

            string directionLabel = goingUp ? "Ascending" : "Descending";
            if (emergency) directionLabel = $"Emergency {directionLabel}";
            Plugin.mls.LogInfo($"Elevator {directionLabel} from {elevatorFloor} to {floor}");

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
                float normalTime = animInfo.normalizedTime;
                currentNotice.color = Color.Lerp(MovingColor, CurrentColor, normalTime);
                oldNotice.color = Color.Lerp(CurrentColor, MovingColor, normalTime);
                return animHash != animInfo.shortNameHash && normalTime >= 0.98f;
            });

            elevatorMoving = false;
            controller.OpenDoor(elevatorFloor);
            currentNotice.color = CurrentColor;
            oldNotice.color = OffColor;

            yield return 0;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.2f);

            elevatorLink.activated = elevatorIdle = true;

            bool active = Operational;
            string text = active ? "Idle" : "\"No Power\"";

            foreach (ElevatorPannel pannel in controlPannels)
            {
                pannel.display.text = text;
                pannel.SetInteractable(active);
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
                RegisterPathfinding(true);
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
            RegisterPathfinding(true);
        }

        public float TimeToCompleteCurrentMovement()
        {
            if (elevatorIdle) return 0;
            return Vector3.Distance(controller.insidePos.position, controller.floorPos[elevatorFloor].position) / 5.8f;
        }

        public float TimeFromFloorToFloor(ElevatorFloor a, ElevatorFloor b)
        {
            if (a == b) return 0;
            return 4f;
        }
    }
}
