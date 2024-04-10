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
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
        private static void SetHoverTipAndCurrentInteractTrigger_Postfix(ref TextMeshProUGUI ___cursorTip)
        {
            if (___cursorTip.text == "Inventory full!")
            {
                ___cursorTip.text = "인벤토리 가득 참!";
            }else if (___cursorTip.text == "(Cannot hold until ship has landed)")
            {
                ___cursorTip.text = "(함선이 착륙하기 전까지 집을 수 없음)";
            }else if (___cursorTip.text == "[Hands full]")
            {
                ___cursorTip.text = "[양 손 사용 중]";
            }else if (___cursorTip.text == "Grab : [E]")
            {
                ___cursorTip.text = "줍기 : [E]]";
            }
        }
    }
}
