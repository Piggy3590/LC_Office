using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Netcode;
using LethalLib.Modules;
using System.Security;
using System.Security.Permissions;
using LethalLib.Extras;
using DunGen.Graph;
using DunGen;
using LethalLevelLoader;
using LCOffice.Patches;
using static LethalLib.Modules.Levels;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.HID;
using System.Collections;
using System.Management.Instrumentation;
using GameNetcodeStuff;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace LCOffice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.LCOffice";
        private const string modName = "LCOffice";
        private const string modVersion = "1.1.27";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        public static ManualLogSource mls;
        public static AssetBundle Bundle;

        public static AudioClip ElevatorOpen;
        public static AudioClip ElevatorClose;
        public static AudioClip ElevatorUp;
        public static AudioClip ElevatorDown;

        public static AudioClip stanleyVoiceline1;

        public static AudioClip bossaLullaby;
        public static AudioClip shopTheme;
        public static AudioClip saferoomTheme;
        public static AudioClip cootieTheme;

        public static AudioClip bossaLullabyLowPitch;
        public static AudioClip shopThemeLowPitch;
        public static AudioClip saferoomThemeLowPitch;

        public static AudioClip garageDoorSlam;
        public static AudioClip garageSlide;
        public static AudioClip floorOpen;
        public static AudioClip floorClose;

        public static AudioClip footstep1;
        public static AudioClip footstep2;
        public static AudioClip footstep3;
        public static AudioClip footstep4;
        public static AudioClip dogEatItem;
        public static AudioClip dogEatPlayer;
        public static AudioClip bigGrowl;
        public static AudioClip enragedScream;
        public static AudioClip dogSprint;
        public static AudioClip ripPlayerApart;
        public static AudioClip cry1;
        public static AudioClip dogHowl;
        public static AudioClip stomachGrowl;

        public static AudioClip eatenExplode;
        public static AudioClip dogSneeze;
        public static AudioClip dogSatisfied;

        public static GameObject shrimpPrefab;
        public static GameObject elevatorManager;
        public static GameObject storagePrefab;
        public static GameObject socketPrefab;
        public static GameObject socketInteractPrefab;
        public static GameObject insideCollider;
        public static EnemyType shrimpEnemy;
        public static EnemyType haltEnemy;

        public static GameObject officeRoundSystem;

        public static GameObject extLevelGeneration;

        public static ExtendedDungeonFlow officeExtendedDungeonFlow;

        public static DungeonArchetype officeArchetype;
        public static DungeonArchetype officeArchetype_A;

        public static DungeonFlow officeDungeonFlow;
        public static DungeonFlow officeDungeonFlow_A;
        public static TerminalNode shrimpTerminalNode;
        public static TerminalKeyword shrimpTerminalKeyword;

        public static TerminalKeyword elevatorKeyword;
        public static TerminalKeyword elevator1Keyword;
        public static TerminalKeyword elevator2Keyword;
        public static TerminalKeyword elevator3Keyword;

        public static TerminalNode elevator1Node;
        public static TerminalNode elevator2Node;
        public static TerminalNode elevator3Node;

        //public static TerminalNode haltFile;
        //public static TerminalKeyword haltTK;

        public static Item coinItem;
        public static GameObject coinPrefab;

        public static Item toolBoxItem;
        public static GameObject toolBoxPrefab;

        public static Item screwDriverItem;
        public static GameObject screwDriverPrefab;

        public static Item laptopItem;
        public static GameObject laptopPrefab;

        public static Item wrenchItem;
        public static GameObject wrenchPrefab;

        //public static DungeonArchetype CoolDungeonArchetype;
        //public static TileSet CoolTilesetStart;

        public static RuntimeDungeon dungeonGenerator;

        public static RuntimeAnimatorController playerScreenController;
        public static RuntimeAnimatorController playerScreenParentController;
        public static AudioClip haltMusic;
        public static AudioClip haltNoise1;
        public static AudioClip haltNoise2;
        public static AudioClip haltNoise3;
        public static AudioClip haltNoise4;
        public static AudioClip haltAttack;
        public static GameObject haltRoom;
        public static GameObject haltNoiseScreen;
        public static GameObject haltVolume;
        public static GameObject glitchSound;

        public static string PluginDirectory;

        private ConfigEntry<bool> configGuaranteedOffice;
        private ConfigEntry<int> configOfficeRarity;
        private ConfigEntry<string> configMoons;

        private ConfigEntry<int> shrimpSpawnWeight;
        //private ConfigEntry<int> shrimpModdedSpawnWeight;

        private ConfigEntry<int> configLengthOverride;

        private ConfigEntry<bool> configEnableScraps;

        public static bool setKorean;
        public static int configHaltPropability;
        public static bool configDisableCameraShake;
        public static bool configDiversityHaltBrighness;
        public static float musicVolume;
        public static bool elevatorMusicPitchdown;
        public static bool emergencyPowerSystem;

        public static bool cameraDisable;
        public static int cameraFrameSpeed;

        public static Item bottleItem;
        public static Item goldencupItem;

        public static ItemGroup itemGroupGeneral;
        public static ItemGroup itemGroupTabletop;
        public static ItemGroup itemGroupSmall;

        public static bool diversityIntergrated;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            PluginDirectory = base.Info.Location;

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
                officeDungeonFlow = Bundle.LoadAsset<DungeonFlow>("OfficeDungeonFlow.asset");
                officeArchetype = Bundle.LoadAsset<DungeonArchetype>("OfficeArchetype.asset");
                //officeDungeonFlow_A = Bundle.LoadAsset<DungeonFlow>("OfficeAdditionalFlow.asset");
                //officeArchetype_A = Bundle.LoadAsset<DungeonArchetype>("A_OfficeArchetype.asset");
                extLevelGeneration = Bundle.LoadAsset<GameObject>("ExtLevelGeneration.prefab");
                this.configOfficeRarity = base.Config.Bind<int>("General", "OfficeRarity", 40, new ConfigDescription("How rare it is for the office to be chosen. Higher values increases the chance of spawning the office.", new AcceptableValueRange<int>(0, 300), Array.Empty<object>()));
                this.configGuaranteedOffice = base.Config.Bind<bool>("General", "OfficeGuaranteed", false, new ConfigDescription("If enabled, the office will be effectively guaranteed to spawn. Only recommended for debugging/sightseeing purposes.", null, Array.Empty<object>()));
                this.configMoons = base.Config.Bind<string>("General", "OfficeMoonsList", "free", new ConfigDescription("The moon(s) that the office can spawn on, in the form of a comma separated list of selectable level names (e.g. \"TitanLevel,RendLevel,DineLevel\")\nNOTE: These must be the internal data names of the levels (all vanilla moons are \"MoonnameLevel\", for modded moon support you will have to find their name if it doesn't follow the convention).\nThe following strings: \"all\", \"vanilla\", \"modded\", \"paid\", \"free\", \"none\" are dynamic presets which add the dungeon to that specified group (string must only contain one of these, or a manual moon name list).\nDefault dungeon generation size is balanced around the dungeon scale multiplier of Titan (2.35), moons with significantly different dungeon size multipliers (see Lethal Company wiki for values) may result in dungeons that are extremely small/large.", null, Array.Empty<object>()));
                this.configLengthOverride = base.Config.Bind<int>("General", "OfficeLengthOverride", -1, new ConfigDescription(string.Format("If not -1, overrides the office length to whatever you'd like. Adjusts how long/large the dungeon generates.\nBe *EXTREMELY* careful not to set this too high (anything too big on a moon with a high dungeon size multipier can cause catastrophic problems, like crashing your computer or worse)\nFor reference, the default value for the current version [{0}] is {1}. If it's too big, make this lower e.g. 6, if it's too small use something like 10 (or higher, but don't go too crazy with it).", "1.1.2", 4), null, Array.Empty<object>()));

                this.configEnableScraps = base.Config.Bind<bool>("General", "OfficeCustomScrap", true, new ConfigDescription("When enabled, enables custom scrap spawning.", null, Array.Empty<object>()));

                musicVolume = (float)base.Config.Bind<float>("General", "ElevatorMusicVolume", 100, "Set the volume of music played in the elevator. (0 - 100)").Value;
                elevatorMusicPitchdown = (bool)base.Config.Bind<bool>("General", "ElevatorMusicPitchDown", true, "Lower the pitch of the elevator music.").Value;
                configDisableCameraShake = (bool)base.Config.Bind<bool>("General", "DisableCameraShake", false, "Turn off custom camera shake.").Value;
                configDiversityHaltBrighness = (bool)base.Config.Bind<bool>("General", "DiversityHaltBrighness", true, "Increase brightness when encountering Halt if Diversity mode is detected. Disabling it will make the game VERY difficult when encountering Halt!").Value;
                emergencyPowerSystem = (bool)base.Config.Bind<bool>("General", "EmergencyPowerSystem", false, "(EXPERIMENTAL) When set to true, if a apparatus is removed from a facility, the elevator will not operate until the apparatus is inserted into the elevator's emergency power unit.").Value;

                cameraDisable = (bool)base.Config.Bind<bool>("General", "Disable Camera", false, "Disable cameras inside the office.").Value;
                cameraFrameSpeed = (int)base.Config.Bind<int>("General", "Camera Frame Speed", 20, "Specifies the camera speed inside the office. If it is over 100, it changes to real-time capture. (FPS)").Value;

                this.shrimpSpawnWeight = base.Config.Bind<int>("Spawn", "ShrimpSpawnWeight", 5, new ConfigDescription("Sets the shrimp spawn weight for every moons.", null, Array.Empty<object>()));
                configHaltPropability = (int)base.Config.Bind<int>("Spawn", "HaltSpawnPropability", 12, "Sets the halt spawn propability for office interior. (0 - 100)").Value;
                //this.shrimpSpawnWeight = base.Config.Bind<string>("Spawn", "ShrimpSpawnWeight", "0,1,1,2,1,5,6,8", new ConfigDescription("Set the shrimp spawn weight for each moon. In this order:\n(experimentation, assurance, vow, march, offense, rend, dine, titan).", null, Array.Empty<object>()));
                //this.shrimpModdedSpawnWeight = base.Config.Bind<int>("Spawn", "ShrimpModdedMoonSpawnWeight", 5, new ConfigDescription("Set the shrimp spawn weight for modded moon. ", null, Array.Empty<object>()));

                setKorean = (bool)base.Config.Bind<bool>("Translation", "Enable Korean", false, "Set language to Korean.").Value;

                if (this.configLengthOverride.Value == -1)
                {
                    officeDungeonFlow.Length.Min = 3;
                    officeDungeonFlow.Length.Max = 3;
                }
                else
                {
                    mls.LogInfo(string.Format("Office length override has been set to {0}. Be careful with this value.", this.configLengthOverride.Value));
                    officeDungeonFlow.Length.Min = this.configLengthOverride.Value;
                    officeDungeonFlow.Length.Max = this.configLengthOverride.Value;
                    //officeDungeonFlow_A.Length.Min = this.configLengthOverride.Value;
                    //officeDungeonFlow_A.Length.Max = this.configLengthOverride.Value;
                }
                ExtendedDungeonFlow officeExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
                officeExtendedDungeonFlow.contentSourceName = "LC Office";
                officeExtendedDungeonFlow.dungeonFlow = officeDungeonFlow;
                officeExtendedDungeonFlow.dungeonDefaultRarity = 0;
                //officeExtendedDungeonFlow.dungeonFirstTimeAudio

                int num = (configGuaranteedOffice.Value ? 99999 : configOfficeRarity.Value);
                string text = this.configMoons.Value.ToLower();
                bool flag7 = text == "all";
                if (flag7)
                {
                    officeExtendedDungeonFlow.dynamicLevelTagsList.Add(new StringWithRarity("Vanilla", num));
                    officeExtendedDungeonFlow.dynamicLevelTagsList.Add(new StringWithRarity("Custom", num));
                    mls.LogInfo("Registered Office dungeon for all moons.");
                }
                else
                {
                    bool flag8 = text == "vanilla";
                    if (flag8)
                    {
                        officeExtendedDungeonFlow.dynamicLevelTagsList.Add(new StringWithRarity("Lethal Company", num));
                        mls.LogInfo("Registered Office dungeon for all vanilla moons.");
                    }
                    else
                    {
                        bool flag9 = text == "modded";
                        if (flag9)
                        {
                            officeExtendedDungeonFlow.dynamicLevelTagsList.Add(new StringWithRarity("Custom", num));
                            mls.LogInfo("Registered Office dungeon for all modded moons.");
                        }
                        else
                        {
                            bool flag10 = text == "paid";
                            if (flag10)
                            {
                                officeExtendedDungeonFlow.dynamicRoutePricesList.Add(new Vector2WithRarity(new Vector2(1f, 9999f), num));
                                mls.LogInfo("Registered Office dungeon for all paid moons.");
                            }
                            else
                            {
                                bool flag11 = text == "free";
                                if (flag11)
                                {
                                    officeExtendedDungeonFlow.dynamicRoutePricesList.Add(new Vector2WithRarity(new Vector2(0f, 0f), num));
                                    mls.LogInfo("Registered Office dungeon for all free moons.");
                                }
                                else
                                {
                                    mls.LogInfo("Registering Office dungeon for predefined moon list.");
                                    string[] array3 = configMoons.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    List<StringWithRarity> list = new List<StringWithRarity>();
                                    for (int k = 0; k < array3.Length; k++)
                                    {
                                        string[] array4 = array3[k].Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                                        int num2 = array4.Length;
                                        bool flag12 = num2 > 2;
                                        if (flag12)
                                        {
                                            mls.LogError("Invalid setup for moon rarity config: " + array3[k] + ". Skipping.");
                                        }
                                        else
                                        {
                                            bool flag13 = num2 == 1;
                                            if (flag13)
                                            {
                                                mls.LogInfo(string.Format("Registering Office dungeon for moon {0} at default rarity {1}", array3[k], num));
                                                list.Add(new StringWithRarity(array3[k], num));
                                            }
                                            else
                                            {
                                                int num3;
                                                bool flag14 = !int.TryParse(array4[1], out num3);
                                                if (flag14)
                                                {
                                                    mls.LogError(string.Concat(new string[]
                                                    {
                                                            "Failed to parse rarity value for moon ",
                                                            array4[0],
                                                            ": ",
                                                            array4[1],
                                                            ". Skipping."
                                                    }));
                                                }
                                                else
                                                {
                                                    mls.LogInfo(string.Format("Registering Office dungeon for moon {0} at default rarity {1}", array3[k], num));
                                                    list.Add(new StringWithRarity(array4[0], num3));
                                                }
                                            }
                                        }
                                    }
                                    officeExtendedDungeonFlow.manualPlanetNameReferenceList = list;
                                }
                            }
                        }
                    }
                }

                officeExtendedDungeonFlow.dungeonSizeMin = 1f;
                officeExtendedDungeonFlow.dungeonSizeMax = 1f;
                officeExtendedDungeonFlow.dungeonSizeLerpPercentage = 0f;
                PatchedContent.RegisterExtendedDungeonFlow(officeExtendedDungeonFlow);

                shrimpPrefab = Bundle.LoadAsset<GameObject>("Shrimp.prefab");
                shrimpEnemy = Bundle.LoadAsset<EnemyType>("ShrimpEnemy.asset");
                haltEnemy = Bundle.LoadAsset<EnemyType>("HaltEnemy.asset");

                elevatorManager = Bundle.LoadAsset<GameObject>("ElevatorSystem.prefab");

                storagePrefab = Bundle.LoadAsset<GameObject>("DepositPlace.prefab");
                socketPrefab = Bundle.LoadAsset<GameObject>("ElevatorSocket.prefab");
                socketInteractPrefab = Bundle.LoadAsset<GameObject>("LungPlacement.prefab");
                officeRoundSystem = Bundle.LoadAsset<GameObject>("OfficeRoundSystem.prefab");
                insideCollider = Bundle.LoadAsset<GameObject>("InsideCollider.prefab");

                bossaLullaby = Bundle.LoadAsset<AudioClip>("bossa_lullaby_refiltered.ogg");
                shopTheme = Bundle.LoadAsset<AudioClip>("shop_refiltered.ogg");
                saferoomTheme = Bundle.LoadAsset<AudioClip>("saferoom_refiltered.ogg");

                bossaLullabyLowPitch = Bundle.LoadAsset<AudioClip>("bossa_lullaby_low_wpitch.ogg");
                shopThemeLowPitch = Bundle.LoadAsset<AudioClip>("shop_low_wpitch.ogg");
                saferoomThemeLowPitch = Bundle.LoadAsset<AudioClip>("saferoom_low_wpitch.ogg");
                //cootieTheme = Bundle.LoadAsset<AudioClip>("cootie_low.ogg");

                ElevatorOpen = Bundle.LoadAsset<AudioClip>("ElevatorOpen.ogg");
                ElevatorClose = Bundle.LoadAsset<AudioClip>("ElevatorClose.ogg");
                ElevatorDown = Bundle.LoadAsset<AudioClip>("ElevatorDown.ogg");
                ElevatorUp = Bundle.LoadAsset<AudioClip>("ElevatorUp.ogg");

                garageDoorSlam = Bundle.LoadAsset<AudioClip>("GarageDoorSlam.ogg");
                garageSlide = Bundle.LoadAsset<AudioClip>("GarageDoorSlide1.ogg");
                floorOpen = Bundle.LoadAsset<AudioClip>("FloorOpen.ogg");
                floorClose = Bundle.LoadAsset<AudioClip>("FloorClosed.ogg");

                footstep1 = Bundle.LoadAsset<AudioClip>("Footstep1.ogg");
                footstep2 = Bundle.LoadAsset<AudioClip>("Footstep2.ogg");
                footstep3 = Bundle.LoadAsset<AudioClip>("Footstep3.ogg");
                footstep4 = Bundle.LoadAsset<AudioClip>("Footstep4.ogg");
                dogEatItem = Bundle.LoadAsset<AudioClip>("DogEatObject.ogg");
                dogEatPlayer = Bundle.LoadAsset<AudioClip>("EatPlayer.ogg");
                bigGrowl = Bundle.LoadAsset<AudioClip>("BigGrowl.ogg");
                enragedScream = Bundle.LoadAsset<AudioClip>("DogRage.ogg");
                dogSprint = Bundle.LoadAsset<AudioClip>("DogSprint.ogg");
                ripPlayerApart = Bundle.LoadAsset<AudioClip>("RipPlayerApart.ogg");
                cry1 = Bundle.LoadAsset<AudioClip>("Cry1.ogg");
                dogHowl = Bundle.LoadAsset<AudioClip>("DogHowl.ogg");
                stomachGrowl = Bundle.LoadAsset<AudioClip>("StomachGrowl.ogg");

                eatenExplode = Bundle.LoadAsset<AudioClip>("eatenExplode.ogg");
                dogSneeze = Bundle.LoadAsset<AudioClip>("Sneeze.ogg");
                dogSatisfied = Bundle.LoadAsset<AudioClip>("PlayBow.ogg");

                stanleyVoiceline1 = Bundle.LoadAsset<AudioClip>("stanley.ogg");

                shrimpTerminalNode = Bundle.LoadAsset<TerminalNode>("ShrimpFile.asset");
                shrimpTerminalKeyword = Bundle.LoadAsset<TerminalKeyword>("shrimpTK.asset");

                elevator1Node = Bundle.LoadAsset<TerminalNode>("Elevator1Node.asset");
                elevator2Node = Bundle.LoadAsset<TerminalNode>("Elevator2Node.asset");
                elevator3Node = Bundle.LoadAsset<TerminalNode>("Elevator3Node.asset");

                elevatorKeyword = Bundle.LoadAsset<TerminalKeyword>("ElevatorKeyword.asset");
                elevator1Keyword = Bundle.LoadAsset<TerminalKeyword>("Elevator1f.asset");
                elevator2Keyword = Bundle.LoadAsset<TerminalKeyword>("Elevator2f.asset");
                elevator3Keyword = Bundle.LoadAsset<TerminalKeyword>("Elevator3f.asset");

                //haltFile = Bundle.LoadAsset<TerminalNode>("haltFile.asset");
                //haltTK = Bundle.LoadAsset<TerminalKeyword>("haltTK.asset");

                //Items
                /*
                coinItem = Bundle.LoadAsset<Item>("Coin.asset");
                coinPrefab = Bundle.LoadAsset<GameObject>("Coin.prefab");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(coinPrefab);
                Utilities.FixMixerGroups(coinPrefab);

                toolBoxItem = Bundle.LoadAsset<Item>("Toolbox.asset");
                toolBoxPrefab = Bundle.LoadAsset<GameObject>("Toolbox.prefab");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(toolBoxPrefab);
                Utilities.FixMixerGroups(toolBoxPrefab);

                screwDriverItem = Bundle.LoadAsset<Item>("ScrewDriver.asset");
                screwDriverPrefab = Bundle.LoadAsset<GameObject>("ScrewDriver.prefab");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(screwDriverPrefab);
                Utilities.FixMixerGroups(screwDriverPrefab);

                laptopItem = Bundle.LoadAsset<Item>("Laptop.asset");
                laptopPrefab = Bundle.LoadAsset<GameObject>("Laptop.prefab");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(laptopPrefab);
                Utilities.FixMixerGroups(laptopPrefab);

                wrenchItem = Bundle.LoadAsset<Item>("Wrench.asset");
                wrenchPrefab = Bundle.LoadAsset<GameObject>("Wrench.prefab");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(wrenchPrefab);
                Utilities.FixMixerGroups(wrenchPrefab);

                if (this.configEnableScraps.Value)
                {
                    Items.RegisterScrap(coinItem, 8, LevelTypes.All);
                    Items.RegisterScrap(toolBoxItem, 4, LevelTypes.All);
                    Items.RegisterScrap(screwDriverItem, 10, LevelTypes.All);
                    Items.RegisterScrap(laptopItem, 3, LevelTypes.All);
                    Items.RegisterScrap(wrenchItem, 8, LevelTypes.All);
                }
                */

                playerScreenController = Bundle.LoadAsset<RuntimeAnimatorController>("PlayerScreenRe.controller");
                playerScreenParentController = Bundle.LoadAsset<RuntimeAnimatorController>("PlayerScreenParent.controller");
                haltMusic = Bundle.LoadAsset<AudioClip>("HaltMusic.wav");
                haltNoise1 = Bundle.LoadAsset<AudioClip>("HaltNoise1.wav");
                haltNoise2 = Bundle.LoadAsset<AudioClip>("HaltNoise2.wav");
                haltNoise3 = Bundle.LoadAsset<AudioClip>("HaltNoise3.wav");
                haltNoise4 = Bundle.LoadAsset<AudioClip>("HaltNoise4.wav");
                haltAttack = Bundle.LoadAsset<AudioClip>("HaltAttack.wav");
                haltRoom = Bundle.LoadAsset<GameObject>("HaltTile.prefab");
                haltNoiseScreen = Bundle.LoadAsset<GameObject>("NoiseScreen.prefab");
                haltVolume = Bundle.LoadAsset<GameObject>("HaltVolume.prefab");
                glitchSound = Bundle.LoadAsset<GameObject>("GlitchSound.prefab");

                if (!setKorean)
                {
                    shrimpTerminalNode.displayText = "Shrimp\r\n\r\nSigurd’s Danger Level: 60%\r\n\r\n\nScientific name: Canispiritus-Artemus\r\n\r\nShrimps are dog-like creatures, known to be the first tenant of the Upturned Inn. For the most part, he is relatively friendly to humans, following them around, curiously stalking them. Unfortunately, their passive temperament comes with a dangerously vicious hunger.\r\nDue to the nature of their biology, he has a much more unique stomach organ than most other creatures. The stomach lining is flexible, yet hardy, allowing a Shrimp to digest and absorb the nutrients from anything, biological or not, so long as it isn’t too large.\r\n\r\nHowever, this evolutionary adaptation was most likely a result of their naturally rapid metabolism. He uses nutrients so quickly that he needs to eat multiple meals a day to survive. The time between these meals are inconsistent, as the rate of caloric consumption is variable. This can range from hours to even minutes and causes the shrimp to behave monstrously if he has not eaten for a while.\r\n\r\nKnown to live in abandoned buildings, shrimp can often be seen in large abandoned factories or offices scavenging for scrap metal, to eat. That isn’t to say he can’t be found elsewhere. He is usually a lone hunters and expert trackers out of necessity.\r\n\r\nSigurd’s Note:\r\nIf this guy spots you, you’ll want to drop something you’re holding and let him eat it. It’s either you or that piece of scrap on you.\r\n\r\nit’s best to avoid letting him spot you. I swear… it’s almost like his eyes are staring into your soul.\r\nI never want to see one of these guys behind me again.\r\n\r\n\r\nIK: <i>Sir, don't be sad! Shrimp didn't hate you.\r\nhe was just... hungry.</i>\r\n\r\n";
                    shrimpTerminalNode.creatureName = "Shrimp";
                    shrimpTerminalKeyword.word = "shrimp";
                    /*
                    haltFile.displayText = "Halt\r\n\r\nSigurd’s Danger Level: 40%\r\n\r\n\r\nHalt takes an appearance as a large translucent blue ghost, with illuminating, glowing cyan eyes and a static aura. Its ghostly body has a waveform scale and a visible opening at the bottom.\r\n\r\n\r\nHalt mainly lives in long hallways and has no way of identifying its target until it enters the hallway. If the target enters Halt's hallway, the target will hallucinate and the hallway will feel much longer.\r\nHalt moves slowly towards his target and attempts to kill him. If the target is close enough, Halt will damage the target and send it back to the center of the hallway.\r\n\r\nOften Halt will stop the chase and resume the chase in the opposite direction of the hallway. At this point, the suit's HUD will display the message \"TURN AROUND.\"\r\nIf the target reaches the end of the hallway, all of Halt's attacks stop.\r\n\r\nNothing is known about how Halt attacks.\r\n";
                    haltFile.creatureName = "Halt";
                    haltTK.word = "halt";
                    */
                }
                else
                {
                    shrimpTerminalNode.displayText = "쉬림프\r\n\r\n시구르드의 위험 수준: 60%\r\n\r\n\n학명: 카니스피리투스-아르테무스\r\n\r\n쉬림프는 개를 닮은 생명체로 Upturned Inn의 첫 번째 세입자로 알려져 있습니다. 평소에는 상대적으로 우호적이며, 호기심을 가지고 인간을 따라다닙니다. 불행하게도 그는 위험할 정도로 굉장한 식욕을 가지고 있습니다.\r\n생물학적 특성으로 인해, 그는 대부분의 다른 생물보다 훨씬 더 독특한 위장 기관을 가지고 있습니다. 위 내막은 유연하면서도 견고하기 때문에 어떤 물체라도 영양분을 소화하고 흡수할 수 있습니다.\r\n그러나 이러한 진화적 적응은 자연적으로 빠른 신진대사의 결과일 가능성이 높습니다. 그는 영양분을 너무 빨리 사용하기 때문에 생존하려면 하루에 여러 끼를 먹어야 합니다.\r\n칼로리 소비율이 다양하기 때문에 식사 사이의 시간이 일정하지 않습니다. 이는 몇 시간에서 몇 분까지 지속될 수 있으며, 쉬림프가 오랫동안 무언가를 먹지 않으면 매우 포악해지며 따라다니던 사람을 쫒습니다.\r\n\r\n버려진 건물에 사는 것으로 알려진 쉬림프는 버려진 공장이나 사무실에서 폐철물을 찾아다니는 것으로 발견할 수 있습니다. 그렇다고 다른 곳에서 그를 찾을 수 없다는 말은 아닙니다. 그는 일반적으로 고독한 사냥꾼이며, 때로는 전문적인 추적자가 되기도 합니다.\r\n\r\n시구르드의 노트: 이 녀석이 으르렁거리는 소리를 듣게 된다면, 먹이를 줄 수 있는 무언가를 가지고 있기를 바라세요. 아니면 당신이 이 녀석의 식사가 될 거예요.\r\n맹세컨대... 마치 당신의 영혼을 들여다보는 것 같아요. 다시는 내 뒤에서 이 녀석을 보고 싶지 않아요.\r\n\r\n\r\nIK: <i>손님, 슬퍼하지 마세요! 쉬림프는 당신을 싫어하지 않는답니다.\r\n걔는 그냥... 배고플 뿐이에요.</i>\r\n\r\n";
                    shrimpTerminalNode.creatureName = "쉬림프";
                    shrimpTerminalKeyword.word = "쉬림프";
                    /*
                    haltFile.displayText = "홀트\r\n\r\n시구르드의 위험 수준: 40%\r\n\r\n홀트는 빛을 발하는 청록색 눈과 정적인 분위기를 풍기는 커다란 반투명 파란색 유령의 형상을 가진 생명체입니다. \r\n\r\n주로 긴 복도에 서식하는 홀트는 목표물이 직접 복도에 들어서기 전까지는 파악할 방법이 없습니다. 만약 목표물이 홀트의 복도에 진입한다면 목표물에게 환각을 일으키며, 이 때 목표물은 복도가 훨씬 길게 느껴지게 됩니다.\r\n\r\n홀트는 목표물을 향해 천천히 움직이며 목표물을 처치하려고 시도합니다. 만약 목표물과 거리가 충분히 가깝다면 홀트는 목표물에게 피해를 주고 복도의 중앙으로 돌려보냅니다.\r\n종종 홀트는 추격을 멈추고 복도의 반대 방향에서 다시 추격하기도 합니다. 이 때 슈트의 HUD에서 \"TURN AROUND\"라는 메시지가 표시됩니다.\r\n만약 목표물이 복도의 끝에 다다른다면 홀트의 모든 공격이 멈춥니다.\r\n\r\n홀트이 공격하는 방식에 대해서는 알려진 것이 없습니다.";
                    haltFile.creatureName = "홀트";
                    haltTK.word = "홀트";
                    */

                    /*
                    coinItem.itemName = "동전";
                    coinPrefab.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "동전";

                    toolBoxItem.itemName = "공구 상자";
                    toolBoxPrefab.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "공구 상자";

                    screwDriverItem.itemName = "스크류 드라이버";
                    screwDriverPrefab.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "스크류 드라이버";

                    laptopItem.itemName = "노트북";
                    laptopPrefab.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "노트북";

                    wrenchItem.itemName = "렌치";
                    wrenchPrefab.transform.GetChild(1).GetComponent<ScanNodeProperties>().headerText = "렌치";
                    */
                }

                //LethalLib.Modules.Enemies.RegisterEnemy(haltEnemy, 0, Levels.LevelTypes.All, Enemies.SpawnType.Default, haltFile, haltTK);
                /*
                int[] numbers = shrimpSpawnWeight.Value.Split(',').Select(int.Parse).ToArray();
                LevelTypes[] levelTypes = {
                    Levels.LevelTypes.ExperimentationLevel,
                    Levels.LevelTypes.AssuranceLevel,
                    Levels.LevelTypes.VowLevel,
                    Levels.LevelTypes.MarchLevel,
                    Levels.LevelTypes.OffenseLevel,
                    Levels.LevelTypes.RendLevel,
                    Levels.LevelTypes.DineLevel,
                    Levels.LevelTypes.TitanLevel };

                for (int i = 0; i < numbers.Length; i++)
                {
                    Levels.LevelTypes levelType = levelTypes[i];
                    LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, numbers[i], levelType, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
                    Plugin.mls.LogInfo("Shrimp spawn chance in " + levelTypes[i] + ": " + numbers[i]);
                }

                LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, shrimpModdedSpawnWeight.Value, Levels.LevelTypes.Modded, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
                Plugin.mls.LogInfo("Shrimp spawn chance in Modded Moons: " + shrimpModdedSpawnWeight.Value);
                */

                //socketInteractPrefab.AddComponent<PlaceLung>();
                socketInteractPrefab.AddComponent<ElevatorSystem>();

                ShrimpAI shrimpAI = shrimpPrefab.AddComponent<ShrimpAI>();
                shrimpPrefab.transform.GetChild(0).gameObject.AddComponent<ShrimpCollider>().shrimpAI = shrimpAI;
                //shrimpPrefab.transform.GetChild(0).GetComponent<EnemyAICollisionDetect>().mainScript = shrimpAI;

                elevatorManager.AddComponent<ElevatorSystem>();
                officeRoundSystem.AddComponent<OfficeRoundSystem>();
                insideCollider.AddComponent<ElevatorSystem>();
                insideCollider.AddComponent<ElevatorCollider>();

                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(shrimpPrefab);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(elevatorManager);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(storagePrefab);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(socketInteractPrefab);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(officeRoundSystem);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(insideCollider);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(glitchSound);

                LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, shrimpSpawnWeight.Value, Levels.LevelTypes.All, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
                //LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 10, Levels.LevelTypes.All, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);

                //LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 0, Levels.LevelTypes.All, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);

                //shrimpEnemy.MaxCount = CustomConfig.MaxPerLevel;
                base.Logger.LogInfo("[LC_Office] Successfully loaded assets!");

                harmony.PatchAll(typeof(Plugin));
                harmony.PatchAll(typeof(PlayerControllerBPatch));
                harmony.PatchAll(typeof(GrabbableObjectPatch));
                harmony.PatchAll(typeof(TerminalPatch));
                harmony.PatchAll(typeof(StartOfRoundPatch));
                harmony.PatchAll(typeof(GameNetworkManagerPatch));
                //harmony.PatchAll(typeof(PlaceLung));
            }
        }

        void Start()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.Keys.Any(k => k == "Chaos.Diversity"))
            {
                mls.LogInfo("LC_Office found Diversity!");
                diversityIntergrated = true;
            }
        }
        /*
        [HarmonyPatch(typeof(LocalPropSet))]
        internal class LocalPropSetPatch
        {
            public List<TileSet> tilesets;
            public void Awake(ref GameObjectChanceTable props)
            {
                foreach (GameObjectChance gameObjectChance in props.Weights)
                {
                    if (gameObjectChance.TileSet == null)
                    {
                        gameObjectChance.TileSet = A_OfficeHallwayTileset;
                    }
                }
            }
        }
        */

        [HarmonyPatch(typeof(RoundManager))]
        internal class RoundManagerPatch
        {
            public static float spawnTimer;
            // Find and replace the dummy scrap items with the ones already in LE (for some reason it compares the hash of the item directly, not any actual data in it)
            [HarmonyPatch("SpawnScrapInLevel")]
            [HarmonyPrefix]
            private static bool SetItemSpawnPoints(ref RuntimeDungeon ___dungeonGenerator)
            {
                if (___dungeonGenerator.Generator.DungeonFlow.name != "OfficeDungeonFlow") return true;
                RoundManager roundManager = RoundManager.Instance;

                Vector3 levelGenRoot = GameObject.Find("LevelGenerationRoot").transform.position;
                GameObject.Instantiate(extLevelGeneration, new Vector3(levelGenRoot.x - 130, levelGenRoot.y, levelGenRoot.z - 130), Quaternion.Euler(0, 0, 0));

                dungeonGenerator = GameObject.Find("A_DungeonGenerator").GetComponent<RuntimeDungeon>();
                roundManager.SpawnSyncedProps();


                if (GameObject.Find("OfficeTeleport(Clone)") != null)
                {
                    EntranceTeleport entranceTeleport = GameObject.Find("OfficeTeleport(Clone)").GetComponent<EntranceTeleport>();
                    entranceTeleport.entranceId = 40;
                }
                else
                {
                    //EntranceTeleport entranceTeleport = GameObject.Find("OfficeTeleport(Clone)").GetComponent<EntranceTeleport>();
                    //entranceTeleport.entranceId = 40;
                }
                if (GameObject.Find("OfficeOutsideTeleport(Clone)") != null)
                {
                    EntranceTeleport entranceTeleport = GameObject.Find("OfficeOutsideTeleport(Clone)").GetComponent<EntranceTeleport>();
                    entranceTeleport.entranceId = 40;
                }

                /*
                RuntimeDungeon[] runtimeDungeons = GameObject.FindObjectsOfType<RuntimeDungeon>();
                RuntimeDungeon dungeonGenerator = new RuntimeDungeon();

                foreach (RuntimeDungeon runtimeDungeon in runtimeDungeons)
                {
                    if (runtimeDungeon.Generator.Root != null && runtimeDungeon.Generator.DungeonFlow != null)
                    {
                        dungeonGenerator.Generator.allowImmediateRepeats = runtimeDungeon.Generator.allowImmediateRepeats;
                        dungeonGenerator.Generator.Seed = runtimeDungeon.Generator.Seed;
                        dungeonGenerator.Generator.ShouldRandomizeSeed = runtimeDungeon.Generator.ShouldRandomizeSeed;
                        dungeonGenerator.Generator.MaxAttemptCount = runtimeDungeon.Generator.MaxAttemptCount;
                        dungeonGenerator.Generator.UseMaximumPairingAttempts = runtimeDungeon.Generator.UseMaximumPairingAttempts;
                        dungeonGenerator.Generator.MaxPairingAttempts = runtimeDungeon.Generator.MaxPairingAttempts;
                        dungeonGenerator.Generator.IgnoreSpriteBounds = runtimeDungeon.Generator.IgnoreSpriteBounds;
                        dungeonGenerator.Generator.UpDirection = runtimeDungeon.Generator.UpDirection;
                        dungeonGenerator.Generator.OverrideRepeatMode = runtimeDungeon.Generator.OverrideRepeatMode;
                        dungeonGenerator.Generator.RepeatMode = runtimeDungeon.Generator.RepeatMode;
                        dungeonGenerator.Generator.OverrideAllowTileRotation = runtimeDungeon.Generator.OverrideAllowTileRotation;
                        dungeonGenerator.Generator.AllowTileRotation = runtimeDungeon.Generator.AllowTileRotation;
                        dungeonGenerator.Generator.LengthMultiplier = runtimeDungeon.Generator.LengthMultiplier;
                        dungeonGenerator.Generator.PlaceTileTriggers = runtimeDungeon.Generator.PlaceTileTriggers;
                        dungeonGenerator.Generator.TileTriggerLayer = runtimeDungeon.Generator.TileTriggerLayer;
                        dungeonGenerator.Generator.GenerateAsynchronously = runtimeDungeon.Generator.GenerateAsynchronously;
                        dungeonGenerator.Generator.MaxAsyncFrameMilliseconds = runtimeDungeon.Generator.MaxAsyncFrameMilliseconds;
                        dungeonGenerator.Generator.PauseBetweenRooms = runtimeDungeon.Generator.PauseBetweenRooms;
                        dungeonGenerator.Generator.RestrictDungeonToBounds = runtimeDungeon.Generator.RestrictDungeonToBounds;
                        dungeonGenerator.Generator.TilePlacementBounds = runtimeDungeon.Generator.TilePlacementBounds;
                        dungeonGenerator.Generator.OverlapThreshold = runtimeDungeon.Generator.OverlapThreshold;
                        dungeonGenerator.Generator.Padding = runtimeDungeon.Generator.Padding;
                        dungeonGenerator.Generator.DisallowOverhangs = runtimeDungeon.Generator.DisallowOverhangs;
                        dungeonGenerator.Generator.fileVersion = runtimeDungeon.Generator.fileVersion;
                    }
                }
                */
                return true;
            }
        }
    }
}