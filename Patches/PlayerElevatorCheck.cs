using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.Rendering;
using Dissonance;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine.AI;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace LCOffice.Patches
{
    public class PlayerElevatorCheck : MonoBehaviour
    {
        public bool isInElevatorB;
        public bool isAppendedToArray;
        public ElevatorCollider elevatorCollider;

        public Transform mainCamera;
        public Transform cameraContainer;

        public bool isInElevatorNotOwner;

        public PlayerControllerB playerControllerB;

        public bool isConfirmedShrimp;

        void Start()
        {
            playerControllerB = this.GetComponent<PlayerControllerB>();
        }

        void LateUpdate()
        {
            if (!isAppendedToArray && !StartOfRound.Instance.inShipPhase)
            {
                if (GameObject.Find("ElevatorScreenDoor") != null)
                {
                    if (!RoundMapSystem.Instance.isOffice) { RoundMapSystem.Instance.isOffice = true; }
                }
                else
                {
                    if (RoundMapSystem.Instance.isOffice) { RoundMapSystem.Instance.isOffice = false; }
                }
                if (RoundMapSystem.Instance.isOffice)
                {
                    if (GameObject.Find("Stanley") != null)
                    {
                        if (GameObject.Find("Stanley").GetComponent<StanleyTrigger>() == null)
                        {
                            GameObject.Find("Stanley").AddComponent<StanleyTrigger>();
                        }
                    }
                }
                isAppendedToArray = true;
            }
        }
    }
}
