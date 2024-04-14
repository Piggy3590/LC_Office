using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(PlayerControllerB __instance, ref bool ___isCameraDisabled)
        {
            __instance.gameObject.AddComponent<PlayerElevatorCheck>();
        }

        [HarmonyPostfix]
        [HarmonyPatch("TeleportPlayer")]
        private static void TeleportPlayer_Postfix(PlayerControllerB __instance)
        {
            if (HaltRoom.isInHaltSequance)
            {
                __instance.KillPlayer(Vector3.zero, true, CauseOfDeath.Unknown, 1);
            }
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch("DiscardHeldObject")]
        private static void DiscardHeldObject_Prefix(PlayerControllerB __instance, bool placeObject = false, NetworkObject parentObjectTo = null, Vector3 placePosition = default(Vector3), bool matchRotationOfParent = true)
        {
            if (__instance.GetComponent<PlayerElevatorCheck>().isInElevatorB)
            {
                Plugin.mls.LogInfo("SS");
                ElevatorCollider elevatorCollider = GameObject.FindObjectOfType<ElevatorCollider>();
                placeObject = true;
                elevatorCollider.transform.InverseTransformPoint(placePosition);
                parentObjectTo = elevatorCollider.GetComponent<NetworkObject>();
            }
        }
        */
    }
}
