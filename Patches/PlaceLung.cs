using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace LCOffice.Patches
{
    public class PlaceLung : NetworkBehaviour
    {
        public static bool emergencyPowerRequires;
        public static bool emergencyCheck;
        public static bool lungPlaced;
        /*
        public static LethalClientEvent lungPlacedFrameNetwork = new LethalClientEvent(identifier: "LungPlaced", onReceivedFromClient: ReceivedPlacedFrameNetwork);
        public static bool lungPlacedThisFrame;
        public static bool placeLungNetwork;

        //public static LethalClientMessage<PlayerControllerB> lungPlacer = new LethalClientMessage<PlayerControllerB>(identifier: "Installer", onReceivedFromClient: ReceiveLungPlacer);
        //public static PlayerControllerB localLungPlacer;
        [PublicNetworkVariable]
        public static LethalNetworkVariable<PlayerControllerB> lungPlacer = new LethalNetworkVariable<PlayerControllerB>(identifier: "LungPlacer");
        public static LethalNetworkVariable<LungProp> placedLung = new LethalNetworkVariable<LungProp>(identifier: "PlacedLungVar");

        public static bool isPlaceCalled;

        public static bool lungPlacedLocalFrame;
        public static NetworkObject networkObject;

        public static bool isSetupEnd;

        public static ElevatorSystem elevatorSystem;
        public static Animator socketLED;
        public static AudioSource socketAudioSource;
        public static InteractTrigger socketInteractTrigger;
        public static Transform lungPos;
        public static Transform lungSocket;
        public static Transform lungParent;
        public static Animator lungParentAnimator;

        public static AudioSource elevatorMusic;
        public static AudioSource elevatorSound;
        public static Animator elevatorAnimator;
        public static List<Animator> panelAnimators = new List<Animator>();

        public static List<InteractTrigger> upButtons = new List<InteractTrigger>();
        public static List<InteractTrigger> downButtons = new List<InteractTrigger>();


        public bool isForceMoving;

        public static BoxCollider boxCollider;

        public List<Light> elevatorLights = new List<Light>();

        public static void ReceiveLungPlacer(PlayerControllerB data, ulong clientId)
        {
            //localLungPlacer = data;
        }

        public static void ReceivedPlacedFrameNetwork(ulong clientId)
        {
            lungPlacedThisFrame = true;
        }
        void Start()
        {
            lungPlacedThisFrame = false;
            placeLungNetwork = false;
            lungPlacer.Value = null;
            placedLung.Value = null;
            emergencyPowerRequires = false;
            emergencyCheck = false;
        }

        void LateUpdate()
        {
            if (StartOfRound.Instance.inShipPhase)
            {
                Destroy(this.gameObject);
            }
            if (!OfficeRoundSystem.Instance.isOffice)
            {
                return;
            }
            if (!ElevatorSystem.isSetupEnd)
            {
                return;
            }
            Setup();

            this.transform.position = lungPos.position;
            this.transform.rotation = lungPos.rotation;

            if (emergencyCheck && !emergencyPowerRequires && !lungPlaced)
            {
                if (ElevatorSystem.animator.GetInteger("floor") >= 0)
                {
                    ElevatorSystem.ElevatorDownEvent(StartOfRound.Instance.localPlayerController);
                }
            }
            if (emergencyCheck && !emergencyPowerRequires)
            {
                ElevatorSystem.isElevatorClosed = false;
                elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 0, Time.deltaTime * 2.5f);
                if ((ElevatorSystem.doorAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && ElevatorSystem.doorAnimator.GetBool("closed"))
                    && (ElevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && ElevatorSystem.animator.GetBool("goDown"))
                    && elevatorAnimator.gameObject.transform.localPosition.y <= -9.6998f)
                {
                    emergencyPowerRequires = true;
                    emergencyCheck = false;
                }
            }

            CheckPlayerHoldingLung();

            if (emergencyPowerRequires)
            {
                if (lungPlaced)
                {
                    elevatorAnimator.SetFloat("speed", Mathf.Lerp(elevatorAnimator.GetFloat("speed"), 1, Time.deltaTime * 3f));
                    elevatorSound.pitch = Mathf.Lerp(elevatorSound.pitch, 1, Time.deltaTime * 3f);
                    elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 1f, Time.deltaTime * 3.5f);
                }
                else
                {
                    elevatorAnimator.SetFloat("speed", Mathf.Lerp(elevatorAnimator.GetFloat("speed"), 0, Time.deltaTime * 3f));
                    elevatorSound.pitch = Mathf.Lerp(elevatorSound.pitch, 0, Time.deltaTime * 3f);
                    elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 0, Time.deltaTime * 2.5f);
                }
            }
            else
            {
                elevatorAnimator.SetFloat("speed", Mathf.Lerp(elevatorAnimator.GetFloat("speed"), 1, Time.deltaTime * 3f));
                elevatorSound.pitch = Mathf.Lerp(elevatorSound.pitch, 1, Time.deltaTime * 3f);
                elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 1f, Time.deltaTime * 3.5f);
            }
        }

        void Setup()
        {
            if (!isSetupEnd)
            {
                elevatorSystem = this.GetComponent<ElevatorSystem>();
                socketAudioSource = this.GetComponent<AudioSource>();
                socketInteractTrigger = this.GetComponent<InteractTrigger>();
                socketInteractTrigger.onInteract.AddListener(PlaceApparatusEvent);
                socketLED = GameObject.Find("SocketLED").GetComponent<Animator>();
                boxCollider = this.GetComponent<BoxCollider>();
                lungPos = GameObject.Find("LungPos").transform;
                lungSocket = GameObject.Find("LungSocket").transform;
                elevatorMusic = GameObject.Find("ElevatorMusic").GetComponent<AudioSource>();
                elevatorSound = GameObject.Find("ElevatorSound").GetComponent<AudioSource>();
                elevatorAnimator = GameObject.Find("StartRoomElevator").GetComponent<Animator>();

                elevatorLights.Add(GameObject.Find("ElevatorLight1").GetComponent<Light>());
                elevatorLights.Add(GameObject.Find("ElevatorLight2").GetComponent<Light>());
                lungParent = GameObject.Find("LungParent").transform;
                lungParentAnimator = lungParent.GetComponent<Animator>();
                Animator[] animators = GameObject.FindObjectsOfType<Animator>();
                foreach (Animator panelAnimator in animators)
                {
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
                    {
                        upButtons.Add(panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>());
                        downButtons.Add(panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>());
                    }
                }
                networkObject = this.NetworkObject;
                isSetupEnd = true;
            }
        }

        void CheckPlayerHoldingLung()
        {
            if (StartOfRound.Instance.localPlayerController.isHoldingObject)
            {
                if (StartOfRound.Instance.localPlayerController.currentlyGrabbingObject.GetComponent<LungProp>() != null)
                {
                    socketInteractTrigger.interactable = true;
                }
                else
                {
                    socketInteractTrigger.interactable = false;
                }
            }
            else
            {
                socketInteractTrigger.interactable = false;
            }
        }

        public void PlaceApparatusEvent(PlayerControllerB playerControllerB)
        {
            if (playerControllerB.currentlyGrabbingObject.GetComponent<LungProp>() != null && playerControllerB.isHoldingObject)
            {
                //placeLungEvent.SendAllClients(playerControllerB);
                PlaceApparatusLocal(playerControllerB);
            }
        }
        public void PlaceApparatusLocal(PlayerControllerB data)
        {
            Plugin.mls.LogInfo("PlaceApparatus triggered by " + data.gameObject.name);
            if (data.currentlyGrabbingObject.GetComponent<LungProp>() != null && data.isHoldingObject)
            {
                Plugin.mls.LogInfo(data.gameObject.name + " is placed apparatus");
                data.DiscardHeldObject(true, networkObject, networkObject.transform.position, true);
                data.currentlyGrabbingObject.transform.SetParent(PlaceLung.lungParent);
                data.GetComponent<ItemElevatorCheck>().ignoreCollider = true;
                data.currentlyGrabbingObject.GetComponent<PlaceableLungProp>().isPlaced = true;
                lungParentAnimator.SetTrigger("LungPlaced");
                socketAudioSource.PlayOneShot(data.currentlyGrabbingObject.GetComponent<LungProp>().connectSFX);
            }
        }

        void RemoveLung()
        {
            if (placedLung.Value.isHeld)
            {
                lungPlaced = false;
                isPlaceCalled = false;
                PlaceLung.placedLung.Value = null;
            }
        }
        [HarmonyPatch(typeof(LungProp))]
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Patch(LungProp __instance)
        {
            __instance.gameObject.AddComponent<PlaceableLungProp>();
        }
        [HarmonyPatch(typeof(LungProp))]
        [HarmonyPrefix]
        [HarmonyPatch("EquipItem")]
        private static void EquipItem_Patch(ref bool ___isLungDocked)
        {
            if (OfficeRoundSystem.Instance != null && OfficeRoundSystem.Instance.isOffice)
            {
                if (___isLungDocked)
                {
                    Animator socketLed = GameObject.Find("SocketLED").GetComponent<Animator>();
                    socketLed.SetBool("on", true);
                    ElevatorSystem elevatorSystem = GameObject.FindObjectOfType<ElevatorSystem>();
                    PlaceLung lungPlacement = GameObject.FindObjectOfType<PlaceLung>();
                    foreach (Animator panelAnimator in ElevatorSystem.panelAnimators)
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                        if (!PlaceLung.lungPlaced)
                        {
                            GameObject.Find("ElevatorLight1").GetComponent<Light>().enabled = false;
                            GameObject.Find("ElevatorLight2").GetComponent<Light>().enabled = false;
                        }
                        socketLed.SetBool("warning", true);
                        PlaceLung.emergencyCheck = true;
                    }
                }
            }
        }
        */
    }
}
