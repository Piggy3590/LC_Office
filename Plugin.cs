using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using Unity.Netcode;
using LethalLib.Modules;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using LethalLib.Extras;
using DunGen.Graph;
using DunGen;
using LethalLevelLoader;
using LCOffice.Patches;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Linq;
using static LethalLib.Modules.Levels;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace LCOffice
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.LCOffice";
        private const string modName = "LCOffice";
        private const string modVersion = "0.1.1";

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

        public static AudioClip garageDoorSlam;
        public static AudioClip garageSlide;
        public static AudioClip floorOpen;
        public static AudioClip floorClose;

        public static AudioClip footstep1;
        public static AudioClip footstep2;
        public static AudioClip footstep3;
        public static AudioClip footstep4;
        public static AudioClip dogEatItem;
        public static AudioClip bigGrowl;
        public static AudioClip enragedScream;
        public static AudioClip dogSprint;
        public static AudioClip ripPlayerApart;
        public static AudioClip cry1;
        public static AudioClip dogHowl;

        public static GameObject shrimpPrefab;
        public static EnemyType shrimpEnemy;

        public static DungeonFlow officeDungeonFlow;
        public static TerminalNode shrimpTerminalNode;
        public static TerminalKeyword shrimpTerminalKeyword;
        //public static DungeonArchetype CoolDungeonArchetype;
        //public static TileSet CoolTileset;
        //public static TileSet CoolTilesetStart;

        public static string PluginDirectory;

        private ConfigEntry<bool> configGuaranteedOffice;
        private ConfigEntry<int> configOfficeRarity;
        private ConfigEntry<string> configMoons;

        public static bool setKorean;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            this.configOfficeRarity = base.Config.Bind<int>("General", "OfficeRarity", 50, new ConfigDescription("How rare it is for the office to be chosen. Higher values increases the chance of spawning the office.", new AcceptableValueRange<int>(0, 300), Array.Empty<object>()));
            this.configGuaranteedOffice = base.Config.Bind<bool>("General", "OfficeGuaranteed", false, new ConfigDescription("If enabled, the office will be effectively guaranteed to spawn. Only recommended for debugging/sightseeing purposes.", null, Array.Empty<object>()));
            this.configMoons = base.Config.Bind<string>("General", "OfficeMoonsList", "free", new ConfigDescription("The moon(s) that the office can spawn on, in the form of a comma separated list of selectable level names (e.g. \"TitanLevel,RendLevel,DineLevel\")\nNOTE: These must be the internal data names of the levels (all vanilla moons are \"MoonnameLevel\", for modded moon support you will have to find their name if it doesn't follow the convention).\nThe following strings: \"all\", \"vanilla\", \"modded\", \"paid\", \"free\" are dynamic presets which add the dungeon to that specified group (string must only contain one of these, or a manual moon name list).\nDefault dungeon generation size is balanced around the dungeon scale multiplier of Titan (2.35), moons with significantly different dungeon size multipliers (see Lethal Company wiki for values) may result in dungeons that are extremely small/large.", null, Array.Empty<object>()));
            
            setKorean = (bool)base.Config.Bind<bool>("Translation", "Enable Korean", false, "Set language to Korean.").Value;


            PluginDirectory = base.Info.Location;
            
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("LC_Office is loaded!");

            Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "officedungeon"));

            //Item kiwi = Bundle.LoadAsset<Item>("KiwiItem2.asset");
            //LethalLib.Modules.Items.RegisterScrap(kiwi, 1000000, Levels.LevelTypes.All);

            officeDungeonFlow = Bundle.LoadAsset<DungeonFlow>("OfficeDungeonFlow.asset");

            ExtendedDungeonFlow officeExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            officeExtendedDungeonFlow.dungeonFlow = officeDungeonFlow;
            officeExtendedDungeonFlow.dungeonDefaultRarity = 0;

            int num = (this.configGuaranteedOffice.Value ? 99999 : this.configOfficeRarity.Value);

            if (this.configMoons.Value.ToLower() == "all")
            {
                officeExtendedDungeonFlow.manualContentSourceNameReferenceList.Add(new StringWithRarity("Lethal Company", num));
                officeExtendedDungeonFlow.manualContentSourceNameReferenceList.Add(new StringWithRarity("Custom", num));
            }
            else if (this.configMoons.Value.ToLower() == "vanilla")
            {
                officeExtendedDungeonFlow.manualContentSourceNameReferenceList.Add(new StringWithRarity("Lethal Company", num));
            }
            else if (this.configMoons.Value.ToLower() == "modded")
            {
                officeExtendedDungeonFlow.dynamicLevelTagsList.Add(new StringWithRarity("Custom", num));
            }
            else if (this.configMoons.Value.ToLower() == "paid")
            {
                officeExtendedDungeonFlow.dynamicRoutePricesList.Add(new Vector2WithRarity(new Vector2(1f, 9999f), num));
            }
            else if (this.configMoons.Value.ToLower() == "free")
            {
                officeExtendedDungeonFlow.dynamicRoutePricesList.Add(new Vector2WithRarity(new Vector2(0f, 0f), num));
            }
            else if (this.configMoons.Value.ToLower() == "none")
            {
                //none!!!
            }
            else
            {
                string[] array3 = this.configMoons.Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                StringWithRarity[] array4 = new StringWithRarity[array3.Length];
                for (int k = 0; k < array3.Length; k++)
                {
                    array4[k] = new StringWithRarity(array3[k], num);
                }
                officeExtendedDungeonFlow.manualPlanetNameReferenceList = array4.ToList<StringWithRarity>();
            }
            officeExtendedDungeonFlow.dungeonSizeMin = 1f;
            officeExtendedDungeonFlow.dungeonSizeMax = 3f;
            officeExtendedDungeonFlow.dungeonSizeLerpPercentage = 0f;
            AssetBundleLoader.RegisterExtendedDungeonFlow(officeExtendedDungeonFlow);

            shrimpPrefab = Bundle.LoadAsset<GameObject>("Shrimp.prefab");
            shrimpEnemy = Bundle.LoadAsset<EnemyType>("ShrimpEnemy.asset");
            shrimpEnemy.enemyPrefab.AddComponent<ShrimpAI>();

            GameObject startroom = Bundle.LoadAsset<GameObject>("OfficeStartRoom.prefab");
            startroom.AddComponent<ElevatorSystem>();
            Bundle.LoadAsset<GameObject>("TrapRoom").AddComponent<TrapRoomTrigger>();

            bossaLullaby = Bundle.LoadAsset<AudioClip>("bossa_lullaby_refiltered.wav");
            shopTheme = Bundle.LoadAsset<AudioClip>("saferoom_low_refiltered.wav");
            saferoomTheme = Bundle.LoadAsset<AudioClip>("saferoom_low_refiltered.wav");
            //cootieTheme = Bundle.LoadAsset<AudioClip>("cootie_low.wav");

            ElevatorOpen = Bundle.LoadAsset<AudioClip>("ElevatorOpen.wav");
            ElevatorClose = Bundle.LoadAsset<AudioClip>("ElevatorClose.wav");
            ElevatorDown = Bundle.LoadAsset<AudioClip>("ElevatorDown.wav");
            ElevatorUp = Bundle.LoadAsset<AudioClip>("ElevatorUp.wav");

            garageDoorSlam = Bundle.LoadAsset<AudioClip>("GarageDoorSlam.ogg");
            garageSlide = Bundle.LoadAsset<AudioClip>("GarageDoorSlide1.ogg");
            floorOpen = Bundle.LoadAsset<AudioClip>("FloorOpen.wav");
            floorClose = Bundle.LoadAsset<AudioClip>("FloorClosed.wav");

            footstep1 = Bundle.LoadAsset<AudioClip>("Footstep1.ogg");
            footstep2 = Bundle.LoadAsset<AudioClip>("Footstep2.ogg");
            footstep3 = Bundle.LoadAsset<AudioClip>("Footstep3.ogg");
            footstep4 = Bundle.LoadAsset<AudioClip>("Footstep4.ogg");
            dogEatItem = Bundle.LoadAsset<AudioClip>("DogEatObject.ogg");
            bigGrowl = Bundle.LoadAsset<AudioClip>("BigGrowl.ogg");
            enragedScream = Bundle.LoadAsset<AudioClip>("DogRage.wav");
            dogSprint = Bundle.LoadAsset<AudioClip>("DogSprint.ogg");
            ripPlayerApart = Bundle.LoadAsset<AudioClip>("RipPlayerApart.ogg");
            cry1 = Bundle.LoadAsset<AudioClip>("Cry1.wav");
            dogHowl = Bundle.LoadAsset<AudioClip>("DogHowl.wav");

            stanleyVoiceline1 = Bundle.LoadAsset<AudioClip>("stanley.wav");
            
            shrimpTerminalNode = Bundle.LoadAsset<TerminalNode>("ShrimpFile.asset");
            shrimpTerminalKeyword = Bundle.LoadAsset<TerminalKeyword>("shrimpTK.asset");

            if (!setKorean)
            {
                shrimpTerminalNode.displayText = "Shrimp\r\n\r\nSigurd’s Danger Level: 60%\r\n\r\n\nScientific name: Lacerta-Srimpian\r\n\r\nShrimps are dog-like creatures, known to be the first tenant of the Upturned Inn. For the most part, he is relatively friendly to humans, following them around, curiously stalking them. Unfortunately, their passive temperament comes with a dangerously vicious hunger.\r\nDue to the nature of their biology, he has a much more unique stomach organ than most other creatures. The stomach lining is flexible, yet hardy, allowing a Shrimp to digest and absorb the nutrients from anything, biological or not, so long as it isn’t too large.\r\n\r\nHowever, this evolutionary adaptation was most likely a result of their naturally rapid metabolism. He uses nutrients so quickly that he needs to eat multiple meals a day to survive. The time between these meals are inconsistent, as the rate of caloric consumption is variable. This can range from hours to even minutes and causes the shrimp to behave monstrously if he has not eaten for a while.\r\n\r\nKnown to live in abandoned buildings, shrimp can often be seen in large abandoned factories or offices scavenging for scrap metal, to eat. That isn’t to say he can’t be found elsewhere. He is usually a lone hunters and expert trackers out of necessity.\r\n\r\nSigurd’s Note: If you see this creepy guy, you’ll want to avoid him spotting you. If you do, he’ll keep following you. Their appetite is no joke. You can HEAR their bellies growl from a meter away. If you do happen to hear that, you better hope you have something on you to feed them, or else you’ll become their next meal. It doesn’t matter if the only item you have on you is sentimental, either you drop it and let the creature eat it or you die.\r\nI swear… it’s almost like it’s staring into your soul. I never want to see one of these guys behind me again.\r\n\r\n\r\nIK: <i>Sir, don't be sad! Shrimp didn't hate you.\r\nhe was just... hungry.</i>\r\n\r\n";
                shrimpTerminalNode.creatureName = "Shrimp";
                shrimpTerminalKeyword.word = "shrimp";
            }
            else
            {
                shrimpTerminalNode.displayText = "쉬림프\r\n\r\n시구르드의 위험 수준: 60%\r\n\r\n\n학명: 라세르타-쉬림피안\r\n\r\n쉬림프는 개를 닮은 생명체로 Upturned Inn의 첫 번째 세입자로 알려져 있습니다. 평소에는 상대적으로 우호적이며, 호기심을 가지고 인간을 따라다닙니다. 불행하게도 그는 위험할 정도로 굉장한 식욕을 가지고 있습니다.\r\n생물학적 특성으로 인해, 그는 대부분의 다른 생물보다 훨씬 더 독특한 위장 기관을 가지고 있습니다. 위 내막은 유연하면서도 견고하기 때문에 어떤 물체라도 영양분을 소화하고 흡수할 수 있습니다.\r\n그러나 이러한 진화적 적응은 자연적으로 빠른 신진대사의 결과일 가능성이 높습니다. 그는 영양분을 너무 빨리 사용하기 때문에 생존하려면 하루에 여러 끼를 먹어야 합니다.\r\n칼로리 소비율이 다양하기 때문에 식사 사이의 시간이 일정하지 않습니다. 이는 몇 시간에서 몇 분까지 지속될 수 있으며, 쉬림프가 한동안 먹지 않으면 마치 괴물이 되어 따라다니던 사람을 쫒게 됩니다.\r\n\r\n버려진 건물에 사는 것으로 알려진 쉬림프는 버려진 공장이나 사무실에서 폐철물을 찾아다니는 것으로 발견할 수 있습니다. 그렇다고 다른 곳에서 그를 찾을 수 없다는 말은 아닙니다. 그는 일반적으로 고독한 사냥꾼이며, 때로는 전문적인 추적자가 되기도 합니다.\r\n\r\n시구르드의 노트: 이 무서운 녀석을 발견하면 도망치려 할 테지만, 얘도 계속 따라다닐 겁니다. 이 녀석은 식욕이 장난 아닙니다. 1미터 멀리에서도 꼬르륵거리는 소리가 들릴 정도거든요. 만약 녀석이 으르렁거리는 소리를 듣게 된다면, 먹이를 줄 수 있는 무언가를 가지고 있기를 바라세요. 아니면 당신이 이 녀석의 식사가 될 거예요.\r\n맹세컨대... 마치 당신의 영혼을 들여다보는 것 같아요. 다시는 내 뒤에서 이런 놈들을 보고 싶지 않아요.\r\n\r\n\r\nIK: <i>손님, 슬퍼하지 마세요! 쉬림프는 당신을 싫어하지 않는답니다.\r\n걔는 그냥... 배고플 뿐이에요.</i>\r\n\r\n";
                shrimpTerminalNode.creatureName = "쉬림프";
                shrimpTerminalKeyword.word = "쉬림프";
            }

            
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 10, Levels.LevelTypes.ExperimentationLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 10, Levels.LevelTypes.AssuranceLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 14, Levels.LevelTypes.VowLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 11, Levels.LevelTypes.MarchLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 8, Levels.LevelTypes.OffenseLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 25, Levels.LevelTypes.RendLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 30, Levels.LevelTypes.DineLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 40, Levels.LevelTypes.TitanLevel, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);
            
            //LethalLib.Modules.Enemies.RegisterEnemy(shrimpEnemy, 0, Levels.LevelTypes.All, Enemies.SpawnType.Default, shrimpTerminalNode, shrimpTerminalKeyword);

            //shrimpEnemy.MaxCount = CustomConfig.MaxPerLevel;
            base.Logger.LogInfo("[LC_Office] Successfully loaded assets!");

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(TerminalPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));

            //DungeonDef dungeonRef = new DungeonDef();
            //dungeonRef.rarity = 1000000;
            //dungeonRef.dungeonFlow = OfficeDungeonFlow;
            //LethalLib.Modules.TerminalUtils.CreateTerminalKeyword("shrimp", false, null, shrimpTerminalNode, null, false);
            //LethalLib.Modules.
            //LethalLib.Modules.Dungeon.AddDungeon(dungeonRef, Levels.LevelTypes.All);

        }
    }
}
