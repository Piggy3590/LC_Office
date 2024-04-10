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
    [HarmonyPatch(typeof(PreInitSceneScript))]
    internal class PreInitSceneScriptPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("PressContinueButton")]
        private static void PressContinueButton_Postfix(ref int ___currentLaunchSettingPanel, ref GameObject[] ___LaunchSettingsPanels,
            ref Animator ___blackTransition, ref GameObject ___continueButton, ref TextMeshProUGUI ___headerText)
        {
            if (___headerText.text == "LAUNCH MODE")
            {
                ___headerText.text = "실행 모드";
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("SkipToFinalSetting")]
        private static void SkipToFinalSetting_Postfix(ref TextMeshProUGUI ___headerText)
        {
            ___headerText.text = "실행 모드";
        }
    }
}
