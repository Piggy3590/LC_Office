using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Awake")]
        private static void Awake_Prefix()
        {
            StartOfRound.Instance.gameObject.AddComponent<RoundMapSystem>();
        }
        [HarmonyPrefix]
        [HarmonyPatch("SceneManager_OnUnloadComplete")]
        private static void SceneManager_OnUnloadComplete_Prefix()
        {
            PlayerElevatorCheck[] playerElevatorCheck = GameObject.FindObjectsOfType<PlayerElevatorCheck>();
            ItemElevatorCheck[] itemElevatorChecks = GameObject.FindObjectsOfType<ItemElevatorCheck>();
            foreach (PlayerElevatorCheck player in playerElevatorCheck)
            {
                player.elevatorCollider = null;
                player.isAppendedToArray = false;
                player.isConfirmedShrimp = false;
            }
            foreach (ItemElevatorCheck itemElevatorCheck in itemElevatorChecks)
            {
                itemElevatorCheck.elevatorCollider = null;
                itemElevatorCheck.isAppendedToArray = false;
            }

            if (GameObject.Find("InsideCollider") != null)
            {
                RoundMapSystem.Instance.isOffice = true;
            }else
            {
                RoundMapSystem.Instance.isOffice = false;
            }
        }
    }
}
