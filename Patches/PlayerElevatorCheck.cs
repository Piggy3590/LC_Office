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
using System.Security.Permissions;
using UnityEngine.AI;


namespace LCOffice.Patches
{
    public class PlayerElevatorCheck : MonoBehaviour
    {
        public bool isInElevatorB;
        public ElevatorCollider elevatorCollider;

        public bool isInElevatorNotOwner;
        public bool wasInElevator;

        public PlayerControllerB playerControllerB;

        public bool isConfirmedShrimp;

        void Start()
        {
            playerControllerB = this.GetComponent<PlayerControllerB>();
        }

        void Update()
        {
            if (StartOfRound.Instance.shipIsLeaving && wasInElevator)
            {
                if (this.transform.parent != playerControllerB.playersManager.elevatorTransform && this.transform.parent != playerControllerB.playersManager.playersContainer)
                {
                    this.transform.SetParent(playerControllerB.playersManager.playersContainer);
                }
                wasInElevator = false;
            }
        }
    }
}
