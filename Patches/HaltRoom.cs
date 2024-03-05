using DunGen.Tags;
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
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LCOffice.Patches
{
    public class HaltRoom : MonoBehaviour
    {
        public static InteractTrigger haltEnterTrigger;
        public static Animator haltAnimator;
        public static Transform halt1;
        public static Transform halt2;
        public static Transform halt1Pos;
        public static Transform halt2Pos;
        public static Transform fogContainer;
        public float haltTimer;
        public float haltLocalTimer;
        public float haltTime;
        public float haltSpeed;
        public static bool isInHaltSequance;

        void Start()
        {
            GameObject.Instantiate(Plugin.haltRoom, new Vector3(this.transform.position.x, -500, this.transform.position.z), this.transform.rotation);
            haltEnterTrigger = GameObject.Find("HaltEnterTrigger").GetComponent<InteractTrigger>();
            haltEnterTrigger.onInteract.AddListener(HaltEnterTrigger);
            haltAnimator = GameObject.Find("HaltContainer").GetComponent<Animator>();
            fogContainer = GameObject.Find("FogContainer").transform;
            halt1 = GameObject.Find("Halt1Object").transform;
            halt2 = GameObject.Find("Halt2Object").transform;
            halt1Pos = GameObject.Find("Halt1Pos").transform;
            halt2Pos = GameObject.Find("Halt2Pos").transform;
            halt1.GetComponent<AudioSource>().volume = 0;
            halt2.GetComponent<AudioSource>().volume = 0;
            halt1.GetChild(2).GetComponent<InteractTrigger>().onInteract.AddListener(HaltTouchTrigger);
            halt2.GetChild(2).GetComponent<InteractTrigger>().onInteract.AddListener(HaltTouchTrigger);
            GameObject.Find("HaltEscapeTrigger1").GetComponent<InteractTrigger>().onInteract.AddListener(Teleport1);
            GameObject.Find("HaltEscapeTrigger2").GetComponent<InteractTrigger>().onInteract.AddListener(Teleport2);
            haltTime = UnityEngine.Random.Range(2, 6);
        }

        public void HaltEnterTrigger(PlayerControllerB playerController)
        {
            if (!playerController.isCameraDisabled && (UnityEngine.Random.Range(0, 5) == 2 || Plugin.configForceHalt))
            {
                /*
                GameObject glitchSound = GameObject.Instantiate(Plugin.glitchSound, playerController.transform.position, playerController.transform.rotation);
                glitchSound.GetComponent<NetworkObject>().Spawn();
                StartCoroutine(DestroyObjectDelay(glitchSound));
                */
                playerController.DropAllHeldItems(true, false);
                StartOfRound.Instance.localPlayerController.transform.position = GameObject.Find("HaltTeleportPoint").transform.position;
                OfficeRoundSystem.haltNoiseScreen.SetBool("Noise", true);
                OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", true);
                OfficeRoundSystem.haltMusicAudio.clip = Plugin.haltMusic;
                OfficeRoundSystem.haltMusicAudio.loop = true;
                OfficeRoundSystem.haltMusicAudio.Play();
                haltAnimator.SetInteger("halt", 1);
                //HUDManager.Instance.AttemptScanNewCreature(61);
                haltEnterTrigger.interactable = false;
                isInHaltSequance = true;
            }
        }

        void Update()
        {
            if (isInHaltSequance)
            {
                fogContainer.position = StartOfRound.Instance.localPlayerController.transform.position;
                Plugin.mls.LogInfo(Vector3.Distance(halt1Pos.position, StartOfRound.Instance.localPlayerController.transform.position) + "is one, two is " +
                    Vector3.Distance(halt2Pos.position, StartOfRound.Instance.localPlayerController.transform.position));
                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    OfficeRoundSystem.haltMusicAudio.Stop();
                    OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", false);
                    haltLocalTimer = 0;
                    halt1.GetComponent<AudioSource>().volume = 0;
                    halt2.GetComponent<AudioSource>().volume = 0;
                    isInHaltSequance = false;
                }
                haltSpeed += Time.deltaTime;
                haltAnimator.SetFloat("speed", 1f + (haltSpeed * 0.02f));
                halt1.GetComponent<AudioSource>().volume = 1;
                halt2.GetComponent<AudioSource>().volume = 1;
                haltLocalTimer += Time.deltaTime;
                if (haltLocalTimer > 2)
                {
                    OfficeRoundSystem.haltNoiseScreen.SetBool("Noise", false);
                }
                if (haltAnimator.GetInteger("halt") == 1)
                {
                    Plugin.mls.LogInfo(haltLocalTimer + ", " + haltTime);
                    float distance = Vector3.Distance(halt1Pos.position, StartOfRound.Instance.localPlayerController.transform.position);
                    float halt2Distance = Vector3.Distance(halt2Pos.position, StartOfRound.Instance.localPlayerController.transform.position);
                    if (haltLocalTimer == 0)
                    {
                        if (distance > 36)
                        {
                            haltAnimator.transform.position = new Vector3(haltAnimator.transform.position.x, haltAnimator.transform.position.y, StartOfRound.Instance.localPlayerController.transform.position.z);
                        }
                        else
                        {
                            haltAnimator.transform.position = new Vector3(haltAnimator.transform.position.x, haltAnimator.transform.position.y, 0);
                        }
                    }
                    if ((haltLocalTimer > haltTime + (0.1f * (10 + distance)) && halt2Distance > 2.5f) || distance > 50)
                    {
                        OfficeRoundSystem.haltNoiseScreen.SetTrigger("TurnBack");
                        int randomNoise = UnityEngine.Random.Range(0, 4);
                        if (randomNoise == 0)
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise1);
                        }else if (randomNoise == 1)
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise2);
                        }else if (randomNoise == 2)
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise3);
                        }else
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise4);
                        }
                        haltAnimator.SetInteger("halt", 2);
                        haltTime = UnityEngine.Random.Range(1, 5);
                        haltLocalTimer = 0;
                    }
                }else if (haltAnimator.GetInteger("halt") == 2)
                {
                    float distance = Vector3.Distance(halt2Pos.position, StartOfRound.Instance.localPlayerController.transform.position);
                    float halt1Distance = Vector3.Distance(halt1Pos.position, StartOfRound.Instance.localPlayerController.transform.position);
                    if (haltLocalTimer == 0)
                    {
                        if (distance > 36)
                        {
                            haltAnimator.transform.position = new Vector3(haltAnimator.transform.position.x, haltAnimator.transform.position.y, StartOfRound.Instance.localPlayerController.transform.position.z);
                        }
                        else
                        {
                            haltAnimator.transform.position = new Vector3(haltAnimator.transform.position.x, haltAnimator.transform.position.y, 0);
                        }
                    }
                    if ((haltLocalTimer > haltTime + (0.1f * (10 + distance)) && halt1Distance > 2.5f) || distance > 50)
                    {
                        int randomNoise = UnityEngine.Random.Range(0, 4);
                        OfficeRoundSystem.haltNoiseScreen.SetTrigger("TurnBack");
                        if (randomNoise == 0)
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise1);
                        }
                        else if (randomNoise == 1)
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise2);
                        }
                        else if (randomNoise == 2)
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise3);
                        }
                        else
                        {
                            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise4);
                        }
                        haltAnimator.SetInteger("halt", 1);
                        haltTime = UnityEngine.Random.Range(1, 5);
                        haltLocalTimer = 0;
                    }
                }
            }
        }

        public void HaltTouchTrigger(PlayerControllerB playerController)
        {
            StartCoroutine(HaltTouchTriggerCoroutine(playerController));
        }

        public void Teleport1(PlayerControllerB playerController)
        {
            OfficeRoundSystem.haltMusicAudio.Stop();
            OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", false);
            haltLocalTimer = 0;
            int randomNoise = UnityEngine.Random.Range(0, 4);
            if (randomNoise == 0)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise1);
            }
            else if (randomNoise == 1)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise2);
            }
            else if (randomNoise == 2)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise3);
            }
            else
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise4);
            }
            halt1.GetComponent<AudioSource>().volume = 0;
            halt2.GetComponent<AudioSource>().volume = 0;
            isInHaltSequance = false;
            OfficeRoundSystem.haltNoiseScreen.SetTrigger("NoiseOnce");
            playerController.transform.position = GameObject.Find("TP1").transform.position;
        }

        public void Teleport2(PlayerControllerB playerController)
        {
            OfficeRoundSystem.haltMusicAudio.Stop();
            OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", false);
            haltLocalTimer = 0;
            int randomNoise = UnityEngine.Random.Range(0, 4);
            if (randomNoise == 0)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise1);
            }
            else if (randomNoise == 1)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise2);
            }
            else if (randomNoise == 2)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise3);
            }
            else
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise4);
            }
            halt1.GetComponent<AudioSource>().volume = 0;
            halt2.GetComponent<AudioSource>().volume = 0;
            isInHaltSequance = false;
            OfficeRoundSystem.haltNoiseScreen.SetTrigger("NoiseOnce");
            playerController.transform.position = GameObject.Find("TP2").transform.position;
        }

        public IEnumerator HaltTouchTriggerCoroutine(PlayerControllerB playerWhoHit)
        {
            OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltAttack);
            OfficeRoundSystem.haltNoiseScreen.SetTrigger("Damage");
            int randomNoise = UnityEngine.Random.Range(0, 4);
            if (randomNoise == 0)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise1);
            }
            else if (randomNoise == 1)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise2);
            }
            else if (randomNoise == 2)
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise3);
            }
            else
            {
                OfficeRoundSystem.haltMusicAudio.PlayOneShot(Plugin.haltNoise4);
            }
            playerWhoHit.DamagePlayer(30, false, true, CauseOfDeath.Unknown, 0, false, default);
            playerWhoHit.transform.position = GameObject.Find("HaltTeleportPoint").transform.position;
            haltAnimator.SetInteger("halt", 0);
            haltLocalTimer = 0;
            haltTime = 4;
            yield return new WaitForSeconds(1f);
            playerWhoHit.transform.position = GameObject.Find("HaltTeleportPoint").transform.position;
            haltAnimator.SetInteger("halt", 1);
            yield break;
        }

        public IEnumerator DestroyObjectDelay(GameObject objectToDestroy)
        {
            yield return new WaitForSeconds(1.5f);
            GameObject.Destroy(objectToDestroy);
            yield break;
        }
    }
}
