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
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        private static void Update_Prefix(ref InteractTrigger ___triggerScript)
        {
            if (___triggerScript.hoverTip == "Land ship : [LMB]")
            {
                ___triggerScript.hoverTip = "함선 착륙하기 : [LMB]";
            }
            else if (___triggerScript.hoverTip == "Start game : [LMB]")
            {
                ___triggerScript.hoverTip = "게임 시작하기 : [LMB]";
            }
            else if (___triggerScript.hoverTip == "Start ship : [LMB]")
            {
                ___triggerScript.hoverTip = "함선 출발하기 : [LMB]";
            }

            if (___triggerScript.disabledHoverTip == "[Wait for ship to land]")
            {
                ___triggerScript.disabledHoverTip = "[함선이 완전히 이착륙할 때까지 기다리세요]";
            }
        }
    }
}
