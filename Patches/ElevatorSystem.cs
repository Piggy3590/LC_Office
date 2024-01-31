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
        public static LethalNetworkVariable<bool> isElevatorDowned = new LethalNetworkVariable<bool>(identifier: "isElevatorDowned");
        public static LethalNetworkVariable<bool> isElevatorClosed = new LethalNetworkVariable<bool>(identifier: "isElevatorClosed");
        public float elevatorTimer;

        public static LethalNetworkVariable<bool> spawnShrimpBool = new LethalNetworkVariable<bool>(identifier: "spawnShrimpBool");

        public Animator doorAnimator;
        public Animator elevatorScreenDoorAnimator;
        public Animator animator;

        public AudioSource audioSource;

        public List<Animator> panelAnimators;
        public List<Animator> buttonLightAnimators;

        public bool performanceCheck;
        public GameObject storageObject;

        public bool isSetupEnd;

        void Start()
        {
            isElevatorDowned.Value = false;
            isElevatorClosed.Value = false;
            elevatorScreenDoorAnimator.SetBool("open", true);
            spawnShrimpBool.Value = false;
        }

        void LateUpdate()
        {
            if (!RoundMapSystem.Instance.isOffice)
            {
                return;
            }

            //This code will only run once.
            Setup();

            /*
            storageObject.transform.SetParent(animator.transform);
            storageObject.transform.position = GameObject.Find("TestClosetPos").transform.position;
            storageObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            */

            if (isElevatorClosed.Value)
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
                if (!isElevatorDowned.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.4f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                {
                    elevatorScreenDoorAnimator.SetBool("open", true);
                }
                doorAnimator.SetBool("closed", true);
            }

            if (isElevatorDowned.Value && !animator.GetBool("goDown"))
            {
                elevatorTimer += Time.deltaTime;
                foreach (Animator buttonLightAnimator in buttonLightAnimators)
                {
                    buttonLightAnimator.SetBool("up", false);
                }
                if (elevatorTimer > 0)
                {
                    if (!isElevatorClosed.Value)
                    {
                        isElevatorClosed.Value = true;
                    }
                    if (elevatorTimer > 3f)
                    {
                        audioSource.PlayOneShot(Plugin.ElevatorDown);
                        animator.SetBool("goDown", true);
                        if (!isElevatorDowned.Value)
                        {
                            isElevatorDowned.Value = true;
                        }
                    }
                }
            }

            if (!isElevatorDowned.Value && animator.GetBool("goDown"))
            {
                foreach (Animator buttonLightAnimator in buttonLightAnimators)
                {
                    buttonLightAnimator.SetBool("up", true);
                }
                elevatorTimer += Time.deltaTime;
                if (elevatorTimer > 0)
                {
                    isElevatorClosed.Value = true;
                    if (elevatorTimer > 3f)
                    {
                        audioSource.PlayOneShot(Plugin.ElevatorUp);
                        animator.SetBool("goDown", false);
                        if (!isElevatorDowned.Value)
                        {
                            isElevatorDowned.Value = false;
                        }
                    }
                }
            }

            if (isElevatorClosed.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f)
            {
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (Plugin.setKorean)
                    {
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "대기 중";
                    }else
                    {
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Idle";
                    }
                }
                if (isElevatorClosed.Value)
                {
                    isElevatorClosed.Value = false;
                    elevatorTimer = 0;
                }
            }
        }

        void Setup()
        {
            if (!isSetupEnd)
            {
                animator = this.transform.GetChild(0).GetComponent<Animator>();
                audioSource = GameObject.Find("ElevatorSound").GetComponent<AudioSource>();
                doorAnimator = GameObject.Find("DoorBone").GetComponent<Animator>();
                elevatorScreenDoorAnimator = GameObject.Find("ElevatorScreenDoor").GetComponent<Animator>();
                Animator[] animators = GameObject.FindObjectsOfType<Animator>();
                foreach (Animator panelAnimator in animators)
                {
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(ElevatorUpTrigger);
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(ElevatorDownTrigger);
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
                    }
                }

                foreach (Animator animator in animators)
                {
                    if (animator.gameObject.name == "ButtonLights")
                    {
                        buttonLightAnimators.Add(animator);
                    }
                }
                GameObject.Find("InsideCollider").gameObject.AddComponent<ElevatorCollider>();
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

                if (GameNetworkManager.Instance.isHostingGame)
                {
                    GameObject socketInteractObject = GameObject.Instantiate(Plugin.socketInteractPrefab);
                    socketInteractObject.GetComponent<NetworkObject>().Spawn();
                }

                isSetupEnd = true;
                isElevatorClosed.Value = false;
            }
        }
        public void ElevatorUpTrigger(PlayerControllerB playerController)
        {
            Plugin.mls.LogInfo("pressed up button!");
            if (isElevatorDowned.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                foreach (Animator panelAnimator in panelAnimators)
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
                isElevatorDowned.Value = false;
            }
        }

        public void ElevatorDownTrigger(PlayerControllerB playerController)
        {
            Plugin.mls.LogInfo("pressed down button!");
            if (!isElevatorDowned.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                foreach (Animator panelAnimator in panelAnimators)
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
                isElevatorDowned.Value = true;
            }
        }
    }
}
