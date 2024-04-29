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
            SelectNode.Value = 0;
            shrimpVelocity.Value = 0f;
            hungerValue.Value = 0f;
            isHitted.Value = false;
            //base.transform.GetChild(0).GetComponent<EnemyAICollisionDetect>().mainScript = this;
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
            ShrimpAI[] array4 = GameObject.FindObjectsOfType<ShrimpAI>();
            foreach (ShrimpAI shrimpAI in array4)
            {
                if (shrimpAI != this)
                {
                    GameObject.Destroy(shrimpAI.gameObject);
                }
            }
        }

        public IEnumerator stunnedTimer(PlayerControllerB playerWhoHit)
        {
            if (scaredBackingAway > 0f)
            {
                yield break;
            }
            isHitted.Value = false;
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
            if (hungerValue.Value < 60f && this.scaredBackingAway <= 0f)
            {
                isHitted.Value = true;
            }
        }

        public void FootStepSound()
        {
            randomVal = UnityEngine.Random.Range(0, 5);

            switch (randomVal)
            {
                case 0:
                    mainAudio.PlayOneShot(Plugin.footstep1, UnityEngine.Random.Range(0.8f, 1f));
                    break;
                case 1:
                    mainAudio.PlayOneShot(Plugin.footstep2, UnityEngine.Random.Range(0.8f, 1f));
                    break;
                case 2:
                    mainAudio.PlayOneShot(Plugin.footstep3, UnityEngine.Random.Range(0.8f, 1f));
                    break;
                case 3:
                    mainAudio.PlayOneShot(Plugin.footstep4, UnityEngine.Random.Range(0.8f, 1f));
                    break;
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
            bool isOwner = base.IsOwner;
            if (isOwner)
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
                if (Vector3.Distance(base.transform.position, allAINodes[num].transform.position) > 5f && SelectNode.Value != num && base.IsOwner)
                {
                    if (base.IsOwner)
                    {
                        SelectNode.Value = num;
                    }
                }
            }
            else
            {
                bool flag11 = !isNetworkTargetPlayer.Value && !this.isNearestItem && this.targetNode != null;
                if (flag11)
                {
                    int num2 = UnityEngine.Random.Range(1, this.allAINodes.Length);
                    bool flag12 = Vector3.Distance(base.transform.position, this.allAINodes[SelectNode.Value].transform.position) < 1f && SelectNode.Value != num2 && base.IsOwner;
                    if (flag12)
                    {
                        if (base.IsOwner)
                        {
                            SelectNode.Value = num2;
                        }
                    }
                }
            }
            bool flag13 = stuckDetectionTimer > 3.5f;
            if (flag13)
            {
                int num3 = UnityEngine.Random.Range(1, allAINodes.Length);
                bool flag14 = Vector3.Distance(base.transform.position, allAINodes[num3].transform.position) > 5f && ShrimpAI.SelectNode.Value != num3;
                if (flag14)
                {
                    bool isOwner4 = base.IsOwner;
                    if (isOwner4)
                    {
                        SelectNode.Value = num3;
                    }
                    stuckDetectionTimer = 0f;
                }
                else
                {
                    bool flag15 = Vector3.Distance(base.transform.position, allAINodes[num3].transform.position) > 5f;
                    if (flag15)
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
                    bool flag22 = Vector3.Angle(vector, agent.velocity.normalized) > 30f;
                    if (flag22)
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
                shrimpVelocity.Value = agent.velocity.sqrMagnitude;
            }
            else
            {
                networkPosDistance = Vector3.Distance(base.transform.position, ShrimpAI.networkPosition.Value);
                bool flag24 = networkPosDistance > 3f;
                if (flag24)
                {
                    base.transform.position = ShrimpAI.networkPosition.Value;
                    Plugin.mls.LogWarning("Force the shrimp to change position.");
                }
                else
                {
                    base.transform.position = Vector3.Lerp(base.transform.position, ShrimpAI.networkPosition.Value, Time.deltaTime * 10f);
                }
                base.transform.rotation = Quaternion.Euler(Vector3.Lerp(base.transform.rotation.eulerAngles, ShrimpAI.networkRotation.Value, Time.deltaTime * 10f));
                if (networkPosDistance > 15f)
                {
                    Plugin.mls.LogFatal("Shrimp spawned successfully, but the current position is VERY far from the network position. This error typically occurs when network quality is low or the server is experiencing heavy traffic.");
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
                ShrimpAI.networkTargetPlayer.Value = playerControllerB.GetClientId();
                isNetworkTargetPlayer.Value = true;
            }
            if (droppedItems.Count > 0 && nearestDroppedItem == null)
            {
                this.FindNearestItem();
            }
            if (isNetworkTargetPlayer.Value)
            {
                if (!isNearestItem && scaredBackingAway <= 0f)
                {
                    if (hungerValue.Value > 55f)
                    {
                        this.lookRay = new Ray(this.dogHead.position, ShrimpAI.networkTargetPlayer.Value.GetPlayerController().transform.position - this.dogHead.position);
                        this.lookTarget.position = Vector3.Lerp(this.lookTarget.position, this.lookRay.GetPoint(3f), 30f * Time.deltaTime);
                    }
                    else
                    {
                        this.lookTarget.position = Vector3.Lerp(this.lookTarget.position, ShrimpAI.networkTargetPlayer.Value.GetPlayerController().lowerSpine.transform.position, 6f * Time.deltaTime);
                    }
                }
                this.agent.autoBraking = true;
                this.lookRig.weight = Mathf.Lerp(this.lookRig.weight, 1, Time.deltaTime);
                this.dogRandomWalk = false;
                bool flag7 = !this.isSeenPlayer;
                if (flag7)
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
            bool flag9 = this.isNearestItem;
            if (flag9)
            {
            }
        }

        private void SetByHunger()
        {
            bool flag = ShrimpAI.hungerValue.Value < 66f;
            if (flag)
            {
                bool flag2 = this.isSeenPlayer && !this.isTargetAvailable && base.IsOwner;
                if (flag2)
                {
                    ShrimpAI.hungerValue.Value += Time.deltaTime * 0.09f;
                }
                else
                {
                    bool flag3 = this.isSeenPlayer && this.isTargetAvailable && base.IsOwner;
                    if (flag3)
                    {
                        ShrimpAI.hungerValue.Value += Time.deltaTime;
                    }
                }
            }
            bool flag4 = ShrimpAI.hungerValue.Value > 55f && ShrimpAI.hungerValue.Value < 60f;
            if (flag4)
            {
                this.voiceAudio.pitch = Mathf.Lerp(this.voiceAudio.pitch, 1f, 45f * Time.deltaTime);
                this.voiceAudio.volume = Mathf.Lerp(this.voiceAudio.volume, 1f, 125f * Time.deltaTime);
                this.voiceAudio.loop = true;
                this.voiceAudio.clip = Plugin.stomachGrowl;
                bool flag5 = !this.voiceAudio.isPlaying;
                if (flag5)
                {
                    this.voiceAudio.Play();
                }
            }
            else
            {
                bool flag6 = ShrimpAI.hungerValue.Value < 63f && ShrimpAI.hungerValue.Value >= 60f && !this.isEnraging;
                if (flag6)
                {
                    this.voiceAudio.pitch = Mathf.Lerp(this.voiceAudio.pitch, 0.8f, 45f * Time.deltaTime);
                    this.voiceAudio.volume = Mathf.Lerp(this.voiceAudio.volume, 1f, 125f * Time.deltaTime);
                    this.isEnraging = true;
                    this.voiceAudio.clip = Plugin.bigGrowl;
                    bool flag7 = !this.voiceAudio.isPlaying;
                    if (flag7)
                    {
                        this.voiceAudio.Play();
                    }
                }
                else
                {
                    if (hungerValue.Value < 63f && hungerValue.Value > 60f && isEnraging)
                    {
                        this.voiceAudio.pitch = Mathf.Lerp(this.voiceAudio.pitch, 1f, 45f * Time.deltaTime);
                        this.voiceAudio.volume = Mathf.Lerp(this.voiceAudio.volume, 1f, 125f * Time.deltaTime);
                        bool flag9 = !this.isNearestItem;
                    }
                    else
                    {
                        bool flag10 = ShrimpAI.hungerValue.Value < 55f;
                        if (flag10)
                        {
                            this.voiceAudio.pitch = Mathf.Lerp(this.voiceAudio.pitch, 0f, 45f * Time.deltaTime);
                            this.voiceAudio.volume = Mathf.Lerp(this.voiceAudio.volume, 0f, 125f * Time.deltaTime);
                        }
                        else
                        {
                            bool flag11 = ShrimpAI.hungerValue.Value > 63f && !this.isAngered;
                            if (flag11)
                            {
                                bool flag12 = !this.isNearestItem && !isKillingPlayer && this.stunNormalizedTimer <= 0f;
                                if (flag12)
                                {
                                    this.canBeMoved = true;
                                }
                                this.voiceAudio.clip = Plugin.enragedScream;
                                this.voiceAudio.Play();
                                this.isEnraging = false;
                                this.isAngered = true;
                            }
                            else
                            {
                                bool flag13 = hungerValue.Value > 65f;
                                if (flag13)
                                {
                                    openDoorSpeedMultiplier  = 1.5f;
                                    this.isRunning = true;
                                    this.voice2Audio.volume = Mathf.Lerp(this.voice2Audio.volume, 1f, 125f * Time.deltaTime);
                                }
                                else
                                {
                                    bool flag14 = ShrimpAI.hungerValue.Value > 63f;
                                    if (flag14)
                                    {
                                        this.voiceAudio.pitch = Mathf.Lerp(this.voiceAudio.pitch, 0.8f, 45f * Time.deltaTime);
                                        this.mouth.localScale = Vector3.Lerp(this.mouth.localScale, new Vector3(0.005590725f, 0.01034348f, 0.02495567f), 30f * Time.deltaTime);
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
            if (isKillingPlayer)
            {
                canBeMoved = false;
            }
        }

        public void CollideWithObject(GameObject other)
        {
            Plugin.mls.LogInfo("Collided with " + other.gameObject.name);
            if (!other.GetComponent<PlayerControllerB>())
            {
                return;
            }
            if (isKillingPlayer)
            {
                return;
            }
            if (hungerValue.Value < 64f)
            {
                return;
            }
            if (isEnemyDead)
            {
                return;
            }
            PlayerControllerB playerControllerB = other.GetComponent<PlayerControllerB>();
            if (playerControllerB != null)
            {
                KillPlayerServerRpc((int)playerControllerB.playerClientId);
            }
            Plugin.mls.LogInfo("(Shrimp) Called KillPlayer");
        }

        private void EatItem()
        {
            if (nearestDroppedItem != null)
            {
                if (this.isNearestItem && !this.nearestDroppedItem.GetComponent<GrabbableObject>().isHeld)
                {
                    if (hungerValue.Value < 55f)
                    {
                        this.lookRay = new Ray(this.dogHead.position, this.nearestDroppedItem.transform.position - this.dogHead.position);
                        this.lookTarget.position = Vector3.Lerp(this.lookTarget.position, this.lookRay.GetPoint(1.8f), 6f * Time.deltaTime);
                    }
                    else
                    {
                        this.lookTarget.position = Vector3.Lerp(this.lookTarget.position, this.nearestDroppedItem.transform.position, 6f * Time.deltaTime);
                    }
                    if (this.agent.enabled && base.IsOwner)
                    {
                        this.agent.SetDestination(this.nearestDroppedItem.transform.position);
                    }
                    this.nearestItemDistance = Vector3.Distance(base.transform.position, this.nearestDroppedItem.transform.position);
                    if (nearestItemDistance < 1.35f)
                    {
                        this.canBeMoved = false;
                        this.isRunning = false;
                        this.nearestDroppedItem.transform.SetParent(base.transform, true);
                        if (nearestDroppedItem.GetComponent<LungProp>() != null)
                        {
                            this.satisfyValue += 30f;
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
                                StunGrenadeItem component = this.nearestDroppedItem.GetComponent<StunGrenadeItem>();
                                component.hasExploded = true;
                                base.StartCoroutine(this.EatenFlashbang());
                            }
                        }
                        creatureAnimator.SetTrigger("eat");
                        mainAudio.PlayOneShot(Plugin.dogEatItem);
                        isNearestItem = false;
                        nearestItemDistance = 500f;
                        if (nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight > 1f)
                        {
                            satisfyValue += Mathf.Clamp(this.nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight - 1f, 0f, 100f) * 150f;
                            if (base.IsOwner)
                            {
                                hungerValue.Value -= Mathf.Clamp(this.nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight - 1f, 0f, 100f) * 230f;
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
                        GrabbableObject component2 = this.nearestDroppedItem.GetComponent<GrabbableObject>();
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
            foreach (GameObject gameObject in this.droppedItems)
            {
                float num = Vector3.Distance(base.transform.position, gameObject.transform.position);
                if (Vector3.Distance(base.transform.position, gameObject.transform.position) < float.PositiveInfinity && num < 30f)
                {
                    bool flag2 = !gameObject.GetComponent<GrabbableObject>().isHeld;
                    if (flag2)
                    {
                        this.nearestDroppedItem = gameObject;
                        this.isNearestItem = true;
                        return;
                    }
                }
            }
            this.isNearestItem = false;
        }

        public override void DetectNoise(Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot = 0, int noiseID = 0)
        {
            base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
            float num = Vector3.Distance(noisePosition, base.transform.position);
            bool flag = num > 15f;
            if (!flag)
            {
                bool flag2 = Physics.Linecast(this.eye.position, noisePosition, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
                if (flag2)
                {
                    noiseLoudness /= 2f;
                }
                bool flag3 = (double)(noiseLoudness / num) <= 0.045;
                if (!flag3)
                {
                    bool flag4 = this.timeSinceLookingAtNoise > 5f;
                    if (flag4)
                    {
                        this.timeSinceLookingAtNoise = 0f;
                        this.lookAtNoise = noisePosition;
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
                this.agent.destination = ShrimpAI.networkTargetPlayer.Value.GetPlayerController().transform.position;
                position = ShrimpAI.networkTargetPlayer.Value.GetPlayerController().transform.position;
                position.y = base.transform.position.y;
            }else
            { 
                this.agent.destination = nearestPlayer.transform.position;
                position = nearestPlayer.transform.position;
                position.y = base.transform.position.y;
            }
            Vector3 vector = position - base.transform.position;
            this.backAwayRay = new Ray(base.transform.position, vector * -1f);
            if (Physics.Raycast(this.backAwayRay, out this.hitInfo, 60f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                if (this.hitInfo.distance < 4f)
                {
                    if (Physics.Linecast(base.transform.position, this.hitInfo.point + Vector3.Cross(vector, Vector3.up) * 25.5f, out this.hitInfoB, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                    {
                        float distance = this.hitInfoB.distance;
                        if (Physics.Linecast(base.transform.position, this.hitInfo.point + Vector3.Cross(vector, Vector3.up) * -25.5f, out this.hitInfoB, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                        {
                            float distance2 = this.hitInfoB.distance;
                            if (Mathf.Abs(distance - distance2) < 5f)
                            {
                                this.agent.destination = this.hitInfo.point + Vector3.Cross(vector, Vector3.up) * -4.5f;
                            }
                            else
                            {
                                if (distance < distance2)
                                {
                                    this.agent.destination = this.hitInfo.point + Vector3.Cross(vector, Vector3.up) * -4.5f;
                                }
                                else
                                {
                                    this.agent.destination = this.hitInfo.point + Vector3.Cross(vector, Vector3.up) * 4.5f;
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.agent.destination = this.hitInfo.point;
                }
            }
            else
            {
                this.agent.destination = this.backAwayRay.GetPoint(2.3f);
            }
            this.agent.stoppingDistance = 0.2f;
            Quaternion quaternion = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(vector), 3f * Time.deltaTime);
            base.transform.eulerAngles = new Vector3(0f, quaternion.eulerAngles.y, 0f);
            this.agent.speed = 8f;
            agent.acceleration = 50000;
            this.creatureAnimator.SetFloat("walkSpeed", -2.2f);
        }

        private IEnumerator EatenFlashbang()
        {
            yield return new WaitForSeconds(2f);
            this.mainAudio.PlayOneShot(Plugin.eatenExplode);
            yield break;
        }

        private void SetupBehaviour()
        {
            this.roamingState.name = "Roaming";
            this.roamingState.boolValue = true;
            this.followingPlayer.name = "Following";
            this.followingPlayer.boolValue = true;
            this.enragedState.name = "Enraged";
            this.enragedState.boolValue = true;
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerServerRpc(int playerId)
        {
            if (!isKillingPlayer || StartOfRound.Instance.allPlayerScripts[playerId].IsOwnedByServer)
            {
                KillPlayerClientRpc(playerId);
            }
        }


        [ClientRpc]
        public void KillPlayerClientRpc(int playerId)
        {
            if (killPlayerAnimCoroutine != null)
            {
                StopCoroutine(killPlayerAnimCoroutine);
            }
            killPlayerAnimCoroutine = StartCoroutine(KillPlayer(playerId));
        }

        private IEnumerator KillPlayer(int playerId)
        {
            mainAudio.PlayOneShot(Plugin.ripPlayerApart);
            isKillingPlayer = true;
            PlayerControllerB killPlayer = StartOfRound.Instance.allPlayerScripts[playerId];
            killPlayer.KillPlayer(Vector3.zero, true, CauseOfDeath.Mauling);
            creatureAnimator.SetTrigger("RipObject");

            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => killPlayer.deadBody != null || Time.realtimeSinceStartup - startTime > 2f);
            DeadBodyInfo body = killPlayer.deadBody;
            if (body != null && body.attachedTo == null)
            {
                body.attachedLimb = body.bodyParts[5];
                body.attachedTo = bittenObjectHolder;
                body.matchPositionExactly = true;
            }
            yield return new WaitForSeconds(0.03f);
            hungerValue.Value = 0f;
            yield return new WaitForSeconds(4.4f);
            creatureAnimator.SetTrigger("eat");
            mainAudio.PlayOneShot(Plugin.dogEatPlayer);
            killPlayer.deadBody.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            isKillingPlayer = false;
        }


        private Coroutine killPlayerAnimCoroutine;

        public float networkPosDistance;

        public Vector3 prevPosition;

        public float stuckDetectionTimer;

        public float prevPositionDistance;

        public AISearchRoutine roamMap = new AISearchRoutine();

        private Vector3 spawnPosition;

        public PlayerControllerB hittedPlayer;

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
