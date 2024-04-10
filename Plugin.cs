using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using LCKorean.Patches;
using UnityEngine;
using System.IO;
using TMPro;
using BepInEx.Configuration;
using System.Reflection;

namespace LCKorean
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "Piggy.LCKorean";
        private const string modName = "LCKorean";
        private const string modVersion = "1.0.2";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        public static ManualLogSource mls;
        public static AssetBundle Bundle;

        public static TMP_FontAsset font3270_HUDIngame;
        public static TMP_FontAsset font3270_HUDIngame_Variant;
        public static TMP_FontAsset font3270_HUDIngameB;
        public static TMP_FontAsset font3270_Regular_SDF;
        public static TMP_FontAsset font3270_b;
        public static TMP_FontAsset font3270_DialogueText;
        public static TMP_FontAsset fontEdunline;

        public static bool translatePlanet;
        public static bool patchFont;
        public static bool thumperTranslation;

        public static string PluginDirectory;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            Plugin.PluginDirectory = base.Info.Location;

            LoadAssets();

            patchFont = (bool)base.Config.Bind<bool>("폰트", "폰트 변경", true, "기본값은 true입니다.\nFontPatcher 등 외부 폰트 모드를 사용하려면 이 값을 true로 설정하세요. 기타 폰트 모드를 설치하지 않은 상태에서 이 값을 false로 설정한다면 게임 내 폰트가 깨질 것입니다.").Value;
            translatePlanet = (bool)base.Config.Bind<bool>("번역", "행성 내부 이름 번역", false, "기본값은 false입니다.\n코드에서 사용되는 행성의 내부 이름을 한글화합니다. 게임 플레이에서 달라지는 부분은 없지만, true로 하면 모드 인테리어의 구성을 변경할 때 행성 명을 한글로 입력해야 합니다. false로 두면 그대로 영어로 입력하시면 됩니다.").Value;
            thumperTranslation = (bool)base.Config.Bind<bool>("번역", "Thumper 번역", false, "기본값은 false입니다.\ntrue로 설정하면 \"Thumper\"를 썸퍼로 번역합니다. false로 설정하면 덤퍼로 설정됩니다.").Value;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("LC Korean is loaded");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
        private void LoadAssets()
        {
            try
            {
                Plugin.Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Plugin.PluginDirectory), "lckorean"));
            }
            catch (Exception ex)
            {
                Plugin.mls.LogError("Couldn't load asset bundle: " + ex.Message);
                return;
            }
            try
            {
                font3270_HUDIngame = Plugin.Bundle.LoadAsset<TMP_FontAsset>("3270-HUDIngame.asset");
                font3270_HUDIngame_Variant = Plugin.Bundle.LoadAsset<TMP_FontAsset>("3270-HUDIngame - Variant.asset");
                font3270_HUDIngameB = Plugin.Bundle.LoadAsset<TMP_FontAsset>("3270-HUDIngameB.asset");
                font3270_Regular_SDF = Plugin.Bundle.LoadAsset<TMP_FontAsset>("3270-Regular SDF.asset");
                font3270_b = Plugin.Bundle.LoadAsset<TMP_FontAsset>("b.asset");
                font3270_DialogueText = Plugin.Bundle.LoadAsset<TMP_FontAsset>("DialogueText.asset");

                fontEdunline = Plugin.Bundle.LoadAsset<TMP_FontAsset>("edunline SDF.asset");
                base.Logger.LogInfo("Successfully loaded assets!");
            }
            catch (Exception ex2)
            {
                base.Logger.LogError("Couldn't load assets: " + ex2.Message);
            }
        }
    }
}
