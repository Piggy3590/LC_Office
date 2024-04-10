using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(UnlockableSuit))]
    internal class UnlockableSuitPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(UnlockableSuit __instance)
        {
            __instance.GetComponent<InteractTrigger>().hoverTip = "슈트 변경하기: " + StartOfRound.Instance.unlockablesList.unlockables[__instance.suitID].unlockableName;
        }
    }
}
