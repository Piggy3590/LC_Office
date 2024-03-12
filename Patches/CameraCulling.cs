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
        public GameObject[] playerObjects;
        public Transform lookAtTarget;

        public bool isElevatorCamera;

        public float fps = 25;
        private float elapsed;

        void Start()
        {
            if (this.name != "ElevatorCamera")
            {
                isElevatorCamera = true;
            }
            player = StartOfRound.Instance.localPlayerController.transform;
            camera = this.GetComponent<Camera>();
            cullPos = GameObject.Find("FacilityCameraMonitor").transform;
            fps = Plugin.cameraFrameSpeed;
            camera.enabled = false;
            playerObjects = StartOfRound.Instance.allPlayerObjects;
        }
        void Update()
        {
            if (Plugin.cameraDisable)
            {
                return;
            }
            if (player == null)
                return;

            float distance = Vector3.Distance(cullPos.position, player.position);
            if (distance <= 10)
            {
                if (Plugin.cameraFrameSpeed < 100)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed > 1f / fps)
                    {
                        elapsed = 0f;
                        camera.Render();
                    }
                }else
                {
                    camera.enabled = true;
                }
            }
        }

        void LateUpdate()
        {
            if (Plugin.cameraDisable)
            {
                return;
            }
            if (!isElevatorCamera) { return; }
            foreach (GameObject player in playerObjects)
            {
                float playerDistance = Vector3.Distance(this.transform.position, player.transform.position);
                if (playerDistance < 30f && playerDistance < Mathf.Infinity)
                {
                    lookAtTarget = player.transform;
                }
            }
            if (lookAtTarget != null)
            {
                Vector3 lookDirection = lookAtTarget.position - this.transform.position;
                lookDirection.Normalize();

                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(lookDirection), 5 * Time.deltaTime);
            }
        }
    }
}
