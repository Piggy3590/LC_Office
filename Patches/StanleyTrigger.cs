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
    public class StanleyTrigger : MonoBehaviour
    {
        public AudioSource audioSource;
        public bool isPlayed;

        void Start()
        {
            GameObject.Find("Stanley").GetComponent<InteractTrigger>().onInteract.AddListener(TriggerStanley);
            audioSource = GameObject.Find("NarratorAudio").GetComponent<AudioSource>();
        }

        public void TriggerStanley(PlayerControllerB playerControllerB)
        {
            if (!isPlayed)
            {
                base.StartCoroutine(this.PlayVoice1());
                isPlayed = true;
            }
        }

        private IEnumerator PlayVoice1()
        {
            yield return new WaitForSeconds(3f);
            audioSource.PlayOneShot(Plugin.stanleyVoiceline1);
            yield break;
        }
    }
}
