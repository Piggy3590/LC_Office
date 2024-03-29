﻿using GameNetcodeStuff;
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
        private static void Start_Prefix(Terminal __instance, ref List<TerminalNode> ___enemyFiles)
        {
            ___enemyFiles.Add(Plugin.shrimpTerminalNode);
            Plugin.shrimpTerminalKeyword.defaultVerb = __instance.terminalNodes.allKeywords.First(x => x.word == "info");
            //___enemyFiles.Add(Plugin.haltFile);
            //Plugin.haltTK.defaultVerb = __instance.terminalNodes.allKeywords.First(x => x.word == "info");
        }
    }
}
