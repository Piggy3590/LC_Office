using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using LethalNetworkAPI;
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
    public class CameraCulling : MonoBehaviour
    {
        public Transform player;
        public Camera camera;
        public Transform cullPos;

        public float fps = 25;
        private float elapsed;

        void Start()
        {
            player = StartOfRound.Instance.localPlayerController.transform;
            camera = this.GetComponent<Camera>();
            camera.enabled = false;
            cullPos = GameObject.Find("FacilityCameraMonitor").transform;
            fps = 20;
        }
        void Update()
        {
            if (player == null)
                return;

            float distance = Vector3.Distance(cullPos.position, player.position);
            if (distance <= 10)
            {
                elapsed += Time.deltaTime;
                if (elapsed > 1f / fps)
                {
                    elapsed = 0f;
                    camera.Render();
                }
            }
        }
    }
}
