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
            boxCollider = this.GetComponent<BoxCollider>();
            if (elevatorCollider == null)
            {
                elevatorCollider = GameObject.FindObjectOfType<ElevatorCollider>();
            }
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
                    droppedItemMoment = false;
                }
                if (!grabbableObject.isHeld && dogEatTimer <= 1.5f)
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
                    dogEatTimer = 0;
                }
            }
            if (isAppendedToArray && !TimeOfDay.Instance.currentDayTimeStarted)
            {
                isAppendedToArray = false;
            }
            if (elevatorCollider != null)
            {
                if (!isAppendedToArray && TimeOfDay.Instance.currentDayTimeStarted)
                {
                    if (OfficeRoundSystem.Instance.isOffice)
                    {
                        elevatorCollider.allColliders.Add(boxCollider);
                        /*
                        if (!ignoreCollider && !elevatorCollider.allColliders.Contains(boxCollider))
                        {
                            elevatorCollider.allColliders.Add(boxCollider);
                        }
                        if (ignoreCollider && elevatorCollider.allColliders.Contains(boxCollider))
                        {
                            elevatorCollider.allColliders.Remove(boxCollider);
                        }
                        */
                    }
                    isAppendedToArray = true;
                }
            }

            if (OfficeRoundSystem.Instance == null) { return; }
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