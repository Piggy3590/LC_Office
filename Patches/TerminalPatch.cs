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
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(Terminal __instance, ref List<TerminalNode> ___enemyFiles, ref TerminalNodesList ___terminalNodes)
        {
            ___terminalNodes.specialNodes.Add(Plugin.elevator1Node);
            ___terminalNodes.specialNodes.Add(Plugin.elevator2Node);
            ___terminalNodes.specialNodes.Add(Plugin.elevator3Node);
            ___enemyFiles.Add(Plugin.shrimpTerminalNode);
            Plugin.shrimpTerminalKeyword.defaultVerb = __instance.terminalNodes.allKeywords.First(x => x.word == "info");
            //___enemyFiles.Add(Plugin.haltFile);
            //Plugin.haltTK.defaultVerb = __instance.terminalNodes.allKeywords.First(x => x.word == "info");

            AddKeyword(___terminalNodes, Plugin.elevatorKeyword);
            AddKeyword(___terminalNodes, Plugin.elevator1Keyword);
            AddKeyword(___terminalNodes, Plugin.elevator2Keyword);
            AddKeyword(___terminalNodes, Plugin.elevator3Keyword);

            foreach (TerminalKeyword keyword in ___terminalNodes.allKeywords)
            {
                if (keyword.word == "other")
                {
                    if (Plugin.setKorean && !keyword.specialKeywordResult.displayText.Contains("엘리베이터를"))
                    {
                        keyword.specialKeywordResult.displayText = keyword.specialKeywordResult.displayText.TrimEnd() +
                        "\n\n>ELEVATOR [1f, 2f, 3f]\r\n엘리베이터를 원하는 층으로 이동시킵니다.\r\n\r\n\r\n";
                    }
                    else if (!keyword.specialKeywordResult.displayText.Contains("To move an elevator to a specific floor"))
                    {
                        keyword.specialKeywordResult.displayText = keyword.specialKeywordResult.displayText.TrimEnd() +
                        "\n\n>ELEVATOR [1f, 2f, 3f]\r\nTo move an elevator to a specific floor\r\n\r\n\r\n";
                    }
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("RunTerminalEvents")]
        private static void RunTerminalEvents_Prefix(TerminalNode node)
        {
            if (node.terminalEvent == "el1" && GameObject.FindObjectOfType<ElevatorSystem>() != null)
            {
                ElevatorSystem.ElevatorDownEvent(StartOfRound.Instance.localPlayerController);
            }
            if (node.terminalEvent == "el2" && GameObject.FindObjectOfType<ElevatorSystem>() != null)
            {
                ElevatorSystem.ElevatorMidEvent(StartOfRound.Instance.localPlayerController);
            }
            if (node.terminalEvent == "el3" && GameObject.FindObjectOfType<ElevatorSystem>() != null)
            {
                ElevatorSystem.ElevatorUpEvent(StartOfRound.Instance.localPlayerController);
            }
        }

        static void AddKeyword(TerminalNodesList terminalNodes, TerminalKeyword keyword)
        {
            System.Array.Resize(ref terminalNodes.allKeywords, terminalNodes.allKeywords.Length + 1);
            terminalNodes.allKeywords[terminalNodes.allKeywords.Length - 1] = keyword;
        }
    }
}
