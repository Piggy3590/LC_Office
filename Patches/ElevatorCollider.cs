using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace LCOffice.Patches
{
    public class ElevatorCollider : NetworkBehaviour
    {
        public Bounds checkBounds;

        public LethalClientMessage<bool> SendInElevator = new LethalClientMessage<bool>("SendInElevator", onReceivedFromClient:IsInElevatorSync);
        public List<BoxCollider> allColliders = new List<BoxCollider>();
        public Transform storageParent;
        public Transform lungPlacement;
        public Transform colliderPos;
        public Transform elevatorTransform;

        public ElevatorSystem elevatorSystem;

        public Vector3 tempTargetFloorPosition;
        public Vector3 tempStartFallingPosition;

        //public PlayerElevatorCheck tempPlayerElevatorCheck;

        static void IsInElevatorSync(bool value, ulong id)
        {
            if (value)
            {
                id.GetPlayerController().GetComponent<PlayerElevatorCheck>().wasInElevator = true;
                id.GetPlayerController().GetComponent<PlayerElevatorCheck>().isInElevatorNotOwner = true;
            }
            else
            {
                id.GetPlayerController().GetComponent<PlayerElevatorCheck>().isInElevatorNotOwner = false;
            }
        }

        void Start()
        {
            elevatorTransform = GameObject.Find("StartRoomElevator").transform;
            checkBounds.size = new Vector3(4.9f, 17f, 4.9f);
            foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
            {
                allColliders.Add(player.GetComponent<BoxCollider>());
            }
            //lungPlacement = GameObject.FindObjectOfType<PlaceLung>().transform;
            colliderPos = GameObject.Find("InsideColliderPos").transform;
        }

        void FixedUpdate()
        {
            if (colliderPos != null)
            {
                this.transform.position = colliderPos.position;
            }
        }

        private void Update()
        {
            if (!OfficeRoundSystem.Instance.isOffice)
            {
                return;
            }


            if (storageParent == null)
            {
                if (GameObject.Find("ElevatorStorage(Clone)") != null)
                {
                    storageParent = GameObject.Find("ElevatorStorage(Clone)").transform;
                }
            }
            /*
            if (lungPlacement == null)
            {
                lungPlacement = GameObject.FindObjectOfType<PlaceLung>().transform;
            }
            */
            if (elevatorSystem == null)
            {
                elevatorSystem = GameObject.FindObjectOfType<ElevatorSystem>();
            }
            checkBounds.center = colliderPos.position;

            allColliders.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy);

            foreach (BoxCollider collider in allColliders)
            {
                if (checkBounds.Intersects(collider.bounds))
                {
                    if (collider.gameObject.tag == "Player")
                    {
                        PlayerControllerB playerController = collider.GetComponent<PlayerControllerB>();
                        PlayerElevatorCheck playerElevatorCheck = collider.GetComponent<PlayerElevatorCheck>();
                        if (playerElevatorCheck.elevatorCollider == null)
                        {
                            playerElevatorCheck.elevatorCollider = this;
                        }
                        if (!collider.GetComponent<PlayerElevatorCheck>().isInElevatorB && !playerController.isCameraDisabled)
                        {
                            collider.transform.SetParent(elevatorTransform);
                            SendInElevator.SendAllClients(true);
                            playerElevatorCheck.wasInElevator = true;
                            playerElevatorCheck.isInElevatorB = true;
                        }
                    }else if (collider.gameObject.tag == "PhysicsProp")
                    {
                        GrabbableObject grabbableObject = collider.GetComponent<GrabbableObject>();
                        ItemElevatorCheck itemElevatorCheck = collider.GetComponent<ItemElevatorCheck>();
                        /*
                        if (grabbableObject.playerHeldBy != null && (collider.transform.parent != storageParent || collider.transform.parent != PlaceLung.lungParent))
                        {
                            if (grabbableObject.playerHeldBy.GetComponent<PlayerElevatorCheck>().isInElevatorB)
                            {
                                tempTargetFloorPosition = grabbableObject.playerHeldBy.transform.localPosition;
                                tempStartFallingPosition = grabbableObject.transform.localPosition; 
                            }
                        }*/
                        if (grabbableObject.playerHeldBy != null && grabbableObject.playerHeldBy.GetComponent<PlayerElevatorCheck>().isInElevatorB && grabbableObject.isHeld)
                        {
                            tempTargetFloorPosition = grabbableObject.playerHeldBy.transform.localPosition;
                            tempStartFallingPosition = grabbableObject.playerHeldBy.transform.parent.InverseTransformPoint(grabbableObject.transform.position);
                            //tempStartFallingPosition = grabbableObject.playerHeldBy.transform.localPosition;
                        }
                        if (!itemElevatorCheck.isInElevatorB && !grabbableObject.isHeld)
                        {
                            /*
                            if (collider.transform.parent != PlaceLung.lungParent && grabbableObject is LungProp)
                            {
                                if (!collider.GetComponent<PlaceableLungProp>().isPlaced)
                                {
                                    collider.transform.SetParent(elevatorTransform, true);
                                    grabbableObject.targetFloorPosition = tempTargetFloorPosition;
                                    grabbableObject.startFallingPosition = tempStartFallingPosition;
                                    itemElevatorCheck.isInElevatorB = true;
                                }else
                                {
                                    grabbableObject.targetFloorPosition = PlaceLung.lungParent.position;
                                    grabbableObject.startFallingPosition = PlaceLung.lungParent.position;
                                    itemElevatorCheck.isInElevatorB = true;
                                }
                            */
                            if (collider.transform.parent != storageParent)
                            {
                                collider.transform.SetParent(elevatorTransform, true);
                                grabbableObject.targetFloorPosition = tempTargetFloorPosition;
                                grabbableObject.startFallingPosition = tempStartFallingPosition;
                                itemElevatorCheck.isInElevatorB = true;
                            }
                        }
                    }
                }
                else
                {
                    if (collider.gameObject.tag == "Player")
                    {
                        if (collider.GetComponent<PlayerElevatorCheck>().isInElevatorB)
                        {
                            collider.transform.SetParent(StartOfRound.Instance.playersContainer, true);
                            SendInElevator.SendAllClients(false);
                            collider.GetComponent<PlayerElevatorCheck>().isInElevatorB = false;
                        }else if (collider.GetComponent<PlayerElevatorCheck>().isInElevatorB
                            && collider.GetComponent<PlayerControllerB>().isCameraDisabled
                            && !collider.GetComponent<PlayerElevatorCheck>().isInElevatorNotOwner)
                        {
                            collider.transform.SetParent(null, true);
                            collider.GetComponent<PlayerElevatorCheck>().isInElevatorB = false;
                        }
                    }

                    if (collider.GetComponent<PlayerControllerB>() != null)
                    {
                        PlayerControllerB playerController = collider.GetComponent<PlayerControllerB>();
                        PlayerElevatorCheck playerElevatorCheck = collider.GetComponent<PlayerElevatorCheck>();
                        if (!playerElevatorCheck.isInElevatorB && playerController.isCameraDisabled && playerElevatorCheck.isInElevatorNotOwner)
                        {
                            collider.transform.SetParent(elevatorTransform, true);
                            playerElevatorCheck.isInElevatorB = true;
                            playerElevatorCheck.wasInElevator = true;
                        }
                        else if (playerElevatorCheck.isInElevatorB && playerController.isCameraDisabled && !playerElevatorCheck.isInElevatorNotOwner)
                        {
                            collider.transform.SetParent(null, true);
                            playerElevatorCheck.isInElevatorB = false;
                        }
                    }
                }
            }
        }
    }
}
