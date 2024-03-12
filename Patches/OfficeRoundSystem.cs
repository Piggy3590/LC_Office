using DunGen;
using DunGen.Adapters;
using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace LCOffice.Patches
{
    public class OfficeRoundSystem : NetworkBehaviour
    {
        public static OfficeRoundSystem Instance { get; private set; }
        public bool isOffice;
        public bool isChecked;
        public bool isDungeonOfficeChecked;

        public static AudioSource haltMusicAudio;
        public static Animator haltNoiseScreen;
        public static Animator playerScreenAnimator;

        private void Awake()
        {
            /*
            if (OfficeRoundSystem.Instance == null)
            {
                OfficeRoundSystem.Instance = this;
                return;
            }
            GameObject.Destroy(OfficeRoundSystem.Instance.gameObject);
            */
            OfficeRoundSystem.Instance = this;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += this.ResetStaticVariable;
            haltMusicAudio = GameObject.Instantiate(GameObject.Find("Music2")).GetComponent<AudioSource>();
            haltNoiseScreen = GameObject.Instantiate(Plugin.haltNoiseScreen, GameObject.Find("DebugMessagesPanel").transform).GetComponent<Animator>();
            playerScreenAnimator = GameObject.Find("PlayerScreen").transform.parent.gameObject.AddComponent<Animator>();
            playerScreenAnimator.runtimeAnimatorController = Plugin.playerScreenParentController;
            OfficeRoundSystem.playerScreenAnimator.SetLayerWeight(1, 0.8f);
            GameObject.Find("HaltTurnBackText").GetComponent<TMP_Text>().font = GameObject.Find("TipLeft1").GetComponent<TMP_Text>().font;
            haltMusicAudio.name = "HaltMusic";

        }

        private void LateUpdate()
        {
            if (!this.isChecked && TimeOfDay.Instance.currentDayTimeStarted && RoundManager.Instance.dungeonGenerator != null)
            {
                if (RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.name == "OfficeDungeonFlow")
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
                    if (isOffice && base.IsServer)
                    {
                        /*
                        NetworkObject elevatorManager = GameObject.Instantiate(Plugin.socketInteractPrefab).GetComponent<NetworkObject>();
                        elevatorManager.Spawn();
                        */
                        NetworkObject elevatorCollider = GameObject.Instantiate(Plugin.insideCollider).GetComponent<NetworkObject>();
                        elevatorCollider.Spawn();
                    }
                    isDungeonOfficeChecked = true;
                }
            }
            if (isChecked)
            {
                if (!TimeOfDay.Instance.currentDayTimeStarted)
                {
                    if (isOffice && base.IsServer)
                    {
                        //GameObject.Destroy(GameObject.Find("InsideCollider(Clone)"));
                        /*
                        Plugin.mls.LogInfo("destroying elevator system");
                        GameObject.Destroy(GameObject.Find("LungPlacement(Clone)"));
                        */
                    }
                    isOffice = false;
                    isChecked = false;
                    isDungeonOfficeChecked = false;
                }
            }
        }

        void ResetStaticVariable(Scene scene, LoadSceneMode mode)
        {
            //GameObject.Destroy(GameObject.FindObjectOfType<ElevatorCollider>());
            PlaceLung.emergencyPowerRequires = false;
            PlaceLung.emergencyCheck = false;
            PlaceLung.lungPlaced = false;
            /*
            PlaceLung.lungPlacedThisFrame = false;
            PlaceLung.placeLungNetwork = false;
            PlaceLung.lungPlacer.Value = null;
            PlaceLung.placedLung.Value = null;
            PlaceLung.isPlaceCalled = false;
            PlaceLung.lungPlacedLocalFrame = false;
            PlaceLung.isSetupEnd = false;
            PlaceLung.socketLED = null;
            PlaceLung.socketAudioSource = null;
            PlaceLung.socketInteractTrigger = null;
            PlaceLung.lungPos = null;
            PlaceLung.lungSocket = null;
            PlaceLung.isSetupEnd = false;
            */

            ElevatorSystem.elevatorFloor.Value = 1;
            ElevatorSystem.isElevatorClosed = false;
            ElevatorSystem.spawnShrimpBool.Value = false;
            ElevatorSystem.isSetupEnd = false;
        }
        IEnumerator CheckOfficeElevator()
        {
            yield return new WaitForSeconds(7f);
            isChecked = true;
            if (GameObject.Find("HaltOriginalTile(Clone)") != null)
            {
                GameObject.Find("HaltOriginalTile(Clone)").AddComponent<HaltRoom>();
            }
            SetKorean();
            yield break;
        }

        void SetKorean()
        {
            InteractTrigger[] tempInteractTriggers = GameObject.FindObjectsOfType<InteractTrigger>();
            foreach (InteractTrigger interactTrigger in tempInteractTriggers)
            {
                if (Plugin.setKorean)
                {
                    if (interactTrigger.hoverTip == "Open : [LMB]")
                    {
                        interactTrigger.hoverTip = "열기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Open : [E]")
                    {
                        interactTrigger.hoverTip = "열기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Use door : [LMB]")
                    {
                        interactTrigger.hoverTip = "문 사용하기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Use door : [E]")
                    {
                        interactTrigger.hoverTip = "문 사용하기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Go to the 1st floor : [LMB]")
                    {
                        interactTrigger.hoverTip = "1층으로 이동하기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Go to the 1st floor : [E]")
                    {
                        interactTrigger.hoverTip = "1층으로 이동하기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Go to the 2nd floor : [LMB]")
                    {
                        interactTrigger.hoverTip = "2층으로 이동하기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Go to the 2nd floor : [E]")
                    {
                        interactTrigger.hoverTip = "2층으로 이동하기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Go to the 3rd floor : [LMB]")
                    {
                        interactTrigger.hoverTip = "3층으로 이동하기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Go to the 3rd floor : [E]")
                    {
                        interactTrigger.hoverTip = "3층으로 이동하기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Place Apparatus : [LMB]")
                    {
                        interactTrigger.hoverTip = "장치 설치하기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Place Apparatus : [E]")
                    {
                        interactTrigger.hoverTip = "장치 설치하기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Store item : [LMB]")
                    {
                        interactTrigger.hoverTip = "아이템 보관하기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Store item : [E]")
                    {
                        interactTrigger.hoverTip = "아이템 보관하기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Climb : [LMB]")
                    {
                        interactTrigger.hoverTip = "오르기 : [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Climb : [E]")
                    {
                        interactTrigger.hoverTip = "오르기 : [E]";
                    }

                    if (interactTrigger.hoverTip == "Flush: [LMB]")
                    {
                        interactTrigger.hoverTip = "물 내리기: [LMB]";
                    }
                    else if (interactTrigger.hoverTip == "Flush: [E]")
                    {
                        interactTrigger.hoverTip = "물 내리기: [E]";
                    }

                    if (interactTrigger.disabledHoverTip == "[Not holding Apparatus]")
                    {
                        interactTrigger.disabledHoverTip = "[장치를 들고 있지 않음]";
                    }
                }
            }
        }
    }
}

