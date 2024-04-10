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
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(GrabbableObject __instance, ref Item ___itemProperties)
        {
            if (___itemProperties.itemName == "clipboard")
            {
                ___itemProperties.itemName = "클립보드";
                ___itemProperties.toolTips[0] = "자세히 보기: [Z]";
                ___itemProperties.toolTips[1] = "페이지 넘기기 : [Q/E]";
            }
            else if (___itemProperties.itemName == "Sticky note")
            {
                ___itemProperties.itemName = "스티커 메모";
                ___itemProperties.toolTips[0] = "자세히 보기: [Z]";
            }

            if (__instance is StunGrenadeItem)
            {
                if (__instance.GetComponent<StunGrenadeItem>().throwString == "Toss egg: [RMB]")
                {
                    __instance.GetComponent<StunGrenadeItem>().throwString = "달걀 던지기: [RMB]";
                }else if (__instance.GetComponent<StunGrenadeItem>().throwString == "Throw grenade: [RMB]")
                {
                    __instance.GetComponent<StunGrenadeItem>().throwString = "수류탄 던지기: [RMB]";
                }
            }
        }
    }
}
