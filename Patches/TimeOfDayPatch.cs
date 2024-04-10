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
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
        private static void UpdateProfitQuotaCurrentTime_Postfix(ref int ___daysUntilDeadline, ref float ___timeUntilDeadline, ref float ___totalTime,
            ref int ___hoursUntilDeadline, ref float ___lengthOfHours, ref int ___numberOfHours, ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (StartOfRound.Instance.isChallengeFile)
            {
                StartOfRound.Instance.deadlineMonitorText.text = "가능한 한\n많은 수익을\n얻으세요";
                StartOfRound.Instance.profitQuotaMonitorText.text = "환영합니다!\n이곳은:\n" + GameNetworkManager.Instance.GetNameForWeekNumber();
            }else
            {
                if (___timeUntilDeadline <= 0f)
                {
                    StartOfRound.Instance.deadlineMonitorText.text = "마감일:\n 지금";
                }
                else
                {
                    StartOfRound.Instance.deadlineMonitorText.text = $"마감일:\n{___daysUntilDeadline}일";
                }
                StartOfRound.Instance.profitQuotaMonitorText.text = $"수익\n할당량:\n${___quotaFulfilled} / ${___profitQuota}";
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix()
        {
            HUDManager.Instance.clockNumber.text = HUDManager.Instance.clockNumber.text.Replace("AM", "오전");
            HUDManager.Instance.clockNumber.text = HUDManager.Instance.clockNumber.text.Replace("PM", "오후");
        }
        [HarmonyPostfix]
        [HarmonyPatch("VoteShipToLeaveEarly")]
        private static void VoteShipToLeaveEarly_Postfix(ref DialogueSegment[] ___shipLeavingEarlyDialogue)
        {
            ___shipLeavingEarlyDialogue[0].bodyText = ___shipLeavingEarlyDialogue[0].bodyText.Replace("AM", "오전");
            ___shipLeavingEarlyDialogue[0].bodyText = ___shipLeavingEarlyDialogue[0].bodyText.Replace("PM", "오후");
        }
    }
}
