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
    [HarmonyPatch(typeof(InteractTrigger))]
    internal class InteractTriggerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Interact")]
        private static void Interact_Postfix(ref string ___hoverTip)
        {
            if (___hoverTip == "Use ladder : [LMB]")
            {
                ___hoverTip = "사다리 사용하기 : [LMB]";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(ref string ___hoverTip, ref string ___disabledHoverTip, ref string ___holdTip)
        {
            if (___hoverTip == "Charge item : [LMB]")
            {
                ___hoverTip = "아이템 충전하기 : [LMB]";
            }
            else if (___hoverTip == "Access terminal : [LMB]")
            {
                ___hoverTip = "터미널 접근하기 : [LMB]";
            }
            else if (___hoverTip == "Disable speaker: [LMB]")
            {
                ___hoverTip = "스피커 끄기: [LMB]";
            }
            else if (___hoverTip == "Switch lights : [LMB]")
            {
                ___hoverTip = "전등 전환하기 : [LMB]";
            }
            else if (___hoverTip == "Change suit")
            {
                ___hoverTip = "슈트 변경하기";
            }
            else if (___hoverTip == "Open : [LMB]")
            {
                ___hoverTip = "열기 : [LMB]";
            }
            else if (___hoverTip == "Open door : [LMB]")
            {
                ___hoverTip = "문 열기 : [LMB]";
            }
            else if (___hoverTip == "Close door : [LMB]")
            {
                ___hoverTip = "문 닫기 : [LMB]";
            }
            else if (___hoverTip == "Enter : [LMB]")
            {
                ___hoverTip = "들어가기 : [LMB]";
            }
            else if (___hoverTip == "Exit : [LMB]")
            {
                ___hoverTip = "나가기 : [LMB]";
            }
            else if (___hoverTip == "Use door : [LMB]")
            {
                ___hoverTip = "문 사용하기 : [LMB]";
            }
            else if (___hoverTip == "Store item : [LMB]")
            {
                ___hoverTip = "아이템 보관하기 : [LMB]";
            }
            else if (___hoverTip == "Use ladder : [LMB]")
            {
                ___hoverTip = "사다리 사용하기 : [LMB]";
            }
            else if (___hoverTip == "Climb : [LMB]")
            {
                ___hoverTip = "오르기 : [LMB]";
            }
            else if (___hoverTip == "Let go : [LMB]")
            {
                ___hoverTip = "내리기 : [LMB]";
            }
            else if (___hoverTip == "Switch camera : [LMB]")
            {
                ___hoverTip = "카메라 전환하기 : [LMB]";
            }
            else if (___hoverTip == "Turn on/off : [LMB]")
            {
                ___hoverTip = "전원 켜기/끄기 : [LMB]";
            }

            if (___disabledHoverTip == "(Requires battery-powered item)")
            {
                ___disabledHoverTip = "(배터리로 작동하는 아이템 필요)";
            }
            else if (___disabledHoverTip == "[Nothing to store]")
            {
                ___disabledHoverTip = "[보관할 아이템이 없음]";
            }
            else if (___disabledHoverTip == "Locked")
            {
                ___disabledHoverTip = "잠김";
            }
        }
    }
}
