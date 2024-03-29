using System;
using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem.HID;

namespace LCOffice.Patches
{
    public class ShrimpAI : EnemyAI
    {
        private void Awake()
        {
            this.agent = base.GetComponent<NavMeshAgent>();
        }

        public override void Start()
        {
            isNetworkTargetPlayer.Value = false;
            KillingPlayerBool.Value = false;
            SelectNode.Value = 0;
            shrimpVelocity.Value = 0f;
            hungerValue.Value = 0f;
            isHitted.Value = false;
            base.transform.GetChild(0).GetComponent<EnemyAICollisionDetect>().mainScript = this;
            enemyType = Plugin.shrimpEnemy;
            skinnedMeshRenderers = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            meshRenderers = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            thisNetworkObject = base.gameObject.GetComponentInChildren<NetworkObject>();
            serverPosition = base.transform.position;
            thisEnemyIndex = RoundManager.Instance.numberOfEnemiesInScene;
            RoundManager.Instance.numberOfEnemiesInScene++;
            allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            path1 = new NavMeshPath();
            mouth = GameObject.Find("ShrimpMouth").transform;
            leftEye = GameObject.Find("ShrimpLeftEye").transform;
            rightEye = GameObject.Find("ShrimpRightEye").transform;
            shrimpKillTrigger = GameObject.Find("ShrimpKillTrigger");
            creatureAnimator = base.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Animator>();
            creatureAnimator.SetTrigger("Walk");
            mainAudio = GameObject.Find("ShrimpMainAudio").GetComponent<AudioSource>();
            voiceAudio = GameObject.Find("ShrimpGrowlAudio").GetComponent<AudioSource>();
            voice2Audio = GameObject.Find("ShrimpAngerAudio").GetComponent<AudioSource>();
            lookRig = GameObject.Find("ShrimpLookAtPlayer").GetComponent<Rig>();
            lungLight = GameObject.Find("LungFlash").GetComponent<Light>();
            lungLight.intensity = 0f;
            AudioSource[] componentsInChildren = base.transform.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in componentsInChildren)
            {
                audioSource.outputAudioMixerGroup = GameObject.Find("StatusEffectAudio").GetComponent<AudioSource>().outputAudioMixerGroup;
            }
            lookTarget = GameObject.Find("Shrimp_Look_target").transform;
            dogHead = GameObject.Find("ShrimpLookPoint").transform;
            bittenObjectHolder = GameObject.Find("BittenObjectHolder").transform;
            shrimpEye = GameObject.Find("ShrimpEye").transform;
            scaleOfEyesNormally = leftEye.localScale;
            originalMouthScale = mouth.localScale;
            voice2Audio.clip = Plugin.dogSprint;
            voice2Audio.Play();
            creatureVoice = voice2Audio;
            creatureSFX = voice2Audio;
            eye = shrimpEye;
            SetupBehaviour();
            tempEnemyBehaviourStates.Add(roamingState);
            tempEnemyBehaviourStates.Add(followingPlayer);
            tempEnemyBehaviourStates.Add(enragedState);
            enemyBehaviourStates = tempEnemyBehaviourStates.ToArray();
            spawnPosition = base.transform.position;
            roamMap = new AISearchRoutine();
            ItemElevatorCheck[] array2 = UnityEngine.Object.FindObjectsOfType<ItemElevatorCheck>();
            foreach (ItemElevatorCheck itemElevatorCheck in array2)
            {
                itemElevatorCheck.shrimpAI = this;
            }
            bool setKorean = Plugin.setKorean;
            if (setKorean)
            {
                base.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "쉬림프";
            }
            ShrimpAI[] array4 = UnityEngine.Object.FindObjectsOfType<ShrimpAI>();
            foreach (ShrimpAI shrimpAI in array4)
            {
                if (shrimpAI != this)
                {
                    UnityEngine.Object.Destroy(shrimpAI.gameObject);
                }
            }
        }

        public IEnumerator stunnedTimer(PlayerControllerB playerWhoHit)
        {
            if (scaredBackingAway > 0f)
            {
                yield break;
            }
            ShrimpAI.isHitted.Value = false;
            hittedPlayer = playerWhoHit;
            agent.speed = 0f;
            creatureAnimator.SetTrigger("Recoil");
            mainAudio.PlayOneShot(Plugin.cry1, 1f);
            yield return new WaitForSeconds(0.5f);
            scaredBackingAway = 2f;
            yield return new WaitForSeconds(2f);
            hittedPlayer = null;
            yield break;
        }

        private void StunTest()
        {
            if (scaredBackingAway > 0f)
            {
                if (agent.enabled && base.IsOwner)
                {
                    lookTarget.position = Vector3.Lerp(lookTarget.position, base.ChooseFarthestNodeFromPosition(base.transform.position, true, 0, false).position, 10f * Time.deltaTime);
                    agent.SetDestination(base.ChooseFarthestNodeFromPosition(base.transform.position, true, 0, false).position);
                }
                scaredBackingAway -= Time.deltaTime;
            }
        }

        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit, false);
            if (ShrimpAI.hungerValue.Value < 60f && scaredBackingAway <= 0f)
            {
                ShrimpAI.isHitted.Value = true;
            }
        }

        public void FootStepSound()
        {
            randomVal = UnityEngine.Random.Range(0, 20);
            if (randomVal < 5f)
            {
                mainAudio.PlayOneShot(Plugin.footstep1, UnityEngine.Random.Range(0.8f, 1f));
            }
            else
            {
                if (randomVal < 10f)
                {
                    mainAudio.PlayOneShot(Plugin.footstep2, UnityEngine.Random.Range(0.8f, 1f));
                }
                else
                {
                    if (randomVal < 15f)
                    {
                        mainAudio.PlayOneShot(Plugin.footstep3, UnityEngine.Random.Range(0.8f, 1f));
                    }
                    else
                    {
                        if (randomVal < 20f)
                        {
                            mainAudio.PlayOneShot(Plugin.footstep4, UnityEngine.Random.Range(0.8f, 1f));
                        }
                    }
                }
            }
        }

        private IEnumerator DogSatisfied()
        {
            canBeMoved = false;
            yield return new WaitForSeconds(1f);
            if (!isKillingPlayer)
            {
                mainAudio.PlayOneShot(Plugin.dogSatisfied);
                creatureAnimator.SetTrigger("PlayBow");
            }
            yield return new WaitForSeconds(2f);
            canBeMoved = true;
            isNetworkTargetPlayer.Value = false;
            yield break;
        }

        public override void Update()
        {
            timeSinceLookingAtNoise += Time.deltaTime;
            footStepTime += Time.deltaTime * shrimpVelocity.Value / 8f;
            if (footStepTime > 0.5f)
            {
                FootStepSound();
                footStepTime = 0f;
            }
            if (!isSatisfied)
            {
                CheckPlayer();
            }
            creatureAnimator.SetFloat("walkSpeed", Mathf.Clamp(shrimpVelocity.Value / 5f, 0f, 3f));
            creatureAnimator.SetFloat("runSpeed", Mathf.Clamp(shrimpVelocity.Value / 2.7f, 3f, 4f));
            SetByHunger();
            if (isNetworkTargetPlayer.Value)
            {
                EatItem();
            }
            if (!isSatisfied)
            {
                CheckTargetAvailable();
            }
            if (isHitted.Value)
            {
                base.StartCoroutine(stunnedTimer(networkTargetPlayer.Value.GetPlayerController()));
            }
            if (base.IsOwner)
            {
                base.SyncPositionToClients();
            }
            else
            {
                base.SetClientCalculatingAI(false);
            }
            if (ateLung)
            {
                lungLight.intensity = Mathf.Lerp(lungLight.intensity, 1500f, Time.deltaTime * 10f);
            }
            if (satisfyValue >= 21f && !isSatisfied)
            {
                base.StartCoroutine(DogSatisfied());
                isSatisfied = true;
            }
            if (isSatisfied && satisfyValue > 0f)
            {
                satisfyValue -= Time.deltaTime;
                isSeenPlayer = false;
            }
            if (satisfyValue <= 0f && isSatisfied)
            {
                isSatisfied = false;
                droppedItems.Clear();
                satisfyValue = 0f;
            }
            if (!isNetworkTargetPlayer.Value && !isNearestItem && targetNode == null)
            {
                int num = UnityEngine.Random.Range(1, this.allAINodes.Length);
                if (Vector3.Distance(base.transform.position, this.allAINodes[num].transform.position) > 5f && ShrimpAI.SelectNode.Value != num && base.IsOwner)
                {
                    if (base.IsOwner)
                    {
                        ShrimpAI.SelectNode.Value = num;
                    }
                }
            }
            else
            {
                if (!isNetworkTargetPlayer.Value && !isNearestItem && targetNode != null)
                {
                    int num2 = UnityEngine.Random.Range(1, allAINodes.Length);
                    if (Vector3.Distance(base.transform.position, allAINodes[SelectNode.Value].transform.position) < 1f && SelectNode.Value != num2 && base.IsOwner)
                    {
                        if (base.IsOwner)
                        {
                            SelectNode.Value = num2;
                        }
                    }
                }
            }
            if (stuckDetectionTimer > 3.5f)
            {
                int num3 = UnityEngine.Random.Range(1, this.allAINodes.Length);
                if (Vector3.Distance(base.transform.position, this.allAINodes[num3].transform.position) > 5f && ShrimpAI.SelectNode.Value != num3)
                {
                    if (base.IsOwner)
                    {
                        SelectNode.Value = num3;
                    }
                    stuckDetectionTimer = 0f;
                }
                else
                {
                    if (Vector3.Distance(base.transform.position, this.allAINodes[num3].transform.position) > 5f)
                    {
                        stuckDetectionTimer = 0f;
                    }
                }
            }
            if ((!isNetworkTargetPlayer.Value && !isNearestItem) || isSatisfied)
            {
                if (timeSinceLookingAtNoise < 2f && scaredBackingAway <= 0f && !isNetworkTargetPlayer.Value && !isSatisfied)
                {
                    lookRig.weight = Mathf.Lerp(lookRig.weight, 1f, Time.deltaTime);
                    lookTarget.position = Vector3.Lerp(lookTarget.position, lookAtNoise, 10f * Time.deltaTime * 10f);
                    if (base.IsOwner)
                    {
                        Vector3 lookDirection = lookTarget.position - transform.position;
                        lookDirection.Normalize();
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 8 * Time.deltaTime);
                    }
                }
                else
                {
                    if (scaredBackingAway <= 0f)
                    {
                        lookRig.weight = Mathf.Lerp(lookRig.weight, 0f, Time.deltaTime);
                    }
                }
                if (targetNode != null)
                {
                    if (agent.enabled && base.IsOwner)
                    {
                        agent.SetDestination(targetNode.position);
                    }
                    if (shrimpVelocity.Value < 0.5f)
                    {
                        stuckDetectionTimer += Time.deltaTime;
                    }
                    else
                    {
                        stuckDetectionTimer = 0f;
                    }
                    Vector3 vector = base.transform.position - prevPosition;
                    float num5 = Vector3.Angle(vector, agent.velocity.normalized);
                    if (Vector3.Angle(vector, agent.velocity.normalized) > 30f)
                    {
                        Plugin.mls.LogInfo(string.Concat(new string[]
                        {
                            "angle diff: ",
                            num5.ToString(),
                            ", ",
                            "30",
                            " rad."
                        }));
                    }
                    prevPosition = base.transform.position;
                }
            }
            targetNode = allAINodes[ShrimpAI.SelectNode.Value].transform;
            if (isNetworkTargetPlayer.Value || isNearestItem)
            {
                dogRandomWalk = false;
                stuckDetectionTimer = 0f;
            }
            if (base.IsOwner)
            {
                networkPosition.Value = base.transform.position;
                networkRotation.Value = base.transform.rotation.eulerAngles;
                shrimpVelocity.Value = this.agent.velocity.sqrMagnitude;
            }
            else
            {
                this.networkPosDistance = Vector3.Distance(base.transform.position, networkPosition.Value);
                if (networkPosDistance > 3f)
                {
                    base.transform.position = networkPosition.Value;
                    Plugin.mls.LogWarning("Force the shrimp to change position.");
                }
                else
                {
                    base.transform.position = Vector3.Lerp(base.transform.position, networkPosition.Value, Time.deltaTime * 10f);
                }
                base.transform.rotation = Quaternion.Euler(Vector3.Lerp(base.transform.rotation.eulerAngles, networkRotation.Value, Time.deltaTime * 10f));
                if (networkPosDistance > 15f)
                {
                    Plugin.mls.LogFatal("Shrimp's current position is VERY far from the network position. This error typically occurs when network quality is low or the server is experiencing heavy traffic.");
                }
            }
            if (isNetworkTargetPlayer.Value)
            {
                isTargetAvailable = true;
            }
            if (hungerValue.Value < 55f)
            {
                leftEye.localScale = Vector3.Lerp(leftEye.localScale, scaleOfEyesNormally, 20f * Time.deltaTime);
                rightEye.localScale = Vector3.Lerp(rightEye.localScale, scaleOfEyesNormally, 20f * Time.deltaTime);
            }
            else
            {
                if (hungerValue.Value > 55f)
                {
                    leftEye.localScale = Vector3.Lerp(leftEye.localScale, scaleOfEyesNormally * 0.4f, 20f * Time.deltaTime);
                    rightEye.localScale = Vector3.Lerp(rightEye.localScale, scaleOfEyesNormally * 0.4f, 20f * Time.deltaTime);
                }
            }
            creatureAnimator.SetBool("DogRandomWalk", dogRandomWalk);
            if (!isRunning)
            {
                if (canBeMoved)
                {
                    creatureAnimator.SetBool("Running", false);
                    agent.speed = 5.5f;
                    agent.acceleration = 7f;
                    agent.angularSpeed = 150f;
                }
                else
                {
                    creatureAnimator.SetBool("Running", false);
                    agent.speed = 0f;
                    agent.angularSpeed = 0f;
                }
            }
            else
            {
                creatureAnimator.SetBool("Running", true);
                agent.speed = Mathf.Lerp(agent.speed, 15f, Time.deltaTime * 2f);
                agent.angularSpeed = 10000f;
                agent.acceleration = 50f;
            }
            StunTest();
        }

        private IEnumerator SyncRotation()
        {
            base.transform.rotation = Quaternion.Euler(Vector3.Lerp(base.transform.rotation.eulerAngles, ShrimpAI.networkRotation.Value, Time.deltaTime * 10f));
            yield return new WaitForSeconds(1f);
            yield break;
        }

        private float CalculateRotationDifference(Vector3 rotation1, Vector3 rotation2)
        {
            Quaternion quaternion = Quaternion.Euler(rotation1);
            Quaternion quaternion2 = Quaternion.Euler(rotation2);
            return Quaternion.Angle(quaternion, quaternion2);
        }

        private void CheckPlayer()
        {
            PlayerControllerB playerControllerB = base.CheckLineOfSightForPlayer(40f, 40, 2);
            if (playerControllerB != null && !isNetworkTargetPlayer.Value && !isKillingPlayer)
            {
                if (!isSeenPlayer)
                {
                    mainAudio.PlayOneShot(Plugin.dogHowl);
                    creatureAnimator.SetTrigger("Walk");
                    isSeenPlayer = true;
                }
                networkTargetPlayer.Value = playerControllerB.GetClientId();
                isNetworkTargetPlayer.Value = true;
            }
            if (droppedItems.Count > 0 && nearestDroppedItem == null)
            {
                FindNearestItem();
            }
            if (isNetworkTargetPlayer.Value)
            {
                if (!isNearestItem && scaredBackingAway <= 0f)
                {
                    if (hungerValue.Value > 55f)
                    {
                        lookRay = new Ray(dogHead.position, networkTargetPlayer.Value.GetPlayerController().transform.position - dogHead.position);
                        lookTarget.position = Vector3.Lerp(lookTarget.position, lookRay.GetPoint(3f), 30f * Time.deltaTime);
                    }
                    else
                    {
                        lookTarget.position = Vector3.Lerp(lookTarget.position, networkTargetPlayer.Value.GetPlayerController().lowerSpine.transform.position, 6f * Time.deltaTime);
                    }
                }
                agent.autoBraking = true;
                lookRig.weight = Mathf.Lerp(lookRig.weight, 1, Time.deltaTime);
                dogRandomWalk = false;
                if (!this.isSeenPlayer)
                {
                    this.mainAudio.PlayOneShot(Plugin.dogHowl, 1f);
                }
                this.isSeenPlayer = true;
            }
            if (!this.isRunning && !this.isNearestItem)
            {
                this.agent.stoppingDistance = 4.5f;
            }
            else
            {
                this.agent.stoppingDistance = 0.5f;
            }
        }

        private void SetByHunger()
        {
            if (hungerValue.Value < 66f)
            {
                if (isSeenPlayer && !isTargetAvailable && base.IsOwner)
                {
                    hungerValue.Value += Time.deltaTime * 0.09f;
                }
                else
                {
                    if (isSeenPlayer && isTargetAvailable && base.IsOwner)
                    {
                        hungerValue.Value += Time.deltaTime;
                    }
                }
            }
            if (hungerValue.Value > 55f && hungerValue.Value < 60f)
            {
                voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 1f, 45f * Time.deltaTime);
                voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 1f, 125f * Time.deltaTime);
                voiceAudio.loop = true;
                voiceAudio.clip = Plugin.stomachGrowl;
                if (!voiceAudio.isPlaying)
                {
                    voiceAudio.Play();
                }
            }
            else
            {
                if (hungerValue.Value < 63f && hungerValue.Value >= 60f && !isEnraging)
                {
                    voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 0.8f, 45f * Time.deltaTime);
                    voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 1f, 125f * Time.deltaTime);
                    isEnraging = true;
                    voiceAudio.clip = Plugin.bigGrowl;
                    if (!voiceAudio.isPlaying)
                    {
                        voiceAudio.Play();
                    }
                }
                else
                {
                    if (hungerValue.Value < 63f && hungerValue.Value > 60f && isEnraging)
                    {
                        voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 1f, 45f * Time.deltaTime);
                        voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 1f, 125f * Time.deltaTime);
                        bool flag9 = !isNearestItem;
                    }
                    else
                    {
                        if (hungerValue.Value < 55f)
                        {
                            voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 0f, 45f * Time.deltaTime);
                            voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 0f, 125f * Time.deltaTime);
                        }
                        else
                        {
                            if (hungerValue.Value > 63f && !isAngered)
                            {
                                if (!isNearestItem && !isKillingPlayer && stunNormalizedTimer <= 0f)
                                {
                                    canBeMoved = true;
                                }
                                voiceAudio.clip = Plugin.enragedScream;
                                voiceAudio.Play();
                                isEnraging = false;
                                isAngered = true;
                            }
                            else
                            {
                                if (hungerValue.Value > 65f)
                                {
                                    openDoorSpeedMultiplier = 1.5f;
                                    isRunning = true;
                                    voice2Audio.volume = Mathf.Lerp(voice2Audio.volume, 1f, 125f * Time.deltaTime);
                                }
                                else
                                {
                                    if (hungerValue.Value > 63f)
                                    {
                                        voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 0.8f, 45f * Time.deltaTime);
                                        mouth.localScale = Vector3.Lerp(mouth.localScale, new Vector3(0.005590725f, 0.01034348f, 0.02495567f), 30f * Time.deltaTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (hungerValue.Value < 60f)
            {
                isEnraging = false;
                isAngered = false;
                if (!isNearestItem && !isKillingPlayer && stunNormalizedTimer <= 0f && !isSatisfied)
                {
                    canBeMoved = true;
                }
            }
            if (hungerValue.Value < 66f)
            {
                isRunning = false;
                openDoorSpeedMultiplier = 0.3f;
            }
            if (hungerValue.Value < 63f)
            {
                mouth.localScale = Vector3.Lerp(mouth.localScale, originalMouthScale, 20f * Time.deltaTime);
                voice2Audio.volume = Mathf.Lerp(voice2Audio.volume, 0f, 125f * Time.deltaTime);
                isRunning = false;
            }
            if (hungerValue.Value > 64f && networkTargetPlayerDistance < 2f && !isKillingPlayer && isNetworkTargetPlayer.Value)
            {
                if (networkTargetPlayer.Value.GetPlayerController().isCameraDisabled)
                {
                    KillingPlayerBool.Value = true;
                    Plugin.mls.LogInfo("(Shrimp) Called KillPlayer");
                }
            }
            if (KillingPlayerBool.Value && isNetworkTargetPlayer.Value)
            {
                if (!networkTargetPlayer.Value.GetPlayerController().isCameraDisabled)
                {
                    base.StartCoroutine(KillPlayer(networkTargetPlayer.Value.GetPlayerController()));
                    Plugin.mls.LogInfo("(Shrimp) Triggering KillPlayer");
                    KillingPlayerBool.Value = false;
                }
                else
                {
                    if (networkTargetPlayer.Value.GetPlayerController().isCameraDisabled)
                    {
                        base.StartCoroutine(KillPlayerInOtherClient(networkTargetPlayer.Value.GetPlayerController()));
                    }
                }
            }
            if (isKillingPlayer)
            {
                canBeMoved = false;
            }
        }

        private void EatItem()
        {
            if (nearestDroppedItem != null)
            {
                if (isNearestItem && !nearestDroppedItem.GetComponent<GrabbableObject>().isHeld)
                {
                    if (hungerValue.Value < 55f)
                    {
                        lookRay = new Ray(dogHead.position, nearestDroppedItem.transform.position - dogHead.position);
                        lookTarget.position = Vector3.Lerp(lookTarget.position, lookRay.GetPoint(1.8f), 6f * Time.deltaTime);
                    }
                    else
                    {
                        lookTarget.position = Vector3.Lerp(lookTarget.position, nearestDroppedItem.transform.position, 6f * Time.deltaTime);
                    }
                    if (agent.enabled && base.IsOwner)
                    {
                        agent.SetDestination(nearestDroppedItem.transform.position);
                    }
                    nearestItemDistance = Vector3.Distance(base.transform.position, nearestDroppedItem.transform.position);
                    if (nearestItemDistance < 1.35f)
                    {
                        canBeMoved = false;
                        isRunning = false;
                        nearestDroppedItem.transform.SetParent(base.transform, true);
                        if (nearestDroppedItem.GetComponent<LungProp>() != null)
                        {
                            satisfyValue += 30f;
                            if (base.IsOwner)
                            {
                                hungerValue.Value -= 50f;
                            }
                            ateLung = true;
                            nearestDroppedItem.GetComponentInChildren<Light>().enabled = false;
                        }
                        else
                        {
                            if (nearestDroppedItem.GetComponent<StunGrenadeItem>() != null)
                            {
                                StunGrenadeItem component = nearestDroppedItem.GetComponent<StunGrenadeItem>();
                                component.hasExploded = true;
                                base.StartCoroutine(EatenFlashbang());
                            }
                        }
                        creatureAnimator.SetTrigger("eat");
                        mainAudio.PlayOneShot(Plugin.dogEatItem);
                        isNearestItem = false;
                        nearestItemDistance = 500f;
                        if (nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight > 1f)
                        {
                            satisfyValue += Mathf.Clamp(nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight - 1f, 0f, 100f) * 150f;
                            if (base.IsOwner)
                            {
                                hungerValue.Value -= Mathf.Clamp(nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight - 1f, 0f, 100f) * 230f;
                            }
                        }
                        else
                        {
                            satisfyValue += 6f;
                            if (base.IsOwner)
                            {
                                hungerValue.Value -= 12f;
                            }
                        }
                        GrabbableObject component2 = nearestDroppedItem.GetComponent<GrabbableObject>();
                        component2.grabbable = false;
                        component2.grabbableToEnemies = false;
                        component2.deactivated = true;
                        if (component2.radarIcon != null)
                        {
                            UnityEngine.Object.Destroy(component2.radarIcon.gameObject);
                        }
                        MeshRenderer[] componentsInChildren = component2.gameObject.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < componentsInChildren.Length; i++)
                        {
                            UnityEngine.Object.Destroy(componentsInChildren[i]);
                        }
                        Collider[] componentsInChildren2 = component2.gameObject.GetComponentsInChildren<Collider>();
                        for (int j = 0; j < componentsInChildren2.Length; j++)
                        {
                            UnityEngine.Object.Destroy(componentsInChildren2[j]);
                        }
                        droppedItems.Remove(nearestDroppedItem);
                        nearestDroppedItem = null;
                    }
                    else
                    {
                        if (!isKillingPlayer && stunNormalizedTimer <= 0f)
                        {
                            canBeMoved = true;
                        }
                    }
                }
                else
                {
                    if (isNetworkTargetPlayer.Value && !isSatisfied)
                    {
                        nearestDroppedItem = null;
                        nearestItemDistance = 3000f;
                        isNearestItem = false;
                    }
                }
            }
            if (stunNormalizedTimer > 0f)
            {
                canBeMoved = false;
                creatureAnimator.SetBool("Stun", true);
            }
            else
            {
                creatureAnimator.SetBool("Stun", false);
            }
        }

        private void CheckTargetAvailable()
        {
            if ((networkTargetPlayerDistance <= 12f || base.CheckLineOfSightForPlayer(40f, 40, -1)) && !isNearestItem && !isKillingPlayer && scaredBackingAway <= 0f)
            {
                followTimer = 10f;
                canBeMoved = true;
            }
            if (isNetworkTargetPlayer.Value)
            {
                networkTargetPlayerDistance = Vector3.Distance(base.transform.position, ShrimpAI.networkTargetPlayer.Value.GetPlayerController().transform.position);
            }
            else
            {
                networkTargetPlayerDistance = 3000f;
            }
            if ((followTimer > 0f || networkTargetPlayerDistance < 10f) && !isSatisfied)
            {
                if (isTargetAvailable && scaredBackingAway <= 0f && !isNearestItem && agent.enabled && base.IsOwner)
                {
                    if (networkTargetPlayerDistance > 2.1f)
                    {
                        agent.SetDestination(networkTargetPlayer.Value.GetPlayerController().transform.position);
                    }
                    else
                    {
                        if (hungerValue.Value < 55f)
                        {
                            BackAway();
                        }
                    }
                }
            }
            if (((isNetworkTargetPlayer.Value && networkTargetPlayerDistance > 12f) || !base.CheckLineOfSightForPlayer(40f, 30, -1)) && followTimer > 0f)
            {
                followTimer -= Time.deltaTime;
            }
            if (!isNetworkTargetPlayer.Value)
            {
                followTimer = 10f;
            }
            if (followTimer <= 0f)
            {
                isNetworkTargetPlayer.Value = false;
                isTargetAvailable = false;
            }
        }

        private bool IsPlayerVisible(PlayerControllerB player)
        {
            Vector3 vector = player.transform.position - base.transform.position;
            if (Vector3.Angle(base.transform.forward, vector) < 35f)
            {
                if (Physics.Raycast(base.transform.position, vector, 120f, LayerMask.NameToLayer("Player")))
                {
                    return true;
                }
            }
            return false;
        }

        private void FindNearestItem()
        {
            foreach (GameObject gameObject in droppedItems)
            {
                float num = Vector3.Distance(base.transform.position, gameObject.transform.position);
                if (Vector3.Distance(base.transform.position, gameObject.transform.position) < float.PositiveInfinity && num < 30f)
                {
                    if (!gameObject.GetComponent<GrabbableObject>().isHeld)
                    {
                        nearestDroppedItem = gameObject;
                        isNearestItem = true;
                        return;
                    }
                }
            }
            isNearestItem = false;
        }

        public override void DetectNoise(Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot = 0, int noiseID = 0)
        {
            base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
            float num = Vector3.Distance(noisePosition, base.transform.position);
            if (num <= 15f)
            {
                if (Physics.Linecast(this.eye.position, noisePosition, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    noiseLoudness /= 2f;
                }
                if ((noiseLoudness / num) > 0.045f)
                {
                    if (timeSinceLookingAtNoise > 5f)
                    {
                        timeSinceLookingAtNoise = 0f;
                        lookAtNoise = noisePosition;
                    }
                }
            }
        }

        private void BackAway()
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                float num = Vector3.Distance(base.transform.position, player.transform.position);
                if (Vector3.Distance(base.transform.position, player.transform.position) < float.PositiveInfinity && num < 30f)
                {
                    if (!player.isPlayerDead)
                    {
                        nearestPlayer = player;
                    }
                }
            }
            Vector3 position;
            if (isNetworkTargetPlayer.Value)
            {
                agent.destination = networkTargetPlayer.Value.GetPlayerController().transform.position;
                position = networkTargetPlayer.Value.GetPlayerController().transform.position;
                position.y = base.transform.position.y;
            }else
            { 
                agent.destination = nearestPlayer.transform.position;
                position = nearestPlayer.transform.position;
                position.y = base.transform.position.y;
            }
            Vector3 vector = position - base.transform.position;
            backAwayRay = new Ray(base.transform.position, vector * -1f);
            if (Physics.Raycast(backAwayRay, out hitInfo, 60f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                if (hitInfo.distance < 4f)
                {
                    if (Physics.Linecast(base.transform.position, hitInfo.point + Vector3.Cross(vector, Vector3.up) * 25.5f, out hitInfoB, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                    {
                        float distance = hitInfoB.distance;
                        if (Physics.Linecast(base.transform.position, hitInfo.point + Vector3.Cross(vector, Vector3.up) * -25.5f, out hitInfoB, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                        {
                            float distance2 = hitInfoB.distance;
                            if (Mathf.Abs(distance - distance2) < 5f)
                            {
                                agent.destination = hitInfo.point + Vector3.Cross(vector, Vector3.up) * -4.5f;
                            }
                            else
                            {
                                if (distance < distance2)
                                {
                                    agent.destination = hitInfo.point + Vector3.Cross(vector, Vector3.up) * -4.5f;
                                }
                                else
                                {
                                    agent.destination = hitInfo.point + Vector3.Cross(vector, Vector3.up) * 4.5f;
                                }
                            }
                        }
                    }
                }
                else
                {
                    agent.destination = hitInfo.point;
                }
            }
            else
            {
                agent.destination = backAwayRay.GetPoint(2.3f);
            }
            agent.stoppingDistance = 0.2f;
            Quaternion quaternion = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(vector), 3f * Time.deltaTime);
            base.transform.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
            agent.speed = 8f;
            agent.acceleration = 50000;
            creatureAnimator.SetFloat("walkSpeed", -2.2f);
        }

        private IEnumerator EatenFlashbang()
        {
            yield return new WaitForSeconds(2f);
            this.mainAudio.PlayOneShot(Plugin.eatenExplode);
            yield break;
        }

        private void KillPlayerTrigger(PlayerControllerB killPlayer)
        {
            if (!killPlayer.isCameraDisabled)
            {
                base.StartCoroutine(this.KillPlayer(killPlayer));
                Plugin.mls.LogInfo("(Shrimp) Triggered KillPlayer");
            }
            else
            {
                base.StartCoroutine(this.KillPlayerInOtherClient(killPlayer));
            }
        }

        private IEnumerator KillPlayer(PlayerControllerB killPlayer)
        {
            hungerValue.Value = 0f;
            yield return new WaitForSeconds(0.05f);
            isKillingPlayer = true;
            creatureAnimator.SetTrigger("RipObject");
            mainAudio.PlayOneShot(Plugin.ripPlayerApart);
            if (GameNetworkManager.Instance.localPlayerController == killPlayer)
            {
                Plugin.mls.LogInfo("(Shrimp) Killing Player");
                killPlayer.KillPlayer(Vector3.zero, true, CauseOfDeath.Mauling, 0);
            }
            yield return new WaitForSeconds(0.1f);
            if (killPlayer.deadBody == null)
            {
                Debug.Log("Shrimp: Player body was not spawned or found within 0.5 seconds.");
                killPlayer.inAnimationWithEnemy = null;
                isKillingPlayer = false;
                KillingPlayerBool.Value = false;
                yield break;
            }
            TakeBodyInMouth(killPlayer.deadBody);
            yield return new WaitForSeconds(4.4f);
            creatureAnimator.SetTrigger("eat");
            mainAudio.PlayOneShot(Plugin.dogEatItem);
            killPlayer.deadBody.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            isKillingPlayer = false;
            yield break;
        }

        private IEnumerator KillPlayerInOtherClient(PlayerControllerB killPlayer)
        {
            hungerValue.Value = 0f;
            yield return new WaitForSeconds(0.05f);
            isKillingPlayer = true;
            creatureAnimator.SetTrigger("RipObject");
            mainAudio.PlayOneShot(Plugin.ripPlayerApart);
            yield return new WaitForSeconds(0.1f);
            if (killPlayer.deadBody == null)
            {
                Debug.Log("Shrimp: Player body was not spawned or found within 0.5 seconds.");
                killPlayer.inAnimationWithEnemy = null;
                isKillingPlayer = false;
                KillingPlayerBool.Value = false;
                yield break;
            }
            TakeBodyInMouth(killPlayer.deadBody);
            yield return new WaitForSeconds(4.4f);
            creatureAnimator.SetTrigger("eat");
            mainAudio.PlayOneShot(Plugin.dogEatItem);
            killPlayer.deadBody.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            isKillingPlayer = false;
            yield break;
        }

        private void TakeBodyInMouth(DeadBodyInfo body)
        {
            body.attachedTo = bittenObjectHolder;
            body.attachedLimb = body.bodyParts[5];
            body.matchPositionExactly = true;
        }

        private void SetupBehaviour()
        {
            roamingState.name = "Roaming";
            roamingState.boolValue = true;
            followingPlayer.name = "Following";
            followingPlayer.boolValue = true;
            enragedState.name = "Enraged";
            enragedState.boolValue = true;
        }

        public float networkPosDistance;

        public Vector3 prevPosition;

        public float stuckDetectionTimer;

        public float prevPositionDistance;

        public AISearchRoutine roamMap = new AISearchRoutine();

        private Vector3 spawnPosition;

        public PlayerControllerB hittedPlayer;

        [PublicNetworkVariable]
        public static LethalNetworkVariable<bool> KillingPlayerBool = new LethalNetworkVariable<bool>("KillingPlayerBool");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<int> SelectNode = new LethalNetworkVariable<int>("SelectNode");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<float> shrimpVelocity = new LethalNetworkVariable<float>("shrimpVelocity");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<float> hungerValue = new LethalNetworkVariable<float>("hungerValue");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<bool> isHitted = new LethalNetworkVariable<bool>("isHitted");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<Vector3> networkPosition = new LethalNetworkVariable<Vector3>("networkPosition");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<Vector3> networkRotation = new LethalNetworkVariable<Vector3>("networkRotation");

        public static LethalNetworkVariable<ulong> networkTargetPlayer = new LethalNetworkVariable<ulong>("networkTargetPlayer");

        public static PlayerControllerB nearestPlayer;

        public static LethalNetworkVariable<bool> isNetworkTargetPlayer = new LethalNetworkVariable<bool>("isNetworkTargetPlayer");

        public bool isKillingPlayer;

        public bool isSeenPlayer;

        public bool isEnraging;

        public bool isAngered;

        public bool canBeMoved;

        public bool isRunning;

        public bool dogRandomWalk;

        public float footStepTime;

        public float randomVal;

        public bool isTargetAvailable;

        public float networkTargetPlayerDistance;

        public float nearestItemDistance;

        public bool isNearestItem;

        public List<GameObject> droppedItems = new List<GameObject>();

        public GameObject nearestDroppedItem;

        public Transform dogHead;

        public Ray lookRay;

        public Transform lookTarget;

        public BoxCollider[] allBoxCollider;

        public Transform IdleTarget;

        public bool isIdleTargetAvailable;

        public bool forceChangeTarget;

        public Rig lookRig;

        public Light lungLight;

        public bool ateLung;

        public bool isSatisfied;

        public float satisfyValue;

        public Transform leftEye;

        public Transform rightEye;

        public Transform shrimpEye;

        public Transform mouth;

        public GameObject shrimpKillTrigger;

        public Transform bittenObjectHolder;

        public float searchingForObjectTimer;

        private Vector3 scaleOfEyesNormally;

        public AudioSource mainAudio;

        public AudioSource voiceAudio;

        public AudioSource voice2Audio;

        public AudioSource dogMusic;

        public AudioSource sprintAudio;

        public Vector3 originalMouthScale;

        public float scaredBackingAway;

        public Ray backAwayRay;

        private RaycastHit hitInfo;

        private RaycastHit hitInfoB;

        public float followTimer;

        public EnemyBehaviourState roamingState;

        public EnemyBehaviourState followingPlayer;

        public EnemyBehaviourState enragedState;

        public List<EnemyBehaviourState> tempEnemyBehaviourStates;

        public List<SkinnedMeshRenderer> skinnedMeshRendererList;

        public List<MeshRenderer> meshRendererList;

        private float timeSinceLookingAtNoise;

        private Vector3 lookAtNoise;
    }
}
