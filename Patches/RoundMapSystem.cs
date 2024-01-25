using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Patches
{
    public class RoundMapSystem : MonoBehaviour
    {
        public static RoundMapSystem Instance { get; private set; }
        public bool isOffice;

        private void Awake()
        {
            if (RoundMapSystem.Instance == null)
            {
                RoundMapSystem.Instance = this;
                return;
            }
            GameObject.Destroy(RoundMapSystem.Instance.gameObject);
            RoundMapSystem.Instance = this;
        }
    }
}
