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
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        private static void Update_Prefix(GrabbableObject __instance)
        {
            if (__instance.gameObject.GetComponent<ItemElevatorCheck>() == null)
            {
                __instance.gameObject.AddComponent<ItemElevatorCheck>();
            }
        }
    }
}
