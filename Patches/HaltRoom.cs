using DunGen.Tags;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using LethalNetworkAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static float fullDarknessIntensity;

        public static LethalClientEvent HaltEnterTriggerEvent = new LethalClientEvent(identifier: "haltEnterTriggerEvent", onReceivedFromClient: HaltEnterTrigger);
        public static void HaltEnterTrigger(ulong id)
        {
            id.GetPlayerController().DropAllHeldItems();
            id.GetPlayerController().transform.position = GameObject.Find("HaltTeleportPoint").transform.position;
        }
        void Start()
        {
            isInHaltSequance = false;
            GameObject.Instantiate(Plugin.haltRoom, new Vector3(this.transform.position.x - 2500, this.transform.position.y, this.transform.position.z), this.transform.rotation);
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
            fullDarknessIntensity = StartOfRound.Instance.localPlayerController.nightVision.intensity;
        }

        public void HaltEnterTrigger(PlayerControllerB playerController)
        {
            if (!isInHaltSequance && !playerController.isCameraDisabled && (UnityEngine.Random.Range(0, 101) < Plugin.configHaltPropability))
            {
                HaltEnterTriggerEvent.InvokeAllClients();
                OfficeRoundSystem.haltNoiseScreen.SetBool("Noise", true);
                if (!Plugin.configDisableCameraShake)
                {
                    OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", true);
                }
                OfficeRoundSystem.haltMusicAudio.clip = Plugin.haltMusic;
                OfficeRoundSystem.haltMusicAudio.loop = true;
                OfficeRoundSystem.haltMusicAudio.Play();
                OfficeRoundSystem.haltMusicAudio.volume = 1;
                haltAnimator.SetInteger("halt", 1);
                /*
                GameObject glitchSound = GameObject.Instantiate(Plugin.glitchSound, playerController.transform.position, playerController.transform.rotation);
                glitchSound.GetComponent<NetworkObject>().Spawn();
                StartCoroutine(DestroyObjectDelay(glitchSound));
                */
                //HUDManager.Instance.AttemptScanNewCreature(61);
                isInHaltSequance = true;
                if (Plugin.configDiversityHaltBrighness)
                {
                    StartOfRound.Instance.localPlayerController.nightVision.intensity = 600;
                }
                haltEnterTrigger.interactable = false;
                GameObject.Destroy(haltEnterTrigger.gameObject);
            }
        }

        void Update()
        {
            if (isInHaltSequance)
            {
                /*
                float h1Distance = Vector3.Distance(halt1Pos.position, StartOfRound.Instance.localPlayerController.transform.position);
                float h2distance = Vector3.Distance(halt2Pos.position, StartOfRound.Instance.localPlayerController.transform.position);
                Plugin.mls.LogInfo("halt1Distance: " + h1Distance + " halt2Distance: " + h2distance);
                */
                StartOfRound.Instance.localPlayerController.sprintMeter += Time.deltaTime * 0.2f;
                fogContainer.position = StartOfRound.Instance.localPlayerController.transform.position;
                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    OfficeRoundSystem.haltMusicAudio.Stop();
                    OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", false);
                    OfficeRoundSystem.haltMusicAudio.volume = 0;
                    halt1.GetComponent<AudioSource>().volume = 0;
                    halt2.GetComponent<AudioSource>().volume = 0;
                    isInHaltSequance = false;
                    haltLocalTimer = 0;
                    haltTimer = 0;
                    StartOfRound.Instance.localPlayerController.nightVision.intensity = fullDarknessIntensity;
                }
                haltSpeed += Time.deltaTime;
                if (haltAnimator.GetFloat("speed") < 2f)
                {
                    haltAnimator.SetFloat("speed", 1f + (haltSpeed * 0.02f));
                }
                halt1.GetComponent<AudioSource>().volume = 1;
                halt2.GetComponent<AudioSource>().volume = 1;
                haltLocalTimer += Time.deltaTime;
                haltTimer += Time.deltaTime;
                if (haltLocalTimer > 2)
                {
                    OfficeRoundSystem.haltNoiseScreen.SetBool("Noise", false);
                }
                if (haltAnimator.GetInteger("halt") == 1)
                {
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
                    if (haltLocalTimer > haltTime + (0.01f * (100 - distance * 2) + (haltTimer * 0.02f)) && halt2Distance > 4f)
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
                        if (haltTimer > 10)
                        {
                            if (halt2Distance < 20)
                            {
                                haltTime = UnityEngine.Random.Range(2, 8);
                            }
                            else if (halt2Distance > 20)
                            {
                                haltTime = UnityEngine.Random.Range(4, 11);
                            }
                        }else
                        {
                            haltTime = UnityEngine.Random.Range(3, 7);
                        }
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
                    if (haltLocalTimer > haltTime + (0.1f * (100 - distance * 2) + (haltTimer * 0.02f)) && halt1Distance > 4f)
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
                        if (haltTimer > 10)
                        {
                            if (halt1Distance < 20)
                            {
                                haltTime = UnityEngine.Random.Range(2, 8);
                            }
                            else if (halt1Distance > 20)
                            {
                                haltTime = UnityEngine.Random.Range(4, 11);
                            }
                        }
                        else
                        {
                            haltTime = UnityEngine.Random.Range(3, 7);
                        }
                        haltLocalTimer = 0;
                    }
                }
            }else
            {
                halt1.GetComponent<AudioSource>().volume = 0;
                halt2.GetComponent<AudioSource>().volume = 0;
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
            StartOfRound.Instance.localPlayerController.nightVision.intensity = fullDarknessIntensity;
        }

        public void Teleport2(PlayerControllerB playerController)
        {
            OfficeRoundSystem.haltMusicAudio.Stop();
            OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", false);
            haltLocalTimer = 0;
            int randomNoise = UnityEngine.Random.Range(2, 8);
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
            StartOfRound.Instance.localPlayerController.nightVision.intensity = fullDarknessIntensity;
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

        void OnDestroy()
        {
            OfficeRoundSystem.haltMusicAudio.Stop();
            OfficeRoundSystem.playerScreenAnimator.SetBool("rotateLoop", false);
            OfficeRoundSystem.haltMusicAudio.volume = 0;
            halt1.GetComponent<AudioSource>().volume = 0;
            halt2.GetComponent<AudioSource>().volume = 0;
            isInHaltSequance = false;
            haltLocalTimer = 0;
            haltTimer = 0;
            StartOfRound.Instance.localPlayerController.nightVision.intensity = fullDarknessIntensity;
        }
    }
}
