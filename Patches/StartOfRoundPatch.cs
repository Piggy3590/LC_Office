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
        private static void Awake_Prefix(StartOfRound __instance)
        {
            __instance.gameObject.AddComponent<RoundMapSystem>();
        }
        [HarmonyPrefix]
        [HarmonyPatch("SceneManager_OnUnloadComplete")]
        private static void SceneManager_OnUnloadComplete_Prefix()
        {
            if (GameObject.FindObjectsOfType<PlayerElevatorCheck>() != null)
            {
                PlayerElevatorCheck[] playerElevatorCheck = GameObject.FindObjectsOfType<PlayerElevatorCheck>();
                foreach (PlayerElevatorCheck player in playerElevatorCheck)
                {
                    player.elevatorCollider = null;
                    player.isConfirmedShrimp = false;
                }
            }
            if (GameObject.FindObjectsOfType<ItemElevatorCheck>() != null)
            {
                ItemElevatorCheck[] itemElevatorChecks = GameObject.FindObjectsOfType<ItemElevatorCheck>();
                foreach (ItemElevatorCheck itemElevatorCheck in itemElevatorChecks)
                {
                    itemElevatorCheck.elevatorCollider = null;
                    itemElevatorCheck.isAppendedToArray = false;
                }
            }

            if (GameObject.FindObjectOfType<ShrimpAI>() != null)
            {
                GameObject.Destroy(GameObject.FindObjectOfType<ShrimpAI>().gameObject);
            }

            RoundMapSystem.Instance.isChecked = false;
        }
    }
}
