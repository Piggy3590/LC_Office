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
    public class TrapRoomTrigger : NetworkBehaviour
    {
        public LethalClientMessage<bool> SendTrapFloorTrigger = new LethalClientMessage<bool>("SendTrapFloorTrigger");
        public LethalClientMessage<bool> SendTrapDeactivate = new LethalClientMessage<bool>("SendTrapDeactivate");

        public AudioSource floorAudioSource;
        public AudioSource floorAudioSource2;
        public AudioSource doorAudioSource;
        public Animator roomAnimator;
        public bool isPlayed;
        public bool isPlayedOnce;
        public bool isDeactivated;

        public PlayerControllerB[] players;

        public float Timer;

        void SendTrapFloorTriggerSync(bool value, ulong id)
        {
            if (value && !isPlayed)
            {
                isPlayed = true;
            }
            else if (!value && isPlayed)
            {
                isPlayed = false;
            }
        }

        void SendTrapDeactivateSync(bool value, ulong id)
        {
            if (value && !isDeactivated)
            {
                isDeactivated = true;
            }
            else if (!value && isDeactivated)
            {
                isDeactivated = false;
            }
        }

        void Start()
        {
            SendTrapFloorTrigger.OnReceivedFromClient += SendTrapFloorTriggerSync;
            SendTrapDeactivate.OnReceivedFromClient += SendTrapDeactivateSync;

            roomAnimator = GameObject.Find("TrapDoorAnimator").GetComponent<Animator>();

            floorAudioSource = GameObject.Find("Floor1").GetComponent<AudioSource>();
            floorAudioSource2 = GameObject.Find("Floor2").GetComponent<AudioSource>();

            doorAudioSource = GameObject.Find("DoorSlam").GetComponent<AudioSource>();

            GameObject.Find("TrapTrigger").GetComponent<InteractTrigger>().onInteract.AddListener(TriggerTrap);
            GameObject.Find("IgnoreFallDamagePlayer").GetComponent<InteractTrigger>().onInteract.AddListener(IgnoreFallDamage);
            this.GetComponent<TerminalAccessibleObject>().terminalCodeEvent.AddListener(DeactivateTrap);

            //players = GameObject.FindObjectsOfType<PlayerControllerB>();
        }

        void Update()
        {
            if (isPlayed && !isPlayedOnce && !isDeactivated)
            {
                base.StartCoroutine(this.ActivateTrap());
                isPlayed = false;
                isPlayedOnce = true;
            }

            if (!isPlayed && isDeactivated && isPlayedOnce)
            {
                base.StartCoroutine(this.DeactivateTrapCoroutine());
                isDeactivated = false;
            }
        }

        public void TriggerTrap(PlayerControllerB playerControllerB)
        {
            SendTrapFloorTrigger.SendAllClients(true);
        }

        public void IgnoreFallDamage(PlayerControllerB playerControllerB)
        {
            playerControllerB.fallValue -= 5;
        }

        public void DeactivateTrap(PlayerControllerB playerControllerB)
        {
            SendTrapDeactivate.SendAllClients(true);
        }

        private IEnumerator ActivateTrap()
        {
            roomAnimator.SetBool("open", true);
            doorAudioSource.PlayOneShot(Plugin.garageDoorSlam);
            yield return new WaitForSeconds(3f);
            doorAudioSource.PlayOneShot(Plugin.floorOpen);
            yield break;
        }

        private IEnumerator DeactivateTrapCoroutine()
        {
            roomAnimator.SetBool("unlock", true);
            doorAudioSource.PlayOneShot(Plugin.floorClose);
            yield return new WaitForSeconds(1f);
            doorAudioSource.PlayOneShot(Plugin.garageSlide);
            yield break;
        }
    }
}
