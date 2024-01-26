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
        public ElevatorCollider elevatorCollider;

        public bool isInElevatorNotOwner;

        public PlayerControllerB playerControllerB;

        public bool isConfirmedShrimp;

        void Start()
        {
            playerControllerB = this.GetComponent<PlayerControllerB>();
        }
    }
}
