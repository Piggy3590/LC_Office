using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(PlayerControllerB __instance)
        {
            __instance.gameObject.AddComponent<PlayerElevatorCheck>();
        }
    }
}
