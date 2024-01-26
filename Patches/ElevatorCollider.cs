using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Patches
{
    public class ElevatorCollider : NetworkBehaviour
    {
        public Bounds checkBounds; // 검사할 구역의 Bounds

        public LethalClientMessage<bool> SendInElevator = new LethalClientMessage<bool>("SendInElevator", onReceivedFromClient:IsInElevatorSync);
        public List<GameObject> allPlayerColliders = new List<GameObject>();

        public Vector3 tempTargetFloorPosition;
        public Vector3 tempStartFallingPosition;

        //public PlayerElevatorCheck tempPlayerElevatorCheck;

        static void IsInElevatorSync(bool value, ulong id)
        {
            if (value)
            {
                id.GetPlayerController().GetComponent<PlayerElevatorCheck>().isInElevatorNotOwner = true;
            }
            else
            {
                id.GetPlayerController().GetComponent<PlayerElevatorCheck>().isInElevatorNotOwner = false;
            }
        }

        void Start()
        {
            checkBounds.size = new Vector3(4.9f, 17f, 4.9f);
            foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
            {
                allPlayerColliders.Add(player);
            }
        }

        private void Update()
        {
            if (!RoundMapSystem.Instance.isOffice)
            {
                return;
            }
            checkBounds.center = this.transform.position;

            foreach (GameObject colliderObject in allPlayerColliders)
            {
                if (colliderObject.GetComponent<Collider>() != null)
                {
                    BoxCollider collider = colliderObject.GetComponent<BoxCollider>();
                    if (checkBounds.Intersects(collider.bounds))
                    {
                        if (collider.gameObject.tag == "Player")
                        {
                            PlayerControllerB playerController = collider.GetComponent<PlayerControllerB>();
                            PlayerElevatorCheck playerElevatorCheck = collider.GetComponent<PlayerElevatorCheck>();
                            if (!collider.GetComponent<PlayerElevatorCheck>().isInElevatorB && !playerController.isCameraDisabled)
                            {
                                collider.transform.SetParent(this.transform.parent);
                                SendInElevator.SendAllClients(true);
                                playerElevatorCheck.isInElevatorB = true;
                            }
                        }else if (collider.gameObject.tag == "PhysicsProp")
                        {
                            if (collider.GetComponent<GrabbableObject>().playerHeldBy != null)
                            {
                                if (collider.GetComponent<GrabbableObject>().playerHeldBy.GetComponent<PlayerElevatorCheck>().isInElevatorB)
                                {
                                    tempTargetFloorPosition = collider.GetComponent<GrabbableObject>().playerHeldBy.transform.localPosition;
                                    tempStartFallingPosition = collider.GetComponent<GrabbableObject>().transform.localPosition;
                                }
                            }
                            if (!collider.GetComponent<ItemElevatorCheck>().isInElevatorB)
                            {
                                collider.transform.parent = this.transform.parent;
                                collider.GetComponent<GrabbableObject>().targetFloorPosition = tempTargetFloorPosition;
                                collider.GetComponent<GrabbableObject>().startFallingPosition = tempStartFallingPosition;
                                collider.GetComponent<ItemElevatorCheck>().isInElevatorB = true;
                            }
                        }
                    }
                    else
                    {
                        if (collider.gameObject.tag == "Player")
                        {
                            if (collider.GetComponent<PlayerElevatorCheck>().isInElevatorB)
                            {
                                collider.transform.SetParent(null, true);
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

                        if (colliderObject.GetComponent<PlayerControllerB>() != null)
                        {
                            PlayerControllerB playerController = collider.GetComponent<PlayerControllerB>();
                            PlayerElevatorCheck playerElevatorCheck = collider.GetComponent<PlayerElevatorCheck>();
                            if (!playerElevatorCheck.isInElevatorB && playerController.isCameraDisabled && playerElevatorCheck.isInElevatorNotOwner)
                            {
                                Plugin.mls.LogInfo(playerController.actualClientId + " is inside of elevator check!");
                                collider.transform.SetParent(this.transform.parent, true);
                                playerElevatorCheck.isInElevatorB = true;
                            }else if (playerElevatorCheck.isInElevatorB && playerController.isCameraDisabled && !playerElevatorCheck.isInElevatorNotOwner)
                            {
                                Plugin.mls.LogInfo(playerController.actualClientId + " is outside of elevator check!");
                                collider.transform.SetParent(null, true);
                                playerElevatorCheck.isInElevatorB = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
