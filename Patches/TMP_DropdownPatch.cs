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
    [HarmonyPatch(typeof(TMP_Dropdown))]
    internal class TMP_DropdownPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void Awake_Postfix(TMP_Dropdown __instance)
        {
            if (__instance.options[0].text == "Sort: near")
            {
                __instance.options[0].text = "정렬: 가까운 서버";
                __instance.options[1].text = "정렬: 먼 서버";
                __instance.options[2].text = "정렬: 전 세계";
            }else if (__instance.options[0].text == "Use monitor (V-Sync)")
            {
                __instance.options[0].text = "모니터 사용 (수직 동기화)";
                __instance.options[1].text = "제한 없음";
            }else if (__instance.options[0].text == "Fullscreen")
            {
                __instance.options[0].text = "전체 화면";
                __instance.options[1].text = "테두리 없는 창";
                __instance.options[2].text = "최대화된 창";
                __instance.options[3].text = "창 모드";
            }
        }
    }
}
