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

        public GrabbableObject grabbableObject;

        public ShrimpAI shrimpAI;

        void Start()
        {
            grabbableObject = this.GetComponent<GrabbableObject>();
        }

        void LateUpdate()
        {
            if (GameObject.Find("TheDog") != null && shrimpAI == null)
            {
                shrimpAI = GameObject.FindObjectOfType<ShrimpAI>();
            }
            if (!isAppendedToArray && !StartOfRound.Instance.inShipPhase)
            {
                if (RoundMapSystem.Instance.isOffice)
                {
                    elevatorCollider = GameObject.FindObjectOfType<ElevatorCollider>();
                    if (GameObject.FindObjectOfType<ElevatorCollider>() != null)
                    {
                        elevatorCollider.allPlayerColliders.Add(this.gameObject);
                    }
                }
                isAppendedToArray = true;
            }

            if (RoundMapSystem.Instance.isOffice && isInElevatorB && grabbableObject.isHeld)
            {
                isInElevatorB = false;
            }

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
                    if (dogEatTimer > 2f)
                    {
                        shrimpAI.droppedItems.Remove(this.gameObject);
                        droppedItemMoment = false;
                        dogEatTimer += 0;
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (elevatorCollider != null)
            {
                elevatorCollider.allPlayerColliders.Remove(this.gameObject);
            }
        }
    }
}