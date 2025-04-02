using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalLevelLoader;
using LCOffice.Patches;
using LCOffice.Components;
using JLL.ScriptableObjects;
using DunGenPlus;
using DunGen;

namespace LCOffice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("JacobG5.JLL", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.LCOffice";
        private const string modName = "LCOffice";
        private const string modVersion = "2.2.0";

        private readonly Harmony harmony = new(modGUID);

        private static Plugin Instance;

        public static ManualLogSource mls;
        public static AssetBundle Bundle;

        public static ExtendedMod officeMod;
        public static ExtendedDungeonFlow officeDungeon;
        public static DunGenExtender officeExtender;

        private ConfigEntry<bool> configEnableScraps;

        public static ConfigEntry<PathCount> mainPaths;
        public enum PathCount
        {
            One = 1,
            Two = 2,
            Three = 3
        }
        public static ConfigEntry<ScalePreset> generationScale;
        public enum ScalePreset
        {
            Normal = 1,
            Large = 2
        }

        public static ConfigEntry<float> musicVolume;
        public static ConfigEntry<bool> elevatorMusicPitchdown;

        public static ConfigEntry<bool> cameraDisable;
        public static ConfigEntry<bool> controlRoomCameraDisable;
        public static ConfigEntry<int> cameraFrameSpeed;

        public static TerminalKeyword elevatorKeyword;
        public static TerminalKeyword elevator1Keyword;
        public static TerminalKeyword elevator2Keyword;
        public static TerminalKeyword elevator3Keyword;

        public static TerminalNode elevator1Node;
        public static TerminalNode elevator2Node;
        public static TerminalNode elevator3Node;

        public static GameObject ActualElevator;
        public static GameObject StoragePrefab;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("LC_Office is loaded!");

            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Bundle = AssetBundle.LoadFromFile(Path.Combine(directoryName, "lcoffice"));
            if (Bundle == null)
            {
                mls.LogError("Failed to load Office Dungeon assets.");
            }
            else
            {
                configEnableScraps = Config.Bind("General", "OfficeCustomScrap", true, new ConfigDescription("When enabled, enables custom scrap spawning.", null, Array.Empty<object>()));

                mainPaths = Config.Bind("General", "MainDungeonPaths", PathCount.Two, "The number of main paths the interior generates. (Main paths will be longer than other paths. Increasing this number increases the size of the dungeon but makes the distribution between elevator floors larger) (When set to \"One\" switches size and branch count numbers back similar to the 2.0.0 version.)");
                generationScale = Config.Bind("General", "GenerationScale", ScalePreset.Normal, "A lot of people were telling me they don't like the overly large interior even though I personally prefer it. Because of this I've introduced this config where the default is now a much smaller generation but you can choose to play with the large scale if you prefer a challenge. The smaller default preset will lead more often to floors generating without any branch paths and dead ends.");

                musicVolume = Config.Bind("General", "ElevatorMusicVolume", 100f, "Set the volume of music played in the elevator. (0 - 100) (Client Sided)");
                elevatorMusicPitchdown = Config.Bind("General", "ElevatorMusicPitchDown", false, "Change the pitch of the elevator music. (bc i like it) (Client Sided)");

                cameraDisable = Config.Bind("General", "Disable Camera", false, "Disable cameras inside the office. (Client Sided)");
                controlRoomCameraDisable = Config.Bind("General", "Disable Control Room Camera", false, "Disable camera inside the apparatus room. (Client Sided)");
                cameraFrameSpeed = Config.Bind("General", "Camera Frame Speed", 10, "Specifies the camera speed inside the office. When set to a negative value uses the game's frame rate. (FPS) (Client Sided)");

                officeMod = ExtendedMod.Create("LC Office", "Piggy");

                officeDungeon = Bundle.LoadAsset<ExtendedDungeonFlow>("Assets/LethalCompany/Mods/LCOffice/ExtendedOfficeFlow.asset");
                officeDungeon.DungeonEvents.onApparatusTaken.AddListener((lung) =>
                {
                    ElevatorSystem.System?.OnRemoveApparatus();
                    FindObjectOfType<WeirdScreen>()?.DisableScreen();
                });
                officeDungeon.DungeonEvents.onShipLeave.AddListener(() =>
                {
                    if (RoundManager.Instance.IsHost || RoundManager.Instance.IsServer)
                    {
                        ElevatorSystem.Despawn();
                    }
                });

                int mainpaths = (int)mainPaths.Value;
                switch (mainpaths)
                {
                    case 1:
                        switch (generationScale.Value)
                        {
                            case ScalePreset.Large:
                                officeDungeon.DungeonFlow.Length = new IntRange(8, 10);
                                officeDungeon.DungeonFlow.BranchCount = new IntRange(4, 7);
                                break;
                            default:
                                officeDungeon.DungeonFlow.Length = new IntRange(5, 8);
                                officeDungeon.DungeonFlow.BranchCount = new IntRange(3, 6);
                                break;
                        }
                        break;
                    default:
                        switch (generationScale.Value)
                        {
                            case ScalePreset.Large:
                                officeDungeon.DungeonFlow.Length = new IntRange(5, 6);
                                officeDungeon.DungeonFlow.BranchCount = new IntRange(2, 3);
                                break;
                            default:
                                officeDungeon.DungeonFlow.Length = new IntRange(2, 3);
                                officeDungeon.DungeonFlow.BranchCount = new IntRange(1, 3);
                                break;
                        }
                        break;
                }

                officeMod.ExtendedDungeonFlows.Add(officeDungeon);

                officeExtender = Bundle.LoadAsset<DunGenExtender>("Assets/LethalCompany/Mods/LCOffice/OfficeExtender.asset");
                officeExtender.Properties.MainPathProperties.MainPathCount = mainpaths;
                officeExtender.DungeonFlow = officeDungeon.DungeonFlow;
                DunGenPlus.API.AddDunGenExtender(officeDungeon.DungeonFlow, officeExtender);

                mls.LogInfo($"Office registered with Length: (min: {officeDungeon.DungeonFlow.Length.Min}, max: {officeDungeon.DungeonFlow.Length.Max}) BranchCount: (min: {officeDungeon.DungeonFlow.BranchCount.Min}, max: {officeDungeon.DungeonFlow.BranchCount.Max})");

                if (configEnableScraps.Value)
                {
                    officeMod.ExtendedItems.AddRange(Bundle.LoadAllAssets<ExtendedItem>());
                }
                else
                {
                    officeMod.ExtendedItems.Add(Bundle.LoadAsset<ExtendedItem>("Assets/LethalCompany/Mods/LCOffice/Items/UpturnedApparatus.asset"));
                }

                PatchedContent.RegisterExtendedMod(officeMod);

                JNetworkPrefabSet netPrefabs = ScriptableObject.CreateInstance<JNetworkPrefabSet>();
                netPrefabs.SetName = modName;
                netPrefabs.AddPrefabs(
                    new JNetworkPrefabSet.JIdentifiablePrefab { name = "Elevator", prefab = ActualElevator = Bundle.LoadAsset<GameObject>("Assets/LethalCompany/Mods/LCOffice/Prefab/ActualElevator.prefab") },
                    new JNetworkPrefabSet.JIdentifiablePrefab { name = "StorageShelf", prefab = StoragePrefab = Bundle.LoadAsset<GameObject>("Assets/LethalCompany/Mods/LCOffice/Prefab/ElevatorStorage.prefab") }
                );
                JNetworkPrefabSet.NetworkPrefabSets.Add(netPrefabs);

                elevator1Node = Bundle.LoadAsset<TerminalNode>("Assets/LethalCompany/Mods/LCOffice/Terminal/Elevator1Node.asset");
                elevator2Node = Bundle.LoadAsset<TerminalNode>("Assets/LethalCompany/Mods/LCOffice/Terminal/Elevator2Node.asset");
                elevator3Node = Bundle.LoadAsset<TerminalNode>("Assets/LethalCompany/Mods/LCOffice/Terminal/Elevator3Node.asset");

                elevatorKeyword = Bundle.LoadAsset<TerminalKeyword>("Assets/LethalCompany/Mods/LCOffice/Terminal/ElevatorKeyword.asset");
                elevator1Keyword = Bundle.LoadAsset<TerminalKeyword>("Assets/LethalCompany/Mods/LCOffice/Terminal/Elevator1f.asset");
                elevator2Keyword = Bundle.LoadAsset<TerminalKeyword>("Assets/LethalCompany/Mods/LCOffice/Terminal/Elevator2f.asset");
                elevator3Keyword = Bundle.LoadAsset<TerminalKeyword>("Assets/LethalCompany/Mods/LCOffice/Terminal/Elevator3f.asset");

                Logger.LogInfo("[LC_Office] Successfully loaded assets!");

                JLL.JLL.HarmonyPatch(harmony, mls, typeof(TerminalPatch), typeof(LungPropPatch), typeof(MenuManagerPatch));
                JLL.JLL.NetcodePatch(mls, Assembly.GetExecutingAssembly().GetTypes());
            }
        }
    }
}