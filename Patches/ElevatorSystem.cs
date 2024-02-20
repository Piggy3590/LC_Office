using GameNetcodeStuff;
using LethalNetworkAPI;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class ElevatorSystem : NetworkBehaviour
    {
        /*
        public LethalClientMessage<bool> SendElevator = new LethalClientMessage<bool>("SendElevator");
        public LethalClientMessage<bool> SendElevatorDoor = new LethalClientMessage<bool>("SendElevatorDoor");
        public LethalClientMessage<int> SetElevatorTimerInt = new LethalClientMessage<int>("SetElevatorTimerInt");
        */
        public static LethalNetworkVariable<int> elevatorFloor = new LethalNetworkVariable<int>(identifier: "elevatorFloor");

        public static bool isElevatorClosed;
        public float elevatorTimer;

        public static LethalNetworkVariable<bool> spawnShrimpBool = new LethalNetworkVariable<bool>(identifier: "spawnShrimpBool");

        public static LethalClientEvent ElevatorUpTriggerEvent = new LethalClientEvent(identifier: "elevatorUpTriggerEvent", onReceivedFromClient: ElevatorTriggerUp);
        public static LethalClientEvent ElevatorDownTriggerEvent = new LethalClientEvent(identifier: "elevatorDownTriggerEvent", onReceivedFromClient: ElevatorTriggerDown);

        public static List<InteractTrigger> upButtons = new List<InteractTrigger>();
        public static List<InteractTrigger> downButtons = new List<InteractTrigger>();

        public static Animator doorAnimator;
        public Animator elevatorScreenDoorAnimator;
        public static Animator animator;

        public AudioSource audioSource;

        public static List<Animator> panelAnimators = new List<Animator>();
        public List<Animator> buttonLightAnimators = new List<Animator>();

        public bool performanceCheck;
        public GameObject storageObject;
        public Transform storagePos;

        public static bool isSetupEnd;

        void Start()
        {
            spawnShrimpBool.Value = false;
            this.transform.position = new Vector3(0, 200, 0);
            isSetupEnd = false;
        }

        void LateUpdate()
        {
            if (!TimeOfDay.Instance.currentDayTimeStarted && base.IsServer)
            {
                if (panelAnimators.Count != 0)
                {
                    panelAnimators.Clear();
                }
                isSetupEnd = false;
                Destroy(this.gameObject);
            }else
            {
                Setup();
            }
            if (!OfficeRoundSystem.Instance.isOffice)
            {
                return;
            }

            //This code will only run once.
            
            if (storageObject == null)
            {
                storageObject = GameObject.Find("ElevatorStorage(Clone)");
            }else
            {
                storageObject.transform.position = storagePos.position;
            }


            if (storagePos == null)
            {
                storagePos = GameObject.Find("SpawnStorage").transform;
            }


            if (isElevatorClosed)
            {
                if (doorAnimator.GetBool("closed"))
                {
                    audioSource.PlayOneShot(Plugin.ElevatorClose);
                }
                elevatorScreenDoorAnimator.SetBool("open", false);
                doorAnimator.SetBool("closed", false);
            }
            else
            {
                if (!doorAnimator.GetBool("closed"))
                {
                    audioSource.PlayOneShot(Plugin.ElevatorOpen);
                }
                if (elevatorFloor.Value == 1 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.4f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                {
                    elevatorScreenDoorAnimator.SetBool("open", true);
                }
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.2f && !doorAnimator.GetBool("closed"))
                {
                    doorAnimator.SetBool("closed", true);
                    audioSource.PlayOneShot(Plugin.ElevatorOpen);
                }
                doorAnimator.SetBool("closed", true);
            }

            if (elevatorFloor.Value < animator.GetInteger("floor"))
            {
                elevatorTimer += Time.deltaTime;
                if (elevatorTimer > 0)
                {
                    if (!isElevatorClosed)
                    {
                        isElevatorClosed = true;
                    }
                    if (elevatorTimer > 3f)
                    {
                        if (animator.GetInteger("floor") == 2)
                        {
                            animator.SetInteger("floor", 1);
                            foreach (Animator buttonLightAnimator in buttonLightAnimators)
                            {
                                buttonLightAnimator.SetInteger("sta", 1);
                            }
                        }else
                        {
                            animator.SetInteger("floor", 0);
                            foreach (Animator buttonLightAnimator in buttonLightAnimators)
                            {
                                buttonLightAnimator.SetInteger("sta", 0);
                            }
                        }
                        audioSource.PlayOneShot(Plugin.ElevatorDown);
                        elevatorFloor.Value = animator.GetInteger("floor");
                    }
                }
            }

            if (elevatorFloor.Value > animator.GetInteger("floor"))
            {
                foreach (Animator buttonLightAnimator in buttonLightAnimators)
                {
                    buttonLightAnimator.SetBool("up", true);
                }
                elevatorTimer += Time.deltaTime;
                if (elevatorTimer > 0)
                {
                    isElevatorClosed = true;
                    if (elevatorTimer > 3f)
                    {
                        if (animator.GetInteger("floor") == 0)
                        {
                            animator.SetInteger("floor", 1);
                            foreach (Animator buttonLightAnimator in buttonLightAnimators)
                            {
                                buttonLightAnimator.SetInteger("sta", 1);
                            }
                        }
                        else
                        {
                            animator.SetInteger("floor", 2);
                            foreach (Animator buttonLightAnimator in buttonLightAnimators)
                            {
                                buttonLightAnimator.SetInteger("sta", 2);
                            }
                        }
                        audioSource.PlayOneShot(Plugin.ElevatorUp);
                        animator.SetBool("goDown", false);
                        elevatorFloor.Value = animator.GetInteger("floor");
                    }
                }
            }

            if (isElevatorClosed && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f)
            {
                panelAnimators.RemoveAll((Animator item) => item == null || !item.gameObject.activeInHierarchy);
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>() != null)
                    {
                        if (Plugin.setKorean)
                        {
                            panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "대기 중";
                        }else
                        {
                            panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Idle";
                        }
                    }
                }
                if (isElevatorClosed)
                {
                    isElevatorClosed = false;
                    elevatorTimer = 0;
                }
            }

            foreach (InteractTrigger upButton in upButtons)
            {
                if (!upButton.interactable && ElevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.2f)
                {
                    upButton.interactable = true;
                }
                else if (upButton.interactable && ElevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.2f)
                {
                    upButton.interactable = false;
                }
            }
            foreach (InteractTrigger downButton in downButtons)
            {
                if (!downButton.interactable && ElevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.2f)
                {
                    downButton.interactable = true;
                }
                else if (downButton.interactable && ElevatorSystem.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.2f)
                {
                    downButton.interactable = false;
                }
            }
        }

        void Setup()
        {
            if (!isSetupEnd)
            {
                animator = GameObject.Find("StartRoomElevator").GetComponent<Animator>();
                audioSource = GameObject.Find("ElevatorSound").GetComponent<AudioSource>();
                doorAnimator = GameObject.Find("DoorBone").GetComponent<Animator>();
                doorAnimator.SetFloat("speed", 1);
                elevatorScreenDoorAnimator = GameObject.Find("ElevatorScreenDoor").GetComponent<Animator>();
                Animator[] animators = GameObject.FindObjectsOfType<Animator>();
                foreach (Animator panelAnimator in animators)
                {
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(ElevatorUpEvent);
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(ElevatorDownEvent);
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().font = GameObject.Find("doorHydraulics").GetComponent<TMP_Text>().font;
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().material = GameObject.Find("doorHydraulics").GetComponent<TMP_Text>().material;
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().color = new Color(1, 0.3444f, 0, 1);
                        if (!PlaceLung.emergencyPowerRequires || PlaceLung.lungPlaced)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "대기 중";
                            }
                            else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Idle";
                            }
                        }else if (PlaceLung.emergencyPowerRequires && !PlaceLung.lungPlaced)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "전력 없음";
                            }
                            else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "No Power";
                            }
                        }
                        panelAnimators.Add(panelAnimator);
                        upButtons.Add(panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>());
                        downButtons.Add(panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>());
                    }
                }

                foreach (Animator animator in animators)
                {
                    if (animator.gameObject.name == "ButtonLights")
                    {
                        buttonLightAnimators.Add(animator);
                    }
                }
                GameObject.Find("ElevatorCamera").AddComponent<CameraCulling>();
                GameObject.Find("FacilityCamera").AddComponent<CameraCulling>();
                //GameObject.Find("OutOfBoundsTriggerFactory").transform.position = new Vector3(0, -800f, 0);

                GameObject.Find("ElevatorMusic").AddComponent<ElevatorMusic>();
                ScanNodeProperties[] scanNodeProperties = GameObject.FindObjectsOfType<ScanNodeProperties>();
                if (Plugin.setKorean)
                {
                    foreach (ScanNodeProperties scanNodeProperty in scanNodeProperties)
                    {
                        if (scanNodeProperty.headerText == "Elevator Controller")
                        {
                            scanNodeProperty.headerText = "엘리베이터 제어기";
                        }
                        if (scanNodeProperty.headerText == "Auxiliary power unit")
                        {
                            scanNodeProperty.headerText = "보조 동력 장치";
                            scanNodeProperty.subText = "긴급 전력 공급기";
                        }
                    }
                }

                elevatorFloor.Value = 1;
                isElevatorClosed = false;
                spawnShrimpBool.Value = false;
                
                isSetupEnd = true;
            }
        }

        public static void ElevatorUpEvent(PlayerControllerB playerController)
        {
            if (PlaceLung.emergencyPowerRequires)
            {
                if (PlaceLung.lungPlaced)
                {
                    ElevatorUpTriggerEvent.InvokeAllClients();
                }
            }
            else
            {
                ElevatorUpTriggerEvent.InvokeAllClients();
            }
        }
        public static void ElevatorDownEvent(PlayerControllerB playerController)
        {
            if (PlaceLung.emergencyPowerRequires)
            {
                if (PlaceLung.lungPlaced)
                {
                    ElevatorDownTriggerEvent.InvokeAllClients();
                }
            }else
            {
                ElevatorDownTriggerEvent.InvokeAllClients();
            }
        }

        public static void ElevatorTriggerUp(ulong id) { ElevatorUpTrigger(); }
        public static void ElevatorTriggerDown(ulong id) { ElevatorDownTrigger(); }

        public static void ElevatorUpTrigger()
        {
            if (elevatorFloor.Value < 2)
            {
                elevatorFloor.Value += 1;
            }
            if (doorAnimator.GetBool("closed") && animator.GetInteger("floor") < elevatorFloor.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>() != null)
                    {
                        if (!PlaceLung.emergencyPowerRequires || PlaceLung.lungPlaced)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "상승 중";
                            }
                            else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Ascending";
                            }
                        }
                        else if (PlaceLung.emergencyPowerRequires && !PlaceLung.lungPlaced)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "전력 없음";
                            }
                            else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "No Power";
                            }
                        }
                    }    
                }
            }
        }

        public static void ElevatorDownTrigger()
        {
            if (elevatorFloor.Value > 0)
            {
                elevatorFloor.Value -= 1;
            }
            if (doorAnimator.GetBool("closed") && animator.GetInteger("floor") > elevatorFloor.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>() != null)
                    {
                        if (!PlaceLung.emergencyPowerRequires || PlaceLung.lungPlaced)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "하강 중";
                            }else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Descending";
                            }
                        }
                        else if (PlaceLung.emergencyCheck)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "긴급 하강 중";
                            }
                            else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Emergency\nDescending";
                            }
                        }
                        else if (PlaceLung.emergencyPowerRequires && !PlaceLung.lungPlaced)
                        {
                            if (Plugin.setKorean)
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "전력 없음";
                            }
                            else
                            {
                                panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "No Power";
                            }
                        }
                    }
                }
            }
        }
    }
}
