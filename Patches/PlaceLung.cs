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
        public static LethalNetworkVariable<bool> lungPlacedThisFrame = new LethalNetworkVariable<bool>(identifier: "lungPlacedThisFrame");

        public static LethalNetworkVariable<bool> placeLungNetwork = new LethalNetworkVariable<bool>(identifier: "placeLung");
        public static LethalNetworkVariable<ulong> lungPlacer = new LethalNetworkVariable<ulong>(identifier: "Installer");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<GameObject> placedLung = new LethalNetworkVariable<GameObject>(identifier: "placedLung");
        
        public LethalClientEvent onLungPlace = new LethalClientEvent(identifier: "onLungPlace");
        public LethalClientMessage<bool> onLungPlaced = new LethalClientMessage<bool>("onLungPlaced");
        public LethalClientMessage<bool> onLungRemoved = new LethalClientMessage<bool>("onLungRemoved");
        public bool isPlaceCalled;

        public bool isOtherClientPlaceChecked;
        public bool isOtherClientRemoveChecked;

        public static bool lungPlaced;
        public static bool lungPlacedLocalFrame;
        public static bool emergencyPowerRequires;
        public static bool emergencyCheck;
        
        public bool isSetupEnd;

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

        public LungProp lungComponent;

        public bool isForceMoving;

        public BoxCollider boxCollider;

        public List<Light> elevatorLights = new List<Light>();

        void Start()
        {
            lungPlacedThisFrame.Value = false;
            placeLungNetwork.Value = false;
            lungPlacer.Value = 0;
            placedLung.Value = null;
        }

        void LateUpdate()
        {
            if (!RoundMapSystem.Instance.isOffice)
            {
                return;
            }
            Setup();

            this.transform.position = lungPos.position;
            this.transform.rotation = lungPos.rotation;
            //this.transform.position = Vector3.Lerp(this.transform.position, lungPos.position, Time.deltaTime * 7);
            //this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lungPos.rotation, Time.deltaTime * 7);
            if (emergencyCheck && !emergencyPowerRequires && !lungPlaced)
            {
                if (!ElevatorSystem.isElevatorDowned.Value)
                {
                    elevatorSystem.ElevatorDownTrigger(StartOfRound.Instance.localPlayerController);
                }
            }
            if (emergencyCheck && !emergencyPowerRequires)
            {
                if (elevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && elevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && elevatorSystem.animator.GetBool("goDown"))
                {
                    ElevatorSystem.isElevatorClosed.Value = false;
                    emergencyPowerRequires = true;
                    emergencyCheck = false;
                }
            }

            if (lungPlaced)
            {
                lungComponent = placedLung.Value.GetComponent<LungProp>();
                boxCollider.size = new Vector3(0, 0, 0);
                placedLung.Value.transform.SetParent(lungParent);
                placedLung.Value.transform.localPosition = new Vector3(0, 0, 0);
                placedLung.Value.transform.localRotation = Quaternion.Euler(0, 90, 0);
                if (lungPlacedLocalFrame)
                {
                    foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                    {
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(elevatorSystem.ElevatorDownTrigger);
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(elevatorSystem.ElevatorUpTrigger);
                    }
                    lungPlacedLocalFrame = false;
                }
            }
            else
            {
                if (emergencyPowerRequires)
                {
                    foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                    }
                }
                lungPlacedLocalFrame = true;
                boxCollider.size = new Vector3(2, 1.6f, 2);
                lungComponent = null;
            }

            CheckPlayerHoldingLung();
            if (lungPlaced)
            {
                RemoveLung();
            }
            if (lungPlacedThisFrame.Value)
            {
                LungPlacement();
            }

            if (isOtherClientPlaceChecked)
            {
                OnLungPlacedThisFrameOtherClient();
            }
            if (isOtherClientRemoveChecked)
            {
                RemoveLungOtherClient();
            }

            if (emergencyPowerRequires)
            {
                if (lungPlaced)
                {
                    elevatorAnimator.SetFloat("speed", Mathf.Lerp(elevatorAnimator.GetFloat("speed"), 1, Time.deltaTime * 3f));
                    elevatorSystem.doorAnimator.SetFloat("speed", elevatorAnimator.GetFloat("speed"));
                    elevatorSound.pitch = Mathf.Lerp(elevatorMusic.pitch, 1, Time.deltaTime * 3f);
                    elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 1, Time.deltaTime * 3.5f);
                }
                else
                {
                    elevatorAnimator.SetFloat("speed", Mathf.Lerp(elevatorAnimator.GetFloat("speed"), 0, Time.deltaTime * 3f));
                    elevatorSystem.doorAnimator.SetFloat("speed", elevatorAnimator.GetFloat("speed"));
                    elevatorSound.pitch = Mathf.Lerp(elevatorMusic.pitch, 0, Time.deltaTime * 3f);
                    elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 0, Time.deltaTime * 2.5f);
                }
                emergencyCheck = false;
            }else
            {
                elevatorAnimator.SetFloat("speed", Mathf.Lerp(elevatorAnimator.GetFloat("speed"), 1, Time.deltaTime * 3f));
                elevatorSystem.doorAnimator.SetFloat("speed", elevatorAnimator.GetFloat("speed"));
                elevatorSound.pitch = Mathf.Lerp(elevatorMusic.pitch, 1, Time.deltaTime * 3f);
                elevatorMusic.pitch = Mathf.Lerp(elevatorMusic.pitch, 1, Time.deltaTime * 3.5f);
            }
        }

        void Setup()
        {
            if (!isSetupEnd)
            {
                elevatorSystem = GameObject.FindObjectOfType<ElevatorSystem>();
                socketAudioSource = this.GetComponent<AudioSource>();
                socketInteractTrigger = this.GetComponent<InteractTrigger>();
                socketInteractTrigger.onInteract.AddListener(PlaceApparatus);
                socketLED = GameObject.Find("SocketLED").GetComponent<Animator>();
                boxCollider = this.GetComponent<BoxCollider>();
                lungPos = GameObject.Find("LungPos").transform;
                lungSocket = GameObject.Find("LungSocket").transform;
                elevatorMusic = GameObject.Find("ElevatorMusic").GetComponent<AudioSource>();
                elevatorSound = GameObject.Find("ElevatorSound").GetComponent<AudioSource>();
                elevatorAnimator = GameObject.FindObjectOfType<ElevatorSystem>().animator;

                elevatorLights.Add(GameObject.Find("ElevatorLight1").GetComponent<Light>());
                elevatorLights.Add(GameObject.Find("ElevatorLight2").GetComponent<Light>());
                lungParent = GameObject.Find("LungParent").transform;
                lungParentAnimator = lungParent.GetComponent<Animator>();

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

        void LungPlacement()
        {
            onLungPlaced.OnReceivedFromClient += OnLungPlacedThisFrame;
            Plugin.mls.LogInfo("lungPlacedThisFrame.Value is true");
            if (!isPlaceCalled)
            {

                PlayerControllerB player = lungPlacer.Value.GetPlayerController();
                socketLED.SetBool("warning", false);
                Plugin.mls.LogInfo("lungPlacedThisFrame is enabled");
                /*
                GameObject newSparkParticle = GameObject.Instantiate<GameObject>
                    (StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.GetComponent<LungProp>().sparkParticle,
                    StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.transform.position, Quaternion.identity, null);
                */
                placedLung.Value = player.currentlyGrabbingObject.gameObject;
                socketAudioSource.PlayOneShot(player.currentlyGrabbingObject.GetComponent<LungProp>().connectSFX);
                if (emergencyPowerRequires)
                {
                    foreach (Light elevatorLight in elevatorLights)
                    {
                        elevatorLight.enabled = true;
                    }
                    foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(elevatorSystem.ElevatorUpTrigger);
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(elevatorSystem.ElevatorDownTrigger);
                    }
                }
                //placedLung.startFallingPosition = placedLung.transform.InverseTransformPoint(this.transform.position);
                player.DiscardHeldObject(true, this.NetworkObject, this.transform.position, true);
                lungPlaced = true;
                lungComponent = placedLung.Value.GetComponent<LungProp>();
                placedLung.Value.transform.SetParent(lungParent);
                placedLung.Value.transform.localPosition = new Vector3(0, 0, 0);
                placedLung.Value.transform.localRotation = Quaternion.Euler(0, 90, 0);
                isPlaceCalled = true;
                lungParentAnimator.SetTrigger("LungPlaced");
                if (!lungPlacer.Value.GetPlayerController().isCameraDisabled)
                {
                    onLungPlaced.SendAllClients(true, false);
                }
                lungPlacedThisFrame.Value = false;
            }
        }

        void OnLungPlacedThisFrame(bool value, ulong id)
        {
            if (isOtherClientPlaceChecked != value)
            {
                isOtherClientPlaceChecked = value;
            }
        }

        void OnLungPlacedThisFrameOtherClient()
        {
            if (!isPlaceCalled)
            {
                PlayerControllerB player = lungPlacer.Value.GetPlayerController();
                socketLED.SetBool("warning", false);
                Plugin.mls.LogInfo("lungPlacedThisFrame is enabled");
                /*
                GameObject newSparkParticle = GameObject.Instantiate<GameObject>
                    (StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.GetComponent<LungProp>().sparkParticle,
                    StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.transform.position, Quaternion.identity, null);
                */
                
                //placedLung.Value = player.currentlyGrabbingObject.gameObject;

                socketAudioSource.PlayOneShot(placedLung.Value.GetComponent<LungProp>().connectSFX);
                if (emergencyPowerRequires)
                {
                    foreach (Light elevatorLight in elevatorLights)
                    {
                        elevatorLight.enabled = true;
                    }
                    foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(elevatorSystem.ElevatorUpTrigger);
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(elevatorSystem.ElevatorDownTrigger);
                    }
                }
                lungPlaced = true;
                lungComponent = placedLung.Value.GetComponent<LungProp>();
                placedLung.Value.transform.SetParent(lungParent);
                placedLung.Value.transform.localPosition = new Vector3(0, 0, 0);
                placedLung.Value.transform.localRotation = Quaternion.Euler(0, 90, 0);
                boxCollider.size = new Vector3(0, 0, 0);
                lungPlacedThisFrame.Value = false;
                isPlaceCalled = true;
                lungParentAnimator.SetTrigger("LungPlaced");
                isOtherClientPlaceChecked = false;
            }
        }

        void PlaceApparatus(PlayerControllerB playerControllerB)
        {
            Plugin.mls.LogInfo("PlaceApparatus triggered by " + playerControllerB.gameObject.name);
            if (playerControllerB.currentlyGrabbingObject.GetComponent<LungProp>() != null && playerControllerB.isHoldingObject)
            {
                Plugin.mls.LogInfo(playerControllerB.gameObject.name + " is placed LungProp");
                lungPlacedThisFrame.Value = true;
                lungPlacer.Value = playerControllerB.actualClientId;
            }
        }

        void RemoveLung()
        {
            onLungRemoved.OnReceivedFromClient += OnLungRemovedThisFrame;
            /*
            lungComponent.startFallingPosition = lungComponent.transform.parent.InverseTransformPoint(this.transform.position);
            lungComponent.targetFloorPosition = lungComponent.transform.parent.InverseTransformPoint(this.transform.position);
            */
            if (lungComponent.isHeld)
            {
                foreach (Light elevatorLight in elevatorLights)
                {
                    elevatorLight.enabled = false;
                }
                socketLED.SetBool("warning", true);
                if (emergencyPowerRequires)
                {
                    foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                    }
                }
                /*
                GameObject newSparkParticle = GameObject.Instantiate<GameObject>
                    (StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.GetComponent<LungProp>().sparkParticle,
                    StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.transform.position, Quaternion.identity, null);
                */
                socketAudioSource.PlayOneShot(placedLung.Value.GetComponent<LungProp>().disconnectSFX);
                lungPlaced = false;
                isPlaceCalled = false;
                onLungRemoved.SendAllClients(true, false);
            }
        }

        void OnLungRemovedThisFrame(bool value, ulong id)
        {
            if (isOtherClientRemoveChecked != value)
            {
                isOtherClientRemoveChecked = value;
            }
        }

        void RemoveLungOtherClient()
        {
            if (lungPlaced)
            {
                /*
                lungComponent.startFallingPosition = lungComponent.transform.parent.InverseTransformPoint(this.transform.position);
                lungComponent.targetFloorPosition = lungComponent.transform.parent.InverseTransformPoint(this.transform.position);
                */
                if (lungComponent.isHeld)
                {
                    foreach (Light elevatorLight in elevatorLights)
                    {
                        elevatorLight.enabled = false;
                    }
                    socketLED.SetBool("warning", true);
                    if (emergencyPowerRequires)
                    {
                        foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                        {
                            panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                            panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                        }

                    }
                    /*
                    GameObject newSparkParticle = GameObject.Instantiate<GameObject>
                        (StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.GetComponent<LungProp>().sparkParticle,
                        StartOfRound.Instance.allPlayerScripts[lungPlacer.Value].currentlyGrabbingObject.transform.position, Quaternion.identity, null);
                    */
                    socketAudioSource.PlayOneShot(placedLung.Value.GetComponent<LungProp>().disconnectSFX);
                    lungPlaced = false;
                    isPlaceCalled = false;
                }
            }
        }

        IEnumerator LightFlickerOn()
        {
            foreach (Light elevatorLight in elevatorLights)
            {
                elevatorLight.enabled = true;
                yield return new WaitForSeconds(0.05f);
                elevatorLight.enabled = false;
                yield return new WaitForSeconds(0.05f);
                elevatorLight.enabled = true;
                yield return new WaitForSeconds(0.05f);
                elevatorLight.enabled = false;
                yield return new WaitForSeconds(0.05f);
                elevatorLight.enabled = true;
                yield return new WaitForSeconds(2f);
                yield break;
            }
        }

        [HarmonyPatch(typeof(LungProp))]
        [HarmonyPrefix]
        [HarmonyPatch("EquipItem")]
        private static void EquipItem_Patch(ref bool ___isLungDocked)
        {
            if (___isLungDocked)
            {
                Animator socketLed = GameObject.Find("SocketLED").GetComponent<Animator>();
                socketLed.SetBool("on", true); 
                ElevatorSystem elevatorSystem = GameObject.FindObjectOfType<ElevatorSystem>();
                PlaceLung lungPlacement = GameObject.FindObjectOfType<PlaceLung>();
                foreach (Animator panelAnimator in elevatorSystem.panelAnimators)
                {
                    panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                    panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.RemoveAllListeners();
                    if (!PlaceLung.lungPlaced)
                    {
                        GameObject.Find("ElevatorLight1").GetComponent<Light>().enabled = false;
                        GameObject.Find("ElevatorLight2").GetComponent<Light>().enabled = false;
                        socketLed.SetBool("warning", true);
                    }

                    PlaceLung.emergencyCheck = true;
                }
            }
        }
    }
}
