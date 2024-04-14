using GameNetcodeStuff;
using LethalNetworkAPI;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace LCOffice.Patches
{
    public class SetPosOfNetworkObject : MonoBehaviour
    {
        public Transform originPos;
        void Start()
        {

            //originPos = this.transform.position;
        }

        void LateUpdate()
        {

        }
    }
}
