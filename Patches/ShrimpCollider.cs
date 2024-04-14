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
        public ShrimpAI shrimpAI;

        void OnTriggerEnter(Collider collision)
        {
            shrimpAI.CollideWithObject(collision.gameObject);
        }
    }
}