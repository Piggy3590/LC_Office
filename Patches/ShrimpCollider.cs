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

namespace LCOffice.Patches
{
    public class ShrimpCollider : MonoBehaviour
    {
        //IHittable
        /*
        public ShrimpAI shrimpAI;
        void Start()
        {
            shrimpAI = transform.parent.GetComponent<ShrimpAI>();
        }
        bool IHittable.Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX = false)
        {
            if (ShrimpAI.hungerValue.Value < 55 && shrimpAI.scaredBackingAway <= 0)
            {
                ShrimpAI.isHitted.Value = true;
            }
            return true;
        }
        */
    }
}