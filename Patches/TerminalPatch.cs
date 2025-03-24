using HarmonyLib;
using LCOffice.Components;
using System;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(ref TerminalNodesList ___terminalNodes)
        {
            ___terminalNodes.specialNodes.Add(Plugin.elevator1Node);
            ___terminalNodes.specialNodes.Add(Plugin.elevator2Node);
            ___terminalNodes.specialNodes.Add(Plugin.elevator3Node);

            AddKeyword(___terminalNodes, Plugin.elevatorKeyword);
            AddKeyword(___terminalNodes, Plugin.elevator1Keyword);
            AddKeyword(___terminalNodes, Plugin.elevator2Keyword);
            AddKeyword(___terminalNodes, Plugin.elevator3Keyword);

            foreach (TerminalKeyword keyword in ___terminalNodes.allKeywords)
            {
                if (keyword.word == "other")
                {
                    keyword.specialKeywordResult.displayText = keyword.specialKeywordResult.displayText.TrimEnd() +
                    "\n\n>ELEVATOR [1f, 2f, 3f]\r\nTo move an elevator to a specific floor\r\n\r\n\r\n";
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("RunTerminalEvents")]
        private static void RunTerminalEvents_Prefix(TerminalNode node)
        {
            if (ElevatorSystem.System != null && node != null && node.terminalEvent.Length == 3 && node.terminalEvent.StartsWith("el") && int.TryParse(node.terminalEvent[2].ToString(), out int floor))
            {
                ElevatorSystem.System.ElevatorTriggerServerRpc(floor - 1);
            }
        }

        static void AddKeyword(TerminalNodesList terminalNodes, TerminalKeyword keyword)
        {
            Array.Resize(ref terminalNodes.allKeywords, terminalNodes.allKeywords.Length + 1);
            terminalNodes.allKeywords[terminalNodes.allKeywords.Length - 1] = keyword;
        }
    }
}
