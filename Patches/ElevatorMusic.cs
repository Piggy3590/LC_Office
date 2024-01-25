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
    public class ElevatorMusic : MonoBehaviour
    {
        public AudioSource audioSource;
        public int currentMusic;
        public float musicPlayTimer;

        void Start()
        {
            audioSource = this.GetComponent<AudioSource>();
            audioSource.PlayOneShot(Plugin.bossaLullaby);
            musicPlayTimer = 0;
            currentMusic = 1;
        }
        void Update()
        {
            if (!audioSource.isPlaying)
            {
                musicPlayTimer += Time.deltaTime;
                if (musicPlayTimer > 0.5f)
                {
                    if (currentMusic == 0)
                    {
                        audioSource.PlayOneShot(Plugin.bossaLullaby);
                        musicPlayTimer = 0;
                        currentMusic += 1;
                    }else if (currentMusic == 1)
                    {
                        audioSource.PlayOneShot(Plugin.shopTheme);
                        musicPlayTimer = 0;
                        currentMusic += 1;
                    }else if (currentMusic == 2)
                    {
                        audioSource.PlayOneShot(Plugin.saferoomTheme);
                        musicPlayTimer = 0;
                        currentMusic = 0;
                    }
                    //else if (currentMusic == 3)
                    //{
                    //    audioSource.PlayOneShot(Plugin.cootieTheme);
                    //    musicPlayTimer = 0;
                    //    currentMusic = 0;
                    //}
                }
            }
        }
    }
}
