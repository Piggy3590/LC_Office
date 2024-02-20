using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("StartGame")]
        private static void StartGame_Prefix()
        {
            if (GameObject.FindObjectOfType<OfficeRoundSystem>() == null && GameNetworkManager.Instance.isHostingGame)
            {
                NetworkObject roundMapSystem = GameObject.Instantiate(Plugin.officeRoundSystem).GetComponent<NetworkObject>();
                roundMapSystem.Spawn();
            }
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
            OfficeRoundSystem.Instance.isOffice = false;

            ElevatorSystem.elevatorFloor.Value = 1;
            ElevatorSystem.isElevatorClosed = false;
            ElevatorSystem.spawnShrimpBool.Value = false;

            OfficeRoundSystem.Instance.isDungeonOfficeChecked = false;
            OfficeRoundSystem.Instance.isChecked = false;
            OfficeRoundSystem.Instance.isOffice = false;
        }
    }
}