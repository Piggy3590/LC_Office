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
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disconnect")]
        private static void Disconnect_Prefix()
        {
            GameObject.Destroy(GameObject.FindObjectOfType<ElevatorCollider>());
            GameObject.Destroy(GameObject.FindObjectOfType<ElevatorSystem>());
            GameObject.Destroy(GameObject.FindObjectOfType<ItemElevatorCheck>());
            GameObject.Destroy(GameObject.FindObjectOfType<PlayerElevatorCheck>());
            GameObject.Destroy(GameObject.FindObjectOfType<ShrimpAI>());
            GameObject.Destroy(GameObject.FindObjectOfType<ShrimpCollider>());
            GameObject.Destroy(GameObject.FindObjectOfType<StanleyTrigger>());
            GameObject.Destroy(GameObject.FindObjectOfType<TrapRoomTrigger>());
            GameObject.Destroy(GameObject.FindObjectOfType<RoundMapSystem>());
        }
    }
}
