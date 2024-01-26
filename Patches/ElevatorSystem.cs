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

        public Animator[] panelAnimators;

        public bool performanceCheck;

        public bool isSetupEnd;

        void Start()
        {
            GameObject.Find("ElevatorMusic").AddComponent<ElevatorMusic>();
            ScanNodeProperties[] scanNodeProperties = GameObject.FindObjectsOfType<ScanNodeProperties>();
            if (Plugin.setKorean)
            {
                foreach (ScanNodeProperties scanNodeProperty in scanNodeProperties)
                {
                    if (scanNodeProperty.headerText == "Elevator Controller")
                    {
                        scanNodeProperty.headerText = "엘리베이터 조작기";
                    }
                }
            }
            if (GameObject.Find("ShrimpSpawn"))
            {
                RoundManager.Instance.SpawnEnemyOnServer(GameObject.Find("ShrimpSpawn").transform.position, 0f, 13);
                GameObject.Destroy(GameObject.Find("ShrimpSpawn"));
            }
        }

        void LateUpdate()
        {
            RoundMapSystem.Instance.isOffice = true;
            if (!RoundMapSystem.Instance.isOffice)
            {
                return;
            }
            
            if (!isSetupEnd)
            {
                animator = this.transform.GetChild(0).GetComponent<Animator>();
                audioSource = GameObject.Find("ElevatorSound").GetComponent<AudioSource>();
                doorAnimator = GameObject.Find("DoorBone").GetComponent<Animator>();
                elevatorScreenDoorAnimator = GameObject.Find("ElevatorScreenDoor").GetComponent<Animator>();
                panelAnimators = GameObject.FindObjectsOfType<Animator>();
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
                    {
                        panelAnimator.transform.GetChild(1).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(ElevatorGoUp);
                        panelAnimator.transform.GetChild(2).GetChild(0).GetComponent<InteractTrigger>().onInteract.AddListener(ElevatorGoDown);
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().font = GameObject.Find("doorHydraulics").GetComponent<TMP_Text>().font;
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().material = GameObject.Find("doorHydraulics").GetComponent<TMP_Text>().material;
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().color = new Color(1, 0.3444f, 0, 1);
                    }
                }
                GameObject.Find("InsideCollider").gameObject.AddComponent<ElevatorCollider>();
                GameObject.Find("OutOfBoundsTriggerFactory").transform.position = new Vector3(0, -800f, 0);

                isSetupEnd = true;
            }
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
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
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
        void ElevatorGoUp(PlayerControllerB playerController)
        {
            //elevatorSendUpEvent.InvokeAllClients();
            ElevatorUp();
        }

        void ElevatorGoDown(PlayerControllerB playerController)
        {
            //elevatorSendDownEvent.InvokeAllClients();
            ElevatorDown();
        }

        void ElevatorUp()
        {
            Plugin.mls.LogInfo("pressed up button!");
            if (isElevatorDowned.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
                    {
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Moving Up";
                    }
                }
                isElevatorDowned.Value = false;
            }
        }

        void ElevatorDown()
        {
            Plugin.mls.LogInfo("pressed down button!");
            if (!isElevatorDowned.Value && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                foreach (Animator panelAnimator in panelAnimators)
                {
                    if (panelAnimator.gameObject.name == "ElevatorPanel")
                    {
                        panelAnimator.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>().text = "Moving Down";
                    }
                }
                isElevatorDowned.Value = true;
            }
        }
    }
}
