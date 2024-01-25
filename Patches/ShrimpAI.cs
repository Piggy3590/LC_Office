using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using LethalLib.Modules;
using Unity.Netcode;
using DigitalRuby.ThunderAndLightning;
using LethalNetworkAPI;
using UnityEngine.InputSystem.HID;
using System.Data.SqlTypes;
using System.Xml.Linq;

namespace LCOffice.Patches
{
    public class ShrimpAI : EnemyAI
    {
        void Awake()
        {
            agent = this.GetComponent<NavMeshAgent>();
        }
        public override void Start()
        {
            this.transform.GetChild(0).GetComponent<EnemyAICollisionDetect>().mainScript = this;
            enemyType = Plugin.shrimpEnemy;
            skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            thisNetworkObject = gameObject.GetComponentInChildren<NetworkObject>();
            serverPosition = transform.position;
            thisEnemyIndex = RoundManager.Instance.numberOfEnemiesInScene;
            RoundManager.Instance.numberOfEnemiesInScene++;
            allAINodes = GameObject.FindGameObjectsWithTag("AINode");
            path1 = new NavMeshPath();
            openDoorSpeedMultiplier = enemyType.doorSpeedMultiplier;
            if (base.IsOwner)
            {
                this.SyncPositionToClients();
            }
            else
            {
                this.SetClientCalculatingAI(false);
            }

            this.transform.GetChild(0).gameObject.AddComponent<ShrimpCollider>();

            mouth = GameObject.Find("ShrimpMouth").transform;
            leftEye = GameObject.Find("ShrimpLeftEye").transform;
            rightEye = GameObject.Find("ShrimpRightEye").transform;

            creatureAnimator = this.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Animator>();

            creatureAnimator.SetTrigger("Walk");
            
            mainAudio = GameObject.Find("ShrimpMainAudio").GetComponent<AudioSource>();
            voiceAudio = GameObject.Find("ShrimpGrowlAudio").GetComponent<AudioSource>();
            voice2Audio = GameObject.Find("ShrimpAngerAudio").GetComponent<AudioSource>();
            //dogMusic = GameObject.Find("ShrimpAngerAudio").GetComponent<AudioSource>();
            lookRig = GameObject.Find("ShrimpLookAtPlayer").GetComponent<Rig>();

            AudioSource[] audios = this.transform.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                audio.outputAudioMixerGroup = GameObject.Find("StatusEffectAudio").GetComponent<AudioSource>().outputAudioMixerGroup;
            }
            lookTarget = GameObject.Find("Shrimp_Look_target").transform;
            dogHead = GameObject.Find("ShrimpLookPoint").transform;
            bittenObjectHolder = GameObject.Find("BittenObjectHolder").transform;

            shrimpEye = GameObject.Find("ShrimpEye").transform;
            scaleOfEyesNormally = leftEye.localScale;
            originalMouthScale = mouth.localScale;

            enragedScream = Plugin.enragedScream;
            dogSprint = Plugin.dogSprint;
            bigGrowl = Plugin.bigGrowl;

            voice2Audio.clip = dogSprint;
            voice2Audio.Play();

            creatureVoice = voice2Audio;
            creatureSFX = voice2Audio;
            eye = shrimpEye;

            SetupBehaviour();
            tempEnemyBehaviourStates.Add(roamingState);
            tempEnemyBehaviourStates.Add(followingPlayer);
            tempEnemyBehaviourStates.Add(enragedState);
            enemyBehaviourStates = tempEnemyBehaviourStates.ToArray();

            spawnPosition = this.transform.position;
            roamMap = new AISearchRoutine();

            //RoundManager.Instance.SpawnedEnemies.Add(this);

            if (Plugin.setKorean)
            {
                this.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "쉬림프";
            }

            if (isHitted.Value)
            {
                StartCoroutine(stunnedTimer(targetPlayer));
            }
        }

        public override void HitEnemy(int force, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            if (hungerValue < 60 && scaredBackingAway <= 0)
            {
                isHitted.Value = true;
            }
        }

        public IEnumerator stunnedTimer(PlayerControllerB playerWhoHit)
        {
            isHitted.Value = false;
            hittedPlayer = playerWhoHit;
            agent.speed = 0f;
            creatureAnimator.SetTrigger("Recoil");
            mainAudio.PlayOneShot(Plugin.cry1, 1f);
            yield return new WaitForSeconds(0.5f);
            scaredBackingAway = 3f;
            yield return new WaitForSeconds(2f);
            hittedPlayer = null;
            yield break;
        }

        void StunTest()
        {
            if (scaredBackingAway > 0)
            {
                agent.SetDestination(ChooseFarthestNodeFromPosition(this.transform.position, true, 0, false).position);
                creatureAnimator.SetFloat("walkSpeed", -3.5f);
                scaredBackingAway -= Time.deltaTime;
            }
        }

        public void FootStepSound()
        {
            this.randomVal = (float)UnityEngine.Random.Range(0, 20);
            if (this.randomVal < 5f)
            {
                this.mainAudio.PlayOneShot(Plugin.footstep1, UnityEngine.Random.Range(0.8f, 1f));
                return;
            }
            if (this.randomVal < 10f)
            {
                this.mainAudio.PlayOneShot(Plugin.footstep2, UnityEngine.Random.Range(0.8f, 1f));
                return;
            }
            if (this.randomVal < 15f)
            {
                this.mainAudio.PlayOneShot(Plugin.footstep3, UnityEngine.Random.Range(0.8f, 1f));
                return;
            }
            if (this.randomVal < 20f)
            {
                this.mainAudio.PlayOneShot(Plugin.footstep4, UnityEngine.Random.Range(0.8f, 1f));
            }
        }

        public override void Update()
        {
            if (!agent.enabled)
            {
                Plugin.mls.LogInfo("dog: NavMesh Agent is disabled, enabling");
                agent.enabled = true;
            }
            CheckPlayer();
            SetByHunger();
            EatItem();
            CheckTargetAvailable();

            //agent.velocity.sqrMagnitude -> default walk speed = 30.0
            if (networkTargetPlayer.Value == null && !isNearestItem && targetNode == null)
            {
                int nodeTargetCount = UnityEngine.Random.Range(1, allAINodes.Length);
                if (Vector3.Distance(this.transform.position, allAINodes[nodeTargetCount].transform.position) > 5f && SelectNode.Value != nodeTargetCount && GameNetworkManager.Instance.isHostingGame)
                {
                    SelectNode.Value = nodeTargetCount;
                    targetNode = allAINodes[nodeTargetCount];
                }
                //dogRandomWalk = true;
            }
            if (stuckDetectionTimer > 5f)
            {
                int nodeTargetCount = UnityEngine.Random.Range(1, allAINodes.Length);
                if (Vector3.Distance(this.transform.position, allAINodes[nodeTargetCount].transform.position) > 5f && SelectNode.Value != nodeTargetCount && GameNetworkManager.Instance.isHostingGame)
                {
                    SelectNode.Value = nodeTargetCount;
                    targetNode = allAINodes[SelectNode.Value];
                    stuckDetectionTimer = 0;
                }else if (Vector3.Distance(this.transform.position, allAINodes[nodeTargetCount].transform.position) > 5f)
                {
                    stuckDetectionTimer = 0;
                }
            }
            if (networkTargetPlayer.Value == null && !isNearestItem)
            {
                if (targetNode != null)
                {
                    agent.SetDestination(targetNode.transform.position);
                    if (agent.velocity.sqrMagnitude < 0.5f)
                    {
                        stuckDetectionTimer += Time.deltaTime;
                    }else
                    {
                        stuckDetectionTimer = 0f;
                    }
                    float nodeDistance = Vector3.Distance(this.transform.position, targetNode.transform.position);
                    if (nodeDistance < 3f)
                    {
                        int nodeTargetCount = UnityEngine.Random.Range(1, allAINodes.Length);
                        if (Vector3.Distance(this.transform.position, allAINodes[nodeTargetCount].transform.position) > 5f && SelectNode.Value != nodeTargetCount && GameNetworkManager.Instance.isHostingGame)
                        {
                            SelectNode.Value = nodeTargetCount;
                            targetNode = allAINodes[SelectNode.Value];
                        }
                    }
                }
            }
            
            targetNode = allAINodes[SelectNode.Value];

            if (networkTargetPlayer.Value != null || isNearestItem)
            {
                //dogRandomWalk = false;
                stuckDetectionTimer = 0;
            }

            if (GameNetworkManager.Instance.isHostingGame)
            {
                networkPosition.Value = this.transform.position;
                networkRotation.Value = this.transform.rotation.eulerAngles;
            }else
            {
                float distance = Vector3.Distance(networkPosition.Value, this.transform.position);
                if (distance > 2)
                {
                    StartCoroutine(SyncPosition());
                }
                if (CalculateRotationDifference(networkRotation.Value, this.transform.rotation.eulerAngles) > 15)
                {
                    StartCoroutine(SyncRotation());
                    Plugin.mls.LogInfo("the difference in rotation of the shrimp is too large! Syncing... : " + CalculateRotationDifference(networkRotation.Value, this.transform.rotation.eulerAngles));
                }
            }

            if (networkTargetPlayer.Value != null)
            {
                isTargetAvailable = true;
            }

            if (networkTargetPlayer.Value != null)
            {
                if (hungerValue < 55)
                {
                    leftEye.localScale = Vector3.Lerp(leftEye.localScale, scaleOfEyesNormally, 20f * Time.deltaTime);
                    rightEye.localScale = Vector3.Lerp(rightEye.localScale, scaleOfEyesNormally, 20f * Time.deltaTime);
                }
                else if (hungerValue > 55)
                {
                    leftEye.localScale = Vector3.Lerp(leftEye.localScale, scaleOfEyesNormally * 0.4f, 20f * Time.deltaTime);
                    rightEye.localScale = Vector3.Lerp(rightEye.localScale, scaleOfEyesNormally * 0.4f, 20f * Time.deltaTime);
                }
            }else
            {
                leftEye.localScale = Vector3.Lerp(leftEye.localScale, scaleOfEyesNormally, 20f * Time.deltaTime);
                rightEye.localScale = Vector3.Lerp(rightEye.localScale, scaleOfEyesNormally, 20f * Time.deltaTime);
            }

            creatureAnimator.SetBool("DogRandomWalk", dogRandomWalk);

            if (canBeMoved && !isRunning)
            {
                creatureAnimator.SetBool("Running", false);
                agent.speed = 5.5f;
                agent.acceleration = 7;
                agent.angularSpeed = 150;
            }else if (!canBeMoved && !isRunning)
            {
                creatureAnimator.SetBool("Running", false);
                agent.speed = 0;
                agent.angularSpeed = 0;
            }
            if (isRunning)
            {
                creatureAnimator.SetBool("Running", true);
                agent.speed = Mathf.Lerp(agent.speed, 15f, Time.deltaTime * 2.5f);
                agent.angularSpeed = 10000;
                agent.acceleration = 15;
            }

            StunTest();
        }

        IEnumerator SyncPosition()
        {
            this.transform.position = Vector3.Lerp(this.transform.position, networkPosition.Value, Time.deltaTime * 10);
            yield return new WaitForSeconds(1f);
            yield break;
        }

        IEnumerator SyncRotation()
        {
            this.transform.rotation = Quaternion.Euler(Vector3.Lerp(this.transform.rotation.eulerAngles, networkRotation.Value, Time.deltaTime * 10));
            yield return new WaitForSeconds(1f);
            yield break;
        }

        float CalculateRotationDifference(Vector3 rotation1, Vector3 rotation2)
        {
            Quaternion quaternion1 = Quaternion.Euler(rotation1);
            Quaternion quaternion2 = Quaternion.Euler(rotation2);

            float angleDifference = Quaternion.Angle(quaternion1, quaternion2);

            return angleDifference;
        }

        void CheckPlayer()
        {
            PlayerControllerB tempPlayer = CheckLineOfSightForPlayer(60f, 60, -1);
            if (tempPlayer != null && networkTargetPlayer.Value == null && !isKillingPlayer)
            {
                if (!isSeenPlayer)
                {
                    mainAudio.PlayOneShot(Plugin.dogHowl);
                    isSeenPlayer = true;
                }
                networkTargetPlayer.Value = tempPlayer;
            }
            //else if (!isTargetAvailable && !roamMap.inProgress)
            //{
            //    StartSearch(spawnPosition, roamMap);
            //}

            if (droppedItems.Count > 0 && nearestDroppedItem == null)
            {
                FindNearestItem();
            }
            footStepTime += Time.deltaTime * agent.velocity.sqrMagnitude / 8f;
            if (footStepTime > 0.5)
            {
                FootStepSound();
                footStepTime = 0;
            }
            creatureAnimator.SetFloat("walkSpeed", Mathf.Clamp(agent.velocity.sqrMagnitude / 5f, 0f, 3f));
            creatureAnimator.SetFloat("runSpeed", Mathf.Clamp(agent.velocity.sqrMagnitude / 2.7f, 3f, 4f));

            if (isTargetAvailable && hungerValue < 65 && !isRunning)
            {
                if (!isNearestItem)
                {
                    if (hungerValue > 55)
                    {
                        lookRay = new Ray(dogHead.position, networkTargetPlayer.Value.lowerSpine.transform.position - dogHead.position);
                        lookTarget.position = Vector3.Lerp(lookTarget.position, lookRay.GetPoint(3f), 30f * Time.deltaTime);
                    }
                    else
                    {
                        lookTarget.position = Vector3.Lerp(lookTarget.position, networkTargetPlayer.Value.lowerSpine.transform.position, 6f * Time.deltaTime);
                    }
                }
                agent.autoBraking = true;
                lookRig.weight = 0.5f;
                dogRandomWalk = false;
                if (!isSeenPlayer)
                {
                    mainAudio.PlayOneShot(Plugin.dogHowl, 1f);
                }
                isSeenPlayer = true;
            }
            if (!isRunning && !isNearestItem)
            {
                agent.stoppingDistance = 4.5f;
            }
            else
            {
                agent.stoppingDistance = 2f;
            }
        }

        void SetByHunger()
        {
            if (hungerValue < 65)
            {
                if (isSeenPlayer && !isTargetAvailable)
                {
                    hungerValue += Time.deltaTime * 0.6f;
                }
                else if (isSeenPlayer && isTargetAvailable)
                {
                    hungerValue += Time.deltaTime;
                }
            }

            if (hungerValue < 63 && hungerValue > 60 && !isEnraging)
            {
                voiceAudio.loop = true;
                voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 0.8f, 45f * Time.deltaTime);
                voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 1f, 125f * Time.deltaTime);
                isEnraging = true;
                voiceAudio.clip = bigGrowl;
                voiceAudio.Play();
            }
            else if (hungerValue < 63 && hungerValue > 60 && isEnraging)
            {
                voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 1f, 45f * Time.deltaTime);
                voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 1f, 125f * Time.deltaTime);
                if (!isNearestItem)
                {
                    canBeMoved = false;
                }
            }
            else if (hungerValue < 60)
            {
                voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 0f, 45f * Time.deltaTime);
                voiceAudio.volume = Mathf.Lerp(voiceAudio.volume, 0f, 125f * Time.deltaTime);
            }
            else if (hungerValue > 63 && !isAngered)
            {
                if (!isNearestItem && !isKillingPlayer && stunNormalizedTimer <= 0f)
                {
                    canBeMoved = true;
                }
                voiceAudio.clip = enragedScream;
                voiceAudio.Play();
                isEnraging = false;
                isAngered = true;
            }else if (hungerValue > 66)
            {
                isRunning = true;
                voice2Audio.volume = Mathf.Lerp(voice2Audio.volume, 1f, 125f * Time.deltaTime);
            }
            else if (hungerValue > 63)
            {
                voiceAudio.pitch = Mathf.Lerp(voiceAudio.pitch, 0.8f, 45f * Time.deltaTime);
                mouth.localScale = Vector3.Lerp(mouth.localScale, new Vector3(0.005590725f, 0.01034348f, 0.02495567f), 30f * Time.deltaTime);
            }
            if (hungerValue < 60)
            {
                isEnraging = false;
                isAngered = false;
                if (!isNearestItem && !isKillingPlayer && stunNormalizedTimer <= 0f)
                {
                    canBeMoved = true;
                }
            }
            if (hungerValue < 66)
            {
                isRunning = false;
            }

            if (hungerValue < 63)
            {
                mouth.localScale = Vector3.Lerp(mouth.localScale, originalMouthScale, 20f * Time.deltaTime);
                voice2Audio.volume = Mathf.Lerp(voice2Audio.volume, 0f, 125f * Time.deltaTime);
                isRunning = false;
            }
            if (hungerValue > 64 && networkTargetPlayerDistance < 3.5f && !isKillingPlayer && networkTargetPlayer.Value != null)
            {
                if (!networkTargetPlayer.Value.isCameraDisabled)
                {
                    KillingPlayerBool.Value = true;
                }
            }
            if (KillingPlayerBool.Value)
            {
                if (!networkTargetPlayer.Value.isCameraDisabled)
                {
                    Plugin.mls.LogInfo("Shrimp: (Owner) Killing " + networkTargetPlayer.Value.name + "!");
                    StartCoroutine(KillPlayer(networkTargetPlayer.Value));
                    KillingPlayerBool.Value = false;
                }else if (networkTargetPlayer.Value.isCameraDisabled)
                {
                    Plugin.mls.LogInfo("Shrimp: (Not Owner) Killing " + networkTargetPlayer.Value.name + "!");
                    StartCoroutine(KillPlayerInOtherClient(networkTargetPlayer.Value));
                    KillingPlayerBool.Value = false;
                }
            }
            if (isKillingPlayer)
            {
                canBeMoved = false;
            }
        }

        void EatItem()
        {
            if (nearestDroppedItem != null)
            {
                if (isNearestItem && !nearestDroppedItem.GetComponent<GrabbableObject>().isHeld)
                {
                    if (hungerValue < 55)
                    {
                        lookRay = new Ray(dogHead.position, nearestDroppedItem.transform.position - dogHead.position);
                        lookTarget.position = Vector3.Lerp(lookTarget.position, lookRay.GetPoint(1.8f), 6f * Time.deltaTime);
                    }
                    else
                    {
                        lookTarget.position = Vector3.Lerp(this.lookTarget.position, nearestDroppedItem.transform.position, 6f * Time.deltaTime);
                    }
                    agent.SetDestination(nearestDroppedItem.transform.position);
                    nearestItemDistance = Vector3.Distance(this.transform.position, nearestDroppedItem.transform.position);
                    if (nearestItemDistance < 1.35f)
                    {
                        canBeMoved = false;
                        isRunning = false;
                        creatureAnimator.SetTrigger("eat");
                        mainAudio.PlayOneShot(Plugin.dogEatItem);
                        isNearestItem = false;
                        nearestItemDistance = 500;
                        if (nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight > 1)
                        {
                            hungerValue -= Mathf.Clamp(nearestDroppedItem.GetComponent<GrabbableObject>().itemProperties.weight - 1f, 0f, 100f) * 105f;
                        }
                        else
                        {
                            hungerValue -= 5;
                        }
                        GrabbableObject item = nearestDroppedItem.GetComponent<GrabbableObject>();
                        item.grabbable = false;
                        item.grabbableToEnemies = false;
                        item.deactivated = true;
                        if (item.radarIcon != null)
                        {
                            GameObject.Destroy(item.radarIcon.gameObject);
                        }
                        MeshRenderer[] componentsInChildren = item.gameObject.GetComponentsInChildren<MeshRenderer>();
                        for (int i = 0; i < componentsInChildren.Length; i++)
                        {
                            GameObject.Destroy(componentsInChildren[i]);
                        }
                        Collider[] componentsInChildren2 = item.gameObject.GetComponentsInChildren<Collider>();
                        for (int j = 0; j < componentsInChildren2.Length; j++)
                        {
                            GameObject.Destroy(componentsInChildren2[j]);
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
            }
            if (stunNormalizedTimer > 0f)
            {
                canBeMoved = false;
                creatureAnimator.SetBool("Stun", true);
            }else
            {
                creatureAnimator.SetBool("Stun", false);
            }
        }

        void CheckTargetAvailable()
        {
            if (isTargetAvailable && !isNearestItem && !isKillingPlayer && scaredBackingAway <= 0 && stunNormalizedTimer <= 0f)
            {
                followTimer = 10;
                canBeMoved = true;
            }

            if (isTargetAvailable && networkTargetPlayer.Value != null)
            {
                networkTargetPlayerDistance = Vector3.Distance(this.transform.position, networkTargetPlayer.Value.transform.position);
            }
            else
            {
                networkTargetPlayerDistance = 3000;
            }

            if (followTimer > 0 || networkTargetPlayerDistance < 8f)
            {
                if (isTargetAvailable && scaredBackingAway <= 0 && !isNearestItem)
                {
                    agent.SetDestination(networkTargetPlayer.Value.transform.position);
                }
                if (nearestDroppedItem != null && nearestDroppedItem.GetComponent<GrabbableObject>().isHeld)
                {
                    droppedItems.Remove(nearestDroppedItem);
                    nearestDroppedItem = null;
                    agent.SetDestination(networkTargetPlayer.Value.transform.position);
                }
            }
            if (networkTargetPlayer.Value != null && followTimer > 0 && networkTargetPlayerDistance > 8f)
            {
                followTimer -= Time.deltaTime;
            }
            else if (followTimer <= 0)
            {
                isTargetAvailable = false;
                networkTargetPlayer.Value = null;
            }
        }

        private bool IsPlayerVisible(PlayerControllerB player)
        {
            // 플레이어와 적 사이의 방향 벡터
            Vector3 directionToPlayer = player.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // fieldOfView -> 70
            if (angleToPlayer < 70 * 0.5f)
            {
                if (Physics.Raycast(transform.position, directionToPlayer, 120, LayerMask.NameToLayer("Player")))
                {
                    // 플레이어가 적의 시야에 들어왔음
                    return true;
                }
            }

            return false;
        }

        void FindNearestItem()
        {
            foreach (GameObject droppedItem in droppedItems)
            {
                float distance = Vector3.Distance(this.transform.position, droppedItem.transform.position);
                if (distance < Mathf.Infinity && distance < 30f)
                {
                    nearestDroppedItem = droppedItem;
                    isNearestItem = true;
                    return;
                }
            }
            isNearestItem = false;
        }

        IEnumerator KillPlayer(PlayerControllerB killPlayer)
        {
            hungerValue = 0;
            Plugin.mls.LogInfo("Shrimp: Killing " + killPlayer.name + "!");
            yield return new WaitForSeconds(0.05f);
            isKillingPlayer = true;
            creatureAnimator.SetTrigger("RipObject");
            mainAudio.PlayOneShot(Plugin.ripPlayerApart);
            if (GameNetworkManager.Instance.localPlayerController == killPlayer)
            {
                killPlayer.KillPlayer(Vector3.zero, true, CauseOfDeath.Mauling, 0);
            }
            yield return new WaitForSeconds(0.5f);
            if (killPlayer.deadBody == null)
            {
                Debug.Log("Shrimp: Player body was not spawned or found within 0.5 seconds.");
                killPlayer.inAnimationWithEnemy = null;
                isKillingPlayer = false;
                KillingPlayerBool.Value = false;
                yield break;
            }
            TakeBodyInMouth(killPlayer.deadBody);
            yield return new WaitForSeconds(4f);
            creatureAnimator.SetTrigger("eat");
            mainAudio.PlayOneShot(Plugin.dogEatItem);
            killPlayer.deadBody.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            isKillingPlayer = false;
            yield break;
        }

        IEnumerator KillPlayerInOtherClient(PlayerControllerB killPlayer)
        {
            Plugin.mls.LogInfo("Shrimp: Killing " + killPlayer.name + "!");
            hungerValue = 0;
            yield return new WaitForSeconds(0.05f);
            isKillingPlayer = true;
            creatureAnimator.SetTrigger("RipObject");
            mainAudio.PlayOneShot(Plugin.ripPlayerApart);
            yield return new WaitForSeconds(0.5f);
            if (killPlayer.deadBody == null)
            {
                Debug.Log("Shrimp: Player body was not spawned or found within 0.5 seconds.");
                killPlayer.inAnimationWithEnemy = null;
                isKillingPlayer = false;
                KillingPlayerBool.Value = false;
                yield break;
            }
            TakeBodyInMouth(killPlayer.deadBody);
            yield return new WaitForSeconds(4f);
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

        public PlayerControllerB CheckSightForPlayer(float width = 45f, int range = 60, int proximityAwareness = -1)
        {
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                Vector3 position = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                if (Vector3.Distance(position, eye.position) < (float)range && !Physics.Linecast(eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                    Vector3 vector = position - eye.position;
                    if (Vector3.Angle(eye.forward, vector) < width || (proximityAwareness != -1 && Vector3.Distance(eye.position, position) < (float)proximityAwareness))
                    {
                        return StartOfRound.Instance.allPlayerScripts[i];
                    }
                }
            }
            return null;
        }
        void SetupBehaviour()
        {
            roamingState.name = "Roaming";
            roamingState.boolValue = true;

            followingPlayer.name = "Following";
            followingPlayer.boolValue = true;

            enragedState.name = "Enraged";
            enragedState.boolValue = true;
        }

        public new GameObject targetNode;
        public Vector3 prevPosition;
        public float stuckDetectionTimer;
        public float prevPositionDistance;

        public AISearchRoutine roamMap = new AISearchRoutine();
        private Vector3 spawnPosition;

        public PlayerControllerB hittedPlayer;

        public static LethalNetworkVariable<bool> KillingPlayerBool = new LethalNetworkVariable<bool>(identifier: "KillingPlayerBool");
        public static LethalNetworkVariable<int> SelectNode = new LethalNetworkVariable<int>(identifier: "SelectNode");
        //public LethalClientMessage<bool> KillingPlayer = new("KillingPlayer", onReceived: KillPlayerSync);

        public static LethalNetworkVariable<bool> isHitted = new LethalNetworkVariable<bool>(identifier: "isHitted");
        public static LethalNetworkVariable<Vector3> networkPosition = new LethalNetworkVariable<Vector3>(identifier: "networkPosition");
        public static LethalNetworkVariable<Vector3> networkRotation = new LethalNetworkVariable<Vector3>(identifier: "networkRotation");
        public static LethalNetworkVariable<PlayerControllerB> networkTargetPlayer = new LethalNetworkVariable<PlayerControllerB>(identifier: "networkTargetPlayer");
        public bool isKillingPlayer;

        public AudioClip bigGrowl;
        public AudioClip enragedScream;
        public AudioClip dogSprint;

        public bool isSeenPlayer;
        public bool isEnraging;
        public bool isAngered;
        public float hungerValue;

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

        public Rig lookRig;

        //public Transform dogBody;

        //public Transform dogHead;

        //public Transform lookForwardPoint;

        public Transform leftEye;
        public Transform rightEye;
        public Transform shrimpEye;
        public Transform mouth;

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
    }
}