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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(SaveFileUISlot))]
    internal class SaveFileUISlotPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable_Postfix(ref TextMeshProUGUI ___fileStatsText)
        {

        }
    }
}
