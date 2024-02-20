using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using LethalNetworkAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace LCOffice.Patches
{
    public class AnotherInterior : MonoBehaviour
    {
        public RuntimeDungeon dungeonGenerator;
        void Awake()
        {
            dungeonGenerator = this.GetComponent<RuntimeDungeon>();
            dungeonGenerator.Generator.currentArchetype = Plugin.officeArchetype_A;
            dungeonGenerator.Generator.DungeonFlow = Plugin.officeDungeonFlow_A;
        }

        void Start()
        {
            RoundManager roundManager = RoundManager.Instance;

            Vector3 levelGenRoot = GameObject.Find("LevelGenerationRoot").transform.position;
            GameObject levelGenRootObject = GameObject.Instantiate(new GameObject(), new Vector3(levelGenRoot.x - 130, levelGenRoot.y, levelGenRoot.z - 130), Quaternion.Euler(0, 0, 0));
            dungeonGenerator.Root = levelGenRootObject;

            dungeonGenerator.Generator.ShouldRandomizeSeed = false;
            dungeonGenerator.Generator.Seed = roundManager.LevelRandom.Next();
            Debug.Log(string.Format("GenerateNewFloor(). Map generator's random seed: {0}", dungeonGenerator.Generator.Seed));
            float num = roundManager.currentLevel.factorySizeMultiplier * roundManager.mapSizeMultiplier;
            dungeonGenerator.Generator.LengthMultiplier = num;
            dungeonGenerator.Generator.currentArchetype = Plugin.officeArchetype_A;
            dungeonGenerator.Generator.DungeonFlow = Plugin.officeDungeonFlow_A;

            if (dungeonGenerator.Generator.currentArchetype == Plugin.officeArchetype_A && dungeonGenerator.Generator.DungeonFlow == Plugin.officeDungeonFlow_A)
            {
                dungeonGenerator.Generate();
            }
        }

        void FixedUpdate()
        {
            dungeonGenerator.Generator.currentArchetype = Plugin.officeArchetype_A;
            dungeonGenerator.Generator.DungeonFlow = Plugin.officeDungeonFlow_A;
        }
    }
}
