using DunGen;
using DunGen.Adapters;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Patches
{
    public class RoundMapSystem : NetworkBehaviour
    {
        public static RoundMapSystem Instance { get; private set; }
        public bool isOffice;
        public bool isChecked;
        public bool isDungeonOfficeChecked;

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

        private void LateUpdate()
        {
            if (!this.isChecked && !StartOfRound.Instance.inShipPhase)
            {
                if ((GameObject.Find("IndustrialFan") == null && GameObject.Find("ManorStartRoom") == null))
                {
                    return;
                }
                if (GameObject.Find("IndustrialFan") != null && GameObject.Find("StartRoomElevator") != null)
                {
                    isOffice = true;
                }
                else
                {
                    isOffice = false;
                }
                if (!isDungeonOfficeChecked)
                {
                    StartCoroutine(CheckOfficeElevator());
                }
                /*
                if (this.isOffice)
                {
                    if (GameObject.Find("Stanley") != null)
                    {
                        if (GameObject.Find("Stanley").GetComponent<StanleyTrigger>() == null)
                        {
                            GameObject.Find("Stanley").AddComponent<StanleyTrigger>();
                        }
                    }
                }
                */
                isDungeonOfficeChecked = true;
            }
        }
        IEnumerator CheckOfficeElevator()
        {
            yield return new WaitForSeconds(5f);
            isChecked = true;
            yield break;
        }
    }
}

