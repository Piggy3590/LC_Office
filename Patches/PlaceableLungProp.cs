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
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
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
    public class PlaceableLungProp : MonoBehaviour
    {
        /*
        public bool isSetupEnd;
        public bool isPlaced;
        public ElevatorSystem elevatorSystem;
        public PlaceLung placeLung;
        public LungProp lungProp;
        public bool ignoreColliderBool;
        void Start()
        {
            lungProp = this.GetComponent<LungProp>();
        }

        void LateUpdate()
        {
            if (!Plugin.emergencyPowerSystem)
            {
                return;
            }
            if (!PlaceLung.isSetupEnd)
            {
                return;
            }
            if (OfficeRoundSystem.Instance.isOffice)
            {
                Setup();
                if (!isSetupEnd) { return; }
                if (this.transform.parent == placeLung.transform)
                {
                    if (!isPlaced)
                    {
                        PlaceLung.socketLED.SetBool("warning", false);
                        PlaceLung.boxCollider.size = new Vector3(0, 0, 0);
                        PlaceLung.lungParentAnimator.SetTrigger("LungPlaced");
                        PlaceLung.socketAudioSource.PlayOneShot(lungProp.connectSFX);
                        PlaceLung.lungPlaced = true;
                        isPlaced = true;
                    }
                }
                if (lungProp.isHeld)
                {
                    if (isPlaced)
                    {
                        PlaceLung.socketLED.SetBool("warning", true);
                        PlaceLung.boxCollider.size = new Vector3(0, 0, 0);
                        //PlaceLung.boxCollider.size = new Vector3(2, 1.6f, 2);
                        PlaceLung.socketAudioSource.PlayOneShot(lungProp.disconnectSFX);
                        PlaceLung.lungPlaced = false;
                        PlaceLung.isPlaceCalled = false;
                        isPlaced = false;
                    }
                    if (ignoreColliderBool)
                    {
                        this.GetComponent<ItemElevatorCheck>().ignoreCollider = false;
                        ignoreColliderBool = false;
                    }
                }

                if (!PlaceLung.emergencyPowerRequires)
                {
                    foreach (InteractTrigger upButton in PlaceLung.upButtons)
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
                    foreach (InteractTrigger downButton in PlaceLung.downButtons)
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

                if (isPlaced)
                {
                    if (PlaceLung.emergencyPowerRequires)
                    {
                        foreach (InteractTrigger upButton in PlaceLung.upButtons)
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
                        foreach (InteractTrigger downButton in PlaceLung.downButtons)
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
                        PlaceLung.socketLED.SetBool("warning", false);
                    }

                    transform.SetParent(PlaceLung.lungParent);
                    transform.localPosition = Vector3.zero;
                    lungProp.targetFloorPosition = Vector3.zero;
                    lungProp.startFallingPosition = Vector3.zero;
                    transform.localRotation = Quaternion.Euler(0, 90, 0);
                    PlaceLung.boxCollider.size = new Vector3(0, 0, 0);
                    PlaceLung.lungPlacedThisFrame = false;
                    if (PlaceLung.placedLung != lungProp)
                    {
                        PlaceLung.placedLung = lungProp;
                    }
                }
                else
                {
                    if (PlaceLung.emergencyPowerRequires)
                    {
                        foreach (InteractTrigger upButton in PlaceLung.upButtons)
                        {
                            if (upButton.onInteract.GetPersistentEventCount() > 0)
                            {
                                upButton.onInteract.RemoveAllListeners();
                            }
                        }
                        foreach (InteractTrigger downButton in PlaceLung.downButtons)
                        {
                            if (downButton.onInteract.GetPersistentEventCount() > 0)
                            {
                                downButton.onInteract.RemoveAllListeners();
                            }
                        }
                    }

                    PlaceLung.boxCollider.size = new Vector3(0, 0, 0);
                    //PlaceLung.boxCollider.size = new Vector3(2, 1.6f, 2);
                    if (PlaceLung.placedLung == lungProp)
                    {
                        PlaceLung.placedLung = null;
                    }
                }
            }

            if (StartOfRound.Instance.inShipPhase && isSetupEnd)
            {
                isSetupEnd = false;
            }
        }

        void Setup()
        {
            placeLung = GameObject.FindObjectOfType<PlaceLung>();
            elevatorSystem = GameObject.FindObjectOfType<ElevatorSystem>();
            isSetupEnd = true;
        }
        */
    }
}
