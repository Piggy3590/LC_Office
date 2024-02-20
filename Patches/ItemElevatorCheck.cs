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
    public class ItemElevatorCheck : MonoBehaviour
    {
        public bool isInElevatorB;
        public bool isAppendedToArray;
        public ElevatorCollider elevatorCollider;
        public bool droppedItemMoment;
        public float dogEatTimer;
        public bool ignoreCollider;

        public BoxCollider boxCollider;

        public GrabbableObject grabbableObject;

        public ShrimpAI shrimpAI;

        public Vector3 orgScale;

        public GameObject storageObject;

        void Start()
        {
            if (this.GetComponent<PlayerControllerB>() != null)
            {
                Destroy(this);
            }
            grabbableObject = this.GetComponent<GrabbableObject>();
            if (GameObject.Find("TheDog") != null)
            {
                shrimpAI = GameObject.FindObjectOfType<ShrimpAI>();
            }
            orgScale = this.transform.localScale;
            boxCollider = this.GetComponent<BoxCollider>();
        }

        void LateUpdate()
        {
            if (shrimpAI != null)
            {
                if (grabbableObject.isHeld)
                {
                    dogEatTimer = 0;
                    droppedItemMoment = true;
                }
                if (!grabbableObject.isHeld && droppedItemMoment)
                {
                    if (dogEatTimer == 0)
                    {
                        shrimpAI.droppedItems.Add(this.gameObject);
                    }
                    dogEatTimer += Time.deltaTime;
                }
                if (dogEatTimer > 1.5f)
                {
                    shrimpAI.droppedItems.Remove(this.gameObject);
                    droppedItemMoment = false;
                    dogEatTimer = 0;
                }
            }
            if (OfficeRoundSystem.Instance == null) { return; }
            if (!isAppendedToArray && !StartOfRound.Instance.inShipPhase)
            {
                if (OfficeRoundSystem.Instance.isOffice)
                {
                    if (elevatorCollider == null && GameObject.FindObjectOfType<ElevatorCollider>() != null)
                    {
                        elevatorCollider = GameObject.FindObjectOfType<ElevatorCollider>();
                    }
                    if (!ignoreCollider && !elevatorCollider.allColliders.Contains(boxCollider))
                    {
                        elevatorCollider.allColliders.Add(boxCollider);
                    }
                    if (storageObject == null)
                    {
                        storageObject = GameObject.Find("ElevatorStorage(Clone)");
                    }else
                    {
                        if (this.transform.parent == storageObject && grabbableObject.isHeld)
                        {
                            this.transform.SetParent(null);
                        }
                    }
                    if (ignoreCollider && elevatorCollider.allColliders.Contains(boxCollider))
                    {
                        elevatorCollider.allColliders.Remove(boxCollider);
                    }
                }
                isAppendedToArray = true;
            }

            if (this.transform.localScale != orgScale)
            {
                this.transform.localScale = orgScale;
            }

            if (OfficeRoundSystem.Instance.isOffice && isInElevatorB && grabbableObject.isHeld)
            {
                isInElevatorB = false;
            }
        }

        void OnDestroy()
        {
            if (elevatorCollider != null)
            {
                elevatorCollider.allColliders.Remove(boxCollider);
            }
        }
    }
}