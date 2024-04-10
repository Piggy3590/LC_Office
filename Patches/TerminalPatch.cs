using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using System.Reflection;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix(ref TerminalNodesList ___terminalNodes, ref List<TerminalNode> ___enemyFiles)
        {
            TranslateKeyword(___terminalNodes, ___enemyFiles);
            TranslateNode(___terminalNodes);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(ref TMP_InputField ___screenText, ref string ___currentText, ref int ___numberOfItemsInDropship)
        {
            ___screenText.text = ___screenText.text.Replace("mild weather", "맑음");
            ___currentText = ___currentText.Replace("mild weather", "맑음");

            ___screenText.text = ___screenText.text.Replace("Rainy", "우천");
            ___screenText.text = ___screenText.text.Replace("rainy", "우천");
            ___currentText = ___currentText.Replace("Rainy", "우천");
            ___currentText = ___currentText.Replace("rainy", "우천");

            ___screenText.text = ___screenText.text.Replace("Foggy", "안개");
            ___screenText.text = ___screenText.text.Replace("foggy", "안개");
            ___currentText = ___currentText.Replace("Foggy", "안개");
            ___currentText = ___currentText.Replace("foggy", "안개");

            ___screenText.text = ___screenText.text.Replace("Flooded", "홍수");
            ___screenText.text = ___screenText.text.Replace("flooded", "홍수");
            ___currentText = ___currentText.Replace("Flooded", "홍수");
            ___currentText = ___currentText.Replace("flooded", "홍수");

            ___screenText.text = ___screenText.text.Replace("Stormy", "뇌우");
            ___screenText.text = ___screenText.text.Replace("stormy", "뇌우");
            ___currentText = ___currentText.Replace("Stormy", "뇌우");
            ___currentText = ___currentText.Replace("stormy", "뇌우");

            ___screenText.text = ___screenText.text.Replace("Eclipsed", "일식");
            ___screenText.text = ___screenText.text.Replace("eclipsed", "일식");
            ___currentText = ___currentText.Replace("Eclipsed", "일식");
            ___currentText = ___currentText.Replace("eclipsed", "일식");

            ___screenText.text = ___screenText.text.Replace("OFF!", "세일!");
            ___currentText = ___currentText.Replace("OFF!", "세일!");

            ___screenText.text = ___screenText.text.Replace("(NEW)", "(신규)");
            ___currentText = ___currentText.Replace("(NEW)", "(신규)");

            ___screenText.text = ___screenText.text.Replace("Price:", "가격:");
            ___currentText = ___currentText.Replace("Price:", "가격:");

            ___screenText.text = ___screenText.text.Replace("[No items available]", "[사용 가능한 아이템이 없음]");
            ___currentText = ___currentText.Replace("[No items available]", "[사용 가능한 아이템이 없음]");

            ___screenText.text = ___screenText.text.Replace("[No items stored. While moving an object with B, press X to store it.]", "[보관된 아이템이 없습니다. B를 사용하여 개체를 이동하는 동안 X를 눌러 보관합니다.]");
            ___currentText = ___currentText.Replace("[No items stored. While moving an object with B, press X to store it.]", "[보관된 아이템이 없습니다. B를 사용하여 개체를 이동하는 동안 X를 눌러 보관합니다.]");

            ___screenText.text = ___screenText.text.Replace("[ALL DATA HAS BEEN CORRUPTED OR OVERWRITTEN]", "[모든 데이터가 손상되거나 덮어쓰기되었습니다]");
            ___currentText = ___currentText.Replace("[ALL DATA HAS BEEN CORRUPTED OR OVERWRITTEN]", "[모든 데이터가 손상되거나 덮어쓰기되었습니다]");
            

            if (___screenText.text.Contains("numberOfItemsOnRoute2"))
            {
                if (___numberOfItemsInDropship != 0)
                {
                    ___screenText.text = ___screenText.text.Replace("[numberOfItemsOnRoute2]", "항로에서 " + ___numberOfItemsInDropship + "개의 아이템을 구매했습니다.");
                    ___currentText = ___currentText.Replace("[numberOfItemsOnRoute2]", "항로에서 " + ___numberOfItemsInDropship + "개의 아이템을 구매했습니다.");
                }else
                {
                    ___screenText.text = ___screenText.text.Replace("[numberOfItemsOnRoute2]", "");
                    ___currentText = ___currentText.Replace("[numberOfItemsOnRoute2]", "");
                }
            }

            if (___screenText.text.Contains("objects outside the ship, totalling at an approximate value"))
            {
                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 91);
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                GrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
                for (int num5 = 0; num5 < array.Length; num5++)
                {
                    if (array[num5].itemProperties.isScrap && !array[num5].isInShipRoom && !array[num5].isInElevator)
                    {
                        num4 += array[num5].itemProperties.maxValue - array[num5].itemProperties.minValue;
                        num3 += Mathf.Clamp(random.Next(array[num5].itemProperties.minValue, array[num5].itemProperties.maxValue), array[num5].scrapValue - 6 * num5, array[num5].scrapValue + 9 * num5);
                        num2++;
                    }
                }
                ___screenText.text = $"함선 외부에 {num2}개의 물체가 있으며, 총 ${num3}의 가치를 가지고 있습니다.";
                ___currentText = $"함선 외부에 {num2}개의 물체가 있으며, 총 ${num3}의 가치를 가지고 있습니다.";
            }
        }

        static void TranslateTerminal(TMP_InputField screenText, string currentText, string oldValue, string newVaule)
        {
            screenText.text = screenText.text.Replace(oldValue, newVaule);
            currentText = currentText.Replace(oldValue, newVaule);
        }

        private static string NewTextPostProcess(Terminal __instance, string modifiedDisplayText, TerminalNode node)
        {
            int num = modifiedDisplayText.Split(new string[] { "[planetTime]" }, StringSplitOptions.None).Length - 1;
            if (num > 0)
            {
                Regex regex = new Regex(Regex.Escape("[planetTime]"));
                int num2 = 0;
                while (num2 < num && __instance.moonsCatalogueList.Length > num2)
                {
                    Debug.Log(string.Format("isDemo:{0} ; {1}", GameNetworkManager.Instance.isDemo, __instance.moonsCatalogueList[num2].lockedForDemo));
                    string text;
                    if (GameNetworkManager.Instance.isDemo && __instance.moonsCatalogueList[num2].lockedForDemo)
                    {
                        text = "(잠김)";
                    }
                    else if (__instance.moonsCatalogueList[num2].currentWeather == LevelWeatherType.None)
                    {
                        text = "";
                    }
                    else
                    {
                        Plugin.mls.LogInfo(__instance.moonsCatalogueList[num2].currentWeather.ToString());
                        if (__instance.moonsCatalogueList[num2].currentWeather.ToString().Contains("Rainy"))
                        {
                            text = "(우천)";
                        }else if (__instance.moonsCatalogueList[num2].currentWeather.ToString().Contains("Stormy"))
                        {
                            text = "(뇌우)";
                        }
                        else if (__instance.moonsCatalogueList[num2].currentWeather.ToString().Contains("Foggy"))
                        {
                            text = "(안개)";
                        }
                        else if (__instance.moonsCatalogueList[num2].currentWeather.ToString().Contains("Flooded"))
                        {
                            text = "(홍수)";
                        }
                        else if (__instance.moonsCatalogueList[num2].currentWeather.ToString().Contains("Eclipsed"))
                        {
                            text = "(일식)";
                        }
                        else
                        {
                            text = "";
                        }
                    }
                    modifiedDisplayText = regex.Replace(modifiedDisplayText, text, 1);
                    num2++;
                }
            }
            try
            {
                if (node.displayPlanetInfo != -1)
                {
                    string text;
                    if (StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather == LevelWeatherType.None)
                    {
                        text = "맑음";
                    }
                    else
                    {
                        if (StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather.ToString().ToLower().Contains("Rainy"))
                        {
                            text = "(우천)";
                        }
                        else if (StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather.ToString().ToLower().Contains("Stormy"))
                        {
                            text = "(뇌우)";
                        }
                        else if (StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather.ToString().ToLower().Contains("Foggy"))
                        {
                            text = "(안개)";
                        }
                        else if (StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather.ToString().ToLower().Contains("Flooded"))
                        {
                            text = "(홍수)";
                        }
                        else if (StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather.ToString().ToLower().Contains("Eclipsed"))
                        {
                            text = "(일식)";
                        }
                        else
                        {
                            text = "";
                        }
                    }
                    modifiedDisplayText = modifiedDisplayText.Replace("[currentPlanetTime]", text);
                }
            }
            catch
            {
                Debug.Log(string.Format("Exception occured on terminal while setting node planet info; current node displayPlanetInfo:{0}", node.displayPlanetInfo));
            }
            if (modifiedDisplayText.Contains("[currentScannedEnemiesList]"))
            {
                if (__instance.scannedEnemyIDs == null || __instance.scannedEnemyIDs.Count <= 0)
                {
                    modifiedDisplayText = modifiedDisplayText.Replace("[currentScannedEnemiesList]", "생명체에 대한 데이터가 수집되지 않습니다. 스캔이 필요합니다.");
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < __instance.scannedEnemyIDs.Count; i++)
                    {
                        Debug.Log(string.Format("scanID # {0}: {1}; {2}", i, __instance.scannedEnemyIDs[i], __instance.enemyFiles[__instance.scannedEnemyIDs[i]].creatureName));
                        Debug.Log(string.Format("scanID # {0}: {1}", i, __instance.scannedEnemyIDs[i]));
                        stringBuilder.Append("\n" + __instance.enemyFiles[__instance.scannedEnemyIDs[i]].creatureName);
                        if (__instance.newlyScannedEnemyIDs.Contains(__instance.scannedEnemyIDs[i]))
                        {
                            stringBuilder.Append(" (신규)");
                        }
                    }
                    modifiedDisplayText = modifiedDisplayText.Replace("[currentScannedEnemiesList]", stringBuilder.ToString());
                }
            }
            if (modifiedDisplayText.Contains("[buyableItemsList]"))
            {
                if (__instance.buyableItemsList == null || __instance.buyableItemsList.Length == 0)
                {
                    modifiedDisplayText = modifiedDisplayText.Replace("[buyableItemsList]", "[재고가 없습니다!]");
                }
                else
                {
                    StringBuilder stringBuilder2 = new StringBuilder();
                    for (int j = 0; j < __instance.buyableItemsList.Length; j++)
                    {
                        if (GameNetworkManager.Instance.isDemo && __instance.buyableItemsList[j].lockedInDemo)
                        {
                            stringBuilder2.Append("\n* " + __instance.buyableItemsList[j].itemName + " (잠김)");
                        }
                        else
                        {
                            stringBuilder2.Append("\n* " + __instance.buyableItemsList[j].itemName + "  //  가격: $" + ((float)__instance.buyableItemsList[j].creditsWorth * ((float)__instance.itemSalesPercentages[j] / 100f)).ToString());
                        }
                        if (__instance.itemSalesPercentages[j] != 100)
                        {
                            stringBuilder2.Append(string.Format("   ({0}% 세일!)", 100 - __instance.itemSalesPercentages[j]));
                        }
                    }
                    modifiedDisplayText = modifiedDisplayText.Replace("[buyableItemsList]", stringBuilder2.ToString());
                }
            }
            if (modifiedDisplayText.Contains("[currentUnlockedLogsList]"))
            {
                if (__instance.unlockedStoryLogs == null || __instance.unlockedStoryLogs.Count <= 0)
                {
                    modifiedDisplayText = modifiedDisplayText.Replace("[currentUnlockedLogsList]", "[모든 데이터가 손상되거나 덮어쓰기됨]");
                }
                else
                {
                    StringBuilder stringBuilder3 = new StringBuilder();
                    for (int k = 0; k < __instance.unlockedStoryLogs.Count; k++)
                    {
                        stringBuilder3.Append("\n" + __instance.logEntryFiles[__instance.unlockedStoryLogs[k]].creatureName);
                        if (__instance.newlyUnlockedStoryLogs.Contains(__instance.unlockedStoryLogs[k]))
                        {
                            stringBuilder3.Append(" (신규)");
                        }
                    }
                    modifiedDisplayText = modifiedDisplayText.Replace("[currentUnlockedLogsList]", stringBuilder3.ToString());
                }
            }
            if (modifiedDisplayText.Contains("[unlockablesSelectionList]"))
            {
                if (__instance.ShipDecorSelection == null || __instance.ShipDecorSelection.Count <= 0)
                {
                    modifiedDisplayText = modifiedDisplayText.Replace("[unlockablesSelectionList]", "[사용 가능한 아이템이 없음]");
                }
                else
                {
                    StringBuilder stringBuilder4 = new StringBuilder();
                    for (int l = 0; l < __instance.ShipDecorSelection.Count; l++)
                    {
                        stringBuilder4.Append(string.Format("\n{0}  //  ${1}", __instance.ShipDecorSelection[l].creatureName, __instance.ShipDecorSelection[l].itemCost));
                    }
                    modifiedDisplayText = modifiedDisplayText.Replace("[unlockablesSelectionList]", stringBuilder4.ToString());
                }
            }
            if (modifiedDisplayText.Contains("[storedUnlockablesList]"))
            {
                StringBuilder stringBuilder5 = new StringBuilder();
                bool flag = false;
                for (int m = 0; m < StartOfRound.Instance.unlockablesList.unlockables.Count; m++)
                {
                    if (StartOfRound.Instance.unlockablesList.unlockables[m].inStorage)
                    {
                        flag = true;
                        stringBuilder5.Append("\n" + StartOfRound.Instance.unlockablesList.unlockables[m].unlockableName);
                    }
                }
                if (!flag)
                {
                    modifiedDisplayText = modifiedDisplayText.Replace("[storedUnlockablesList]", "[보관된 아이템이 없습니다. B로 개체를 이동하는 동안 X를 눌러 보관합니다.]");
                }
                else
                {
                    modifiedDisplayText = modifiedDisplayText.Replace("[storedUnlockablesList]", stringBuilder5.ToString());
                }
            }
            if (modifiedDisplayText.Contains("[scanForItems]"))
            {
                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 91);
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
                GrabbableObject[] array = GameObject.FindObjectsOfType<GrabbableObject>();
                for (int n = 0; n < array.Length; n++)
                {
                    if (array[n].itemProperties.isScrap && !array[n].isInShipRoom && !array[n].isInElevator)
                    {
                        num5 += array[n].itemProperties.maxValue - array[n].itemProperties.minValue;
                        num4 += Mathf.Clamp(random.Next(array[n].itemProperties.minValue, array[n].itemProperties.maxValue), array[n].scrapValue - 6 * n, array[n].scrapValue + 9 * n);
                        num3++;
                    }
                }
                modifiedDisplayText = modifiedDisplayText.Replace("[scanForItems]", string.Format("There are {0} objects outside the ship, totalling at an approximate value of ${1}.", num3, num4));
            }
            if (__instance.numberOfItemsInDropship <= 0)
            {
                modifiedDisplayText = modifiedDisplayText.Replace("[numberOfItemsOnRoute]", "");
            }
            else
            {
                modifiedDisplayText = modifiedDisplayText.Replace("[numberOfItemsOnRoute]", string.Format("{0} purchased items on route.", __instance.numberOfItemsInDropship));
            }
            modifiedDisplayText = modifiedDisplayText.Replace("[currentDay]", DateTime.Now.DayOfWeek.ToString());
            modifiedDisplayText = modifiedDisplayText.Replace("[variableAmount]", __instance.playerDefinedAmount.ToString());
            modifiedDisplayText = modifiedDisplayText.Replace("[playerCredits]", "$" + __instance.groupCredits.ToString());
            FieldInfo fieldInfo = typeof(Terminal).GetField("totalCostOfItems", BindingFlags.NonPublic | BindingFlags.Instance);
            modifiedDisplayText = modifiedDisplayText.Replace("[totalCost]", "$" + fieldInfo.GetValue(__instance).ToString());
            modifiedDisplayText = modifiedDisplayText.Replace("[companyBuyingPercent]", string.Format("{0}%", Mathf.RoundToInt(StartOfRound.Instance.companyBuyingRate * 100f)));
            if (__instance.displayingPersistentImage)
            {
                modifiedDisplayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\nn\n\n\n\n\n\n" + modifiedDisplayText;
            }
            return modifiedDisplayText;
        }

        static void TranslateNode(TerminalNodesList terminalNodes)
        {
            foreach (TerminalNode node in terminalNodes.specialNodes)
            {
                switch (node.name)
                {
                    case "Start":
                        node.displayText = "FORTUNE-9 OS에 오신 것을 환영합니다\n\t회사 제공\n\n행복한 [currentDay] 되세요.\n\n명령 목록을 보려면 \"Help\"를 입력하세요.\n\n\n\n\n";
                        break;
                    case "StartFirstTime":
                        node.displayText.Replace("Welcome to the FORTUNE-9 OS", "FORTUNE-9 OS에 오신 것을 환영합니다");
                        node.displayText.Replace("Courtesy of the Company", "회사 제공");
                        node.displayText.Replace("Type \"Help\" for a list of commands.", "명령 목록을 보려면 \"Help\"를 입력하세요.");
                        break;
                    case "ParserError1":
                        node.displayText = "[이 단어에 제공되는 작업이 없습니다.]\n\n";
                        break;
                    case "ParserError2":
                        node.displayText = "[이 작업과 함께 제공된 개체가 없거나, 단어가 잘못 입력되었거나 존재하지 않습니다.]\n\n";
                        break;
                    case "ParserError3":
                        node.displayText = "[이 작업은 이 개체와 맞지 않습니다.]\n\n";
                        break;
                }

                if (node.displayText.Contains("[DATA CORRUPTED OR OVERWRITTEN]"))
                {
                    node.displayText = node.displayText.Replace("[DATA CORRUPTED OR OVERWRITTEN]", "[데이터가 손상되거나 덮어쓰기됨]");
                }

                switch (node.displayText)
                {
                    case "BG IG, A System-Act Ally\nCopyright (C) 2084-2108, Halden Electronics Inc.\nCourtesy of the Company.\n\n\n\nBios for FORTUNE-9 87.7/10MHZ SYSTEM\n\nCurrent date is Tue  3-7-2532\nCurrent time is 8:03:32.15\n\nPlease enter favorite animal: ":
                        node.displayText = "BG IG, 시스템 행동 연합\nCopyright (C) 2084-2108, Halden Electronics Inc.\n회사 제공.\n\n\n\nFORTUNE-9 전용 바이오스 87.7/10MHZ 시스템\n\n현재 날짜는 2532년 3월 7일 화요일입니다\n현재 시간은 8:03:32.15입니다.\n\n좋아하는 동물을 입력하세요: ";
                        break;
                    case "You could not afford these items!\nYour balance is [playerCredits]. Total cost of these items is [totalCost].\n\n":
                        node.displayText = "자금이 충분하지 않습니다!\n당신의 소지금은 [playerCredits]이지만 이 아이템의 총 가격은 [totalCost]입니다.\n\n";
                        break;
                    case "Unable to route the ship currently. It must be in orbit around a moon to route the autopilot.\nUse the main lever at the front desk to enter orbit.\n\n\n\n":
                        node.displayText = "함선의 항로를 지정할 수 없습니다. 항로를 지정하려면 이륙한 상태여야 합니다.\n궤도에 들어가려면 프런트 데스크에 있는 메인 레버를 사용하세요.\n\n\n\n";
                        break;
                    case "The delivery vehicle cannot hold more than 12 items\nat a time. Please pick up your items when they land!\n\n\n":
                        node.displayText = "수송선은 최대 12개의 아이템만을 적재할 수 있습니다\n착륙하면 아이템을 회수해주세요!\n\n\n";
                        break;
                    case "An error occured! Try again.\n\n":
                        node.displayText = "오류가 발생했습니다! 다시 시도하세요.\n\n";
                        break;
                    case "No data has been collected on this creature. \nA scan is required.\n\n":
                        node.displayText = "이 생명체에 대해 수집된 데이터가 없습니다. \n스캔이 필요합니다.\n\n";
                        break;
                    case "To purchase decorations, the ship cannot be landed.\n\n":
                        node.displayText = "가구를 구매하려면 함선이 완전히 이착륙할 때까지 기다리세요.\n\n";
                        break;
                    case "The autopilot ship is already orbiting this moon!":
                        node.displayText = "이미 이 위성의 궤도에 있습니다!";
                        break;
                    case "This has already been unlocked for your ship!":
                        node.displayText = "이미 잠금이 해제된 아이템입니다!";
                        break;
                    case "The ship cannot be leaving or landing!\n\n":
                        node.displayText = "함선이 완전히 이착륙할 때까지 기다리세요!\n\n";
                        break;
                    case "This item is not in stock!\n\n":
                        node.displayText = "이 아이템은 재고가 없습니다!\n\n";
                        break;
                    case "Returned the item from storage!":
                        node.displayText = "아이템을 저장고에서 꺼냈습니다!";
                        break;
                    case "Entered broadcast code.\n":
                        node.displayText = "송출 코드를 입력했습니다.\n";
                        break;
                    case "Switched radar to player.\n\n":
                        node.displayText = "레이더를 플레이어로 전환했습니다.\n\n";
                        break;
                    case "Pinged radar booster.\n\n":
                        node.displayText = "레이더 부스터를 핑했습니다.\n\n";
                        break;
                    case "Sent transmission.\n\n":
                        node.displayText = "전송했습니다.\n\n";
                        break;
                    case "Flashed radar booster.\n\n":
                        node.displayText = "레이더 부스터의 섬광 효과를 사용했습니다.\n\n";
                        break;
                    case "You selected the Challenge Moon save file. You can't route to another moon during the challenge.":
                        node.displayText = "챌린지 위성 저장 파일을 선택했습니다. 챌린지 도중에는 다른 위성으로 이동할 수 없습니다.";
                        break;
                }
            }
        }


        static void TranslateKeyword(TerminalNodesList terminalNodes, List<TerminalNode> ___enemyFiles)
        {
            foreach (TerminalNode node in ___enemyFiles)
            {
                switch (node.name)
                {
                    case "SnarefleaFile":
                        node.displayText = "올무벼룩\n\n시구르드의 위험도: 30%\n\n학명: Dolus-scolopendra \n순각강에 속하는 매우 큰 절지동물입니다. 그들은 몸에서 은폐된 장소로 이동하는 데 사용하는 실크를 만듭니다. 외골격은 다소 약해서 오래 떨어지면 죽을 수 있습니다. 올무 벼룩은 독을 생산하지도 않고 강하게 물지도 못합니다. 대신 큰 먹잇감을 조여 질식시키는 능력으로 이러한 약점을 보완합니다.\n\n올무 벼룩은 어둡고 따뜻한 곳에서 잘 자랍니다. 저온에서는 살아남지 못하며 일반적으로 야외와 햇빛을 피합니다. 밖으로 데리고 나가거나 때려눕히세요!; 걔네 내장으로 좋은 밀크쉐이크를 만들 수 있을 것 같아,,,\n\n";
                        break;
                    case "BrackenFile":
                        node.displayText = "브래컨 -- 일명 플라워 맨!\n\n그건 플라워 맨이었어! 변명하지 마! 난 걔 시체라도 찾고 싶었다고! 겁쟁이 같은 녀석들!\n\n학명: Rapax-Folium\n브래컨의 생물학적 분류군에 대한 논쟁이 있습니다. 브래컨은 붉은 사탕무에 가까운 피부색과 질감을 가진 이족보행형 척추동물입니다. 브래컨(고사리)이라는 이름은 위쪽 척추에서 솟아난 부위가 잎처럼 생겼기 때문에 붙여진 이름입니다. 이 부위는 적을 위협하기 위한 용도로 추정되지만, 브래컨의 개체 수가 적으며 발견하기도 어렵기 때문에 구체적인 행동에 대해서는 알려진 바가 많지 않습니다.\n\n우리는 그것을 접한 생물 전문가들의 이야기를 통해 약간의 정보를 알고 있습니다. 그는 매우 높은 지능을 가진 고독한 사냥꾼입니다. 브래컨의 행동은 냉담해 보일 수 있으며, 이유 없는 행동에도 높은 공격성을 보이나 직접 마주치면 재빨리 도망갑니다. 하지만 브래컨은 궁지에 몰리거나 오랫동안 지켜보면 적대감을 드러내는 것으로 알려져 있습니다. 따라서 브래컨을 주의 깊게 살펴보되 오래 쳐다보지 않는 것이 좋습니다. 생포되거나 죽은 상태의 표본을 채집한 사례는 없습니다. 다른 대형 동물과 달리 시체가 빠르게 분해되는 것으로 알려져 있습니다.\n\n";
                        break;
                    case "ThumperFile":
                        if (Plugin.thumperTranslation)
                        {
                            node.displayText = "썸퍼\n\n시구르드의 위험도: 90%\n\n학명: Pistris-saevus\n해프(Halves) 또는 썸퍼는 연골어강에 속하는 매우 공격적인 육식성 어종입니다. 골격이 연골로 이루어져 있어 신축성있고 고무처럼 부드러운 신체를 가지고 있습니다. Halves라는 이름은 부화한 알의 껍질을 벗어나기 위해 뒷다리를 뜯어먹어야 하기 때문에 붙혀진 이름입니다. 팔, 즉 앞다리는 매우 강해서 먹잇감을 짓밟을 때에도 사용합니다. 직선 지형에서 빠른 속도를 낼 수 있으며, 보통 먹이사슬의 최상위에 있는 끈질긴 사냥꾼입니다.\n\n가장 큰 약점은 지능이 낮고 청각이 전혀 없다는 것입니다. 모퉁이를 돌 때 속도가 느리고 먹이를 쉽게 추적할 수 없기 때문에 덤퍼와 마주치면 시야에서 벗어나는 것이 최선의 생존 수단입니다.\n\n이 종의 빠르고 불안정한 진화 때문에 일부 학자들은 썸퍼가 엉겅퀴 성운 주변 행성에서 종 분화율이 높은 돌연변이의 증가를 보여주는 사례 중 하나라고 이론을 세웠습니다.";
                        }
                        else
                        {
                            node.displayText = "덤퍼\n\n시구르드의 위험도: 90%\n\n학명: Pistris-saevus\n해프(Halves) 또는 덤퍼는 연골어강에 속하는 매우 공격적인 육식성 어종입니다. 골격이 연골로 이루어져 있어 신축성있고 고무처럼 부드러운 신체를 가지고 있습니다. Halves라는 이름은 부화한 알의 껍질을 벗어나기 위해 뒷다리를 뜯어먹어야 하기 때문에 붙혀진 이름입니다. 팔, 즉 앞다리는 매우 강해서 먹잇감을 짓밟을 때에도 사용합니다. 직선 지형에서 빠른 속도를 낼 수 있으며, 보통 먹이사슬의 최상위에 있는 끈질긴 사냥꾼입니다.\n\n가장 큰 약점은 지능이 낮고 청각이 전혀 없다는 것입니다. 모퉁이를 돌 때 속도가 느리고 먹이를 쉽게 추적할 수 없기 때문에 덤퍼와 마주치면 시야에서 벗어나는 것이 최선의 생존 수단입니다.\n\n이 종의 빠르고 불안정한 진화 때문에 일부 학자들은 덤퍼가 엉겅퀴 성운 주변 행성에서 종 분화율이 높은 돌연변이의 증가를 보여주는 사례 중 하나라고 이론을 세웠습니다.";
                        }
                        break;
                    case "EyelessDogFile":
                        node.displayText = "눈없는 개\\n\\n학명: Leo caecus\\nSaeptivus강에 속하는 대형 포유류입니다. 사교적이며 크게 무리지어 사냥합니다. 알아들을 수 있는 소리와 큰 입 때문에 \"숨쉬는 사자\"라고도 불립니다. 눈없는 개는 지구력이 뛰어난 사냥꾼으로 퇴화된 시각을 청각으로 보완하려고 합니다. 종종 동족의 소리를 먹이로 착각해 무리 내에서 싸움에 뛰어든다는 속설이 있습니다.\\n\\n눈 없는 개의 행동은 다른 무리형 생물과 달리 광범위한 거리를 커버하기 위해 멀리 흩어지는 경향이 있습니다. 눈없는 개가 먹이를 발견하면 주변에 있는 다른 개들에게 경고를 보내기 위해 울부짖고, 이 개들도 경보를 울려 일종의 연쇄 반응이 일어나기도 합니다. 눈 없는 개는 무리를 지어 다니면 위험할 수 있습니다. 그러나 그들은 성급하게 판단하는 면도 있어 먹이의 위치를 추적하던 도중, 부정확한 판단으로 먹이를 놓치는 경우도 있습니다.";
                        break;
                    case "HoardingBugFile":
                        node.displayText = "비축 벌레\n\n시구르드의 위험도: 0%\n\n학명: Linepithema-crassus\n비축 벌레(벌목)은 사회성이 강한 대형 곤충입니다. 혼자 생활하는 경우가 많지만, 같은 종의 개체들과 둥지를 공유하기도 합니다. 평균 키는 3피트 정도이며 둥근 모양의 몸체를 가지고 있습니다. 가벼운 체액과 혈액, 외골격 덕에 막으로 된 날개로 비행할 수 있습니다. 또한 몸통도 약간 투명합니다.\n\n비축벌레라는 이름은 영역적인 성향 때문에 붙여진 이름입니다. 일단 한 장소를 둥지로 선택하면 주변의 물건을 이용해 둥지를 장식하고 둥지의 일부처럼 보호합니다. 비축벌레는 큰 둥지에 있을 때만큼 단독으로는 위험하지 않습니다. 그러나 비축벌레는 혼자 남겨두면 상당히 중립적이며 크게 위험하지 않습니다. ㅇ-우리는 멍청한 포옹벌레를 사랑해.!! - ㅇㅣ것은 불굴의 시구르드의 메모다.";
                        break;
                    case "HygrodereFile":
                        node.displayText = "하이그로디어\n\n시구르드의 위험도: 0%, 만약 너가 달팽이보다 빠르기만 하다면야!\n\n학명: Hygrodere\n원생동물 문에 속하는 진핵 생물로, 수백만 마리가 번식합니다. 이 작은 유기체는 놀라운 번식 속도로 수백만 마리까지 증식할 수 있습니다. 하이그로디어는 분열하는 경우가 거의 없으며, 대신 크고 끈적끈적한 덩어리를 형성하여 많은 공간을 차지할 수 있고 다루기 위험하므로 대형 도구나 미끼를 사용하여 이동시켜야 합니다.\n\n하이그로디어는 열과 산소에 끌리기 때문에 어디에서나 열과 산소를 감지할 수 있습니다. 자신의 체질로 전환할 수 없는 유기물은 거의 없습니다. 이들을 독살할 수 있는 물질은 아직 발견되지 않았습니다. 끊임없이 스스로를 교체하기 때문에 수십만 년 동안 살아있을 수 있습니다. 하이그로디어는 기어오르는 데 어려움을 겪으니 궁지에 몰리면 높은 물체를 찾아 그 위에 올라서세요, 어쩌다 보니 한 개체와 친구가 되기도 했어, 음악 때문인 것 같아. \n";
                        break;
                    case "ForestKeeperFile":
                        node.displayText = "숲지기\n\n시구르드의 위험도: 50%\n\n학명: Satyrid-proceritas\nRapax-Folium과 공통 분류을 공유한 것으로 알려진 이 거대 괴수들은 주로 서식하는 생물군계에서 이름을 본따 숲지기라고 불립니다. 그들의 몸의 앞뒤에는 눈을 모방한 듯한 무늬가 있는데, 이러한 특징은 민첩하지 않은 숲지기의 특성 때문에 어린 개체들에게 매우 유용합니다. 숲지기의 피부는 독특하고 밀도가 높은 물질로 구성되며 그들의 일생 동안 점점 단단해지며, 몸 전체에 있는 커다란 가시와 돌기는 늙은 개체일수록 길고 많이 형성됩니다.\n\n숲지기는 5~6세의 인간 아이와 비슷하게 호기심이 많습니다. 흥미로워 보이는 것은 무엇이든 집어먹으려 합니다. 그들은 실제로 무언가를 먹을 필요가 없으며, 광합성과 유사한 과정으로 에너지를 얻는 것으로 알려져 있습니다. 그럼에도 불구하고, 이러한 행동 양상 때문에 숲지기를 관찰하는 것은 굉장한 위험이 따릅니다. 먼 거리도 볼 수 있기 때문에 낮은 자세를 유지하고 엄폐물을 활용하는 것이 권장됩니다. 좁은 공간에는 들어가지 못하며 일반적으로 파괴적인 성향이 아니므로 대피소나 돌출부에 가까이 피신하는 것이 좋습니다.";
                        break;
                    case "CoilHeadFile":
                        node.displayText = "코일 헤드\n\n시구르드의 위험도: 80%\n\n학명: Vir colligerus\n비르 콜리게루스, 또는 구어체로 코일-헤드라고 명명된 이 생물은 위험하고 예측불허의 특성으로 인해 알려진 것이 많지 않습니다. 해부되거나 무력화될 때에는 연소되는 것으로 알려져 있으며, 위험할 정도로 높은 수준의 방사성 입자를 운반합니다. 여러 이유로 인해, 입증되지는 않았지만 생체 병기로 만들어졌을 가능성이 높습니다.\n\n코일-헤드의 시각적 외형은 스프링으로 머리가 연결된 피투성이 마네킹 같은 외형을 가지고 있습니다. 코일-헤드의 주된 행동 양상은 누군가 쳐다보면 정지하는 것입니다. 그러나 이것은 엄격한 규칙은 아닌 것 같습니다. 또한 시끄럽거나 밝은 빛에 노출될 경우, 긴 리셋 모드에 들어가는 것으로 보이기도 합니다. \n그냥 계속 쳐다보거나 기절 수류탄을 사용하도록! - 시구르드\n\n";
                        break;
                    case "LassoManFile":
                        node.displayText = "올가미 인간\n\n시구르드의 위험도: 30%이긴 한데 좀 무섭네\n\n학명: \n\n\n";
                        break;
                    case "EarthLeviathanFile":
                        node.displayText = "육지 레비아탄\n\n시구르드의 위험도: 2% 왜냐면 함선 카메라에서 숨을 수 없거든!!!\n\n학명: Hemibdella-gigantis\n피시콜리데아과에 속하는 경건한 이름의 육지 레비아탄은 엉겅퀴 성운 주변에서 발견되는 가장 거대한 무척추동물 중 하나입니다. 아직 포획된 개체는 전무하기 때문에 이들의 생태에 대해 알려진 바는 많지 않습니다. \n\n그들은 포식자처럼 행동하는 것으로 보입니다. 육지 레비아탄이 남긴 엄청난 땅굴의 규모로 보아 지하 40m까지 파고들 수 있는 것으로 추측됩니다. 그들은 아주 미세한 진동도 감지할 수 있기 때문에 근처에 있을 때 가만히 있는 것이 좋다고 하지만, 이는 잘못된 상식입니다. 땅을 파는 듯한 소리가 들리면 빠르게 발걸음을 되돌려야 합니다.";
                        break;
                    case "JesterFile":
                        node.displayText = "광대\n\n시구르드의 위험도: 90% 저 녀석이 날뛰기 전에 튀어!! 저 녀석에게서 숨을 곳은 없어. 그냥 밖으로 도망가.\n\n학명: 미친 놈\n망할 과학적 기록 따윈 없어! 행운을 빌어, 너도 우리만큼이나 잘 알잖아. 우린 저걸 광대라고 불러";
                        break;
                    case "PufferFile":
                        node.displayText = "포자 도마뱀\n\n시구르드의 위험도: 잘 모르겠어 아마도 5% 난 단지 이 통통한 다리가 싫을 뿐이야\n\n학명: Lacerta-glomerorum\n구어체로 Puffer 또는 포자 도마뱀이라고 불리는 (악어과에 속한) Lacerta-glomerorum은 가장 거대하고 무거운 파충류 중 하나입니다. 큰 입을 갖고 있지만 초식성이며 강하게 물지 않습니다. 꼬리에 있는 구근은 곰팡이 종인 Lycoperdon perlatum의 성장을 유인하고 촉진하는 화학 물질을 분비하는 것으로 알려져 있으며, 이 물질을 흔들어 포자를 방출하는 방어기제를 가지고 있어 독특한 상호 공생 관계의 예시라고 할 수 있습니다.\n\n포자 도마뱀은 매우 소심한 기질을 가지고 있어 싸움을 피하는 경향이 있습니다. 자신의 위협이 효과적이지 않다고 판단되면 공격을 시도할 수 있으므로 포자 도마뱀을 구석에 몰아넣거나 쫓아다니지 않는 것이 좋습니다. 포자 도마뱀은 수백 년 전에 적어도 부분적으로 가축화되었다는 역사적 기록이 있지만, 이러한 노력은 꼬리를 약용으로 채취하려는 계획에 의해 중단되었습니다.";
                        break;
                    case "BunkerSpiderFile":
                        node.displayText = "벙커 거미\n\n시구르드의 위험도: 20%\n\n학명: Theraphosa-ficedula\n벙커 거미는 테라포사 속에 속하는 거미로, 엉겅퀴 성운에서 발견되는 거미류 중 가장 크고 지금까지 발견된 거미류 중 두 번째로 큰 거미입니다. 벙커 거미는 보트가 엉겅퀴 성운을 여행한 후 약 수백 년에 걸쳐 대형 포유류를 잡아먹도록 진화한 것으로 추정됩니다. (참조: 희미해져 가는 성운 주변의 종 다양성 증가에 대한 추측)\n\n벙커 거미는 실크를 생산하여 선택한 둥지 주변에 깔고 먹이가 걸려 넘어지기를 기다립니다. 벙커 거미는 벽이나 먹잇감이 들어올 수 있는 출입구 위에서 기다리는 모습을 볼 수 있습니다. 벙커 거미를 '준비되지 않은' 상태로 발견하면 방어 반응으로 멈출 수 있습니다. 이 경우에는 그냥 내버려 두는 것이 가장 좋습니다. 벙커 거미가 공격적으로 반응하면 일반적인 도구로 싸우지 않는 것이 가장 좋습니다. 거미는 거미줄을 사용하여 느린 움직임을 보완하므로 주변을 잘 살펴보세요. 거미줄은 무딘 도구로도 쉽게 부술 수 있습니다.\n\n벙커 거미는 생태계에 큰 도움이 되지 않으면서도 특히 인간과 도시 탐험가에게 큰 위험을 초래할 수 있습니다. 이에 따라 벙커 거미가 서식하는 여러 주에서 비공식적으로 사살 명령이 합의되었으며, 2497년 10월 6일부로 ITDA의 승인을 받았습니다.";
                        break;
                    case "ManticoilFile":
                        node.displayText = "만티코일\n\n시구르드의 위험도: 0%\n\n학명: Quadrupes-manta\n만티코일은 코비과에 속하는 텃새입니다. 초기 후손에 비해 몸집이 상당히 크고, 날개 길이가 55~64인치에 이릅니다. 가장 뚜렷한 특징은 네 개의 날개를 가지고 있다는 것입니다. 뒷날개는 저속에서 안정적으로 비행하는 데 주로 사용되며, 앞쪽 두 날개가 대부분의 양력을 만들어냅니다. 둥근 몸통은 눈에 띄는 노란색이지만 주 깃털(뒷깃)을 따라 검은색 윤곽선이나 줄무늬가 있습니다.\n\n만티코일은 주로 작은 곤충을 먹지만 작은 설치류도 잡아먹을 수 있습니다. 매우 지능적이고 사교적입니다. 광견병, 루벤클로리아, 피트 바이러스를 전염시킬 수 있지만 인간에게는 위협이 거의 되지 않으며 일반적으로 소극적인 기질을 가지고 있습니다.\n\n";
                        break;
                    case "CircuitBeeFile":
                        node.displayText = "회로 벌\n\n시구르드의 위험도: 90%\n\n학명: Crabro-coruscus\n붉은벌이라고도 알려진 회로 벌은 꿀벌의 후손인 Apis 속의 진사회성 비행곤충입니다. 그들은 수북한 털과 붉은색 몸체와 두 쌍의 날개로 쉽게 알아볼 수 있습니다. 그들의 조상과 마찬가지로, 그들은 지능적인 사회적 꿀벌 행동, 대규모 군체 크기, 꿀을 저장하는 데 사용하는 밀랍 둥지 건설 및 수분에 있어서 중요한 역할로 잘 알려져 있습니다. 나무 등 높은 곳을 선택해 벌집을 만드는 경우가 많은 꿀벌과 달리, 붉은벌은 땅에 벌집을 만듭니다.\n\n붉은벌은 매우 방어적입니다. 여왕벌과 수벌을 제외하고 모든 벌이 벌집에서 벗어나 수 미터 이내에 접근하는 모든 생물을 공격합니다. 이러한 대담한 행동은 정전기라는 벌의 가장 특징적인 특성 덕분에 가능합니다. 벌은 공기와의 마찰을 일으킵니다. 또한 벌집 안에서 두 쌍의 날개를 서로 문지르며 마찰을 일으키고, 벌집 안에서도 서로를 문지르며 마찰을 일으킵니다. 당황하거나 화를 낼 때 더 강한 전기장을 생성하기 때문에 꿀벌에 비해 더 많은 전기장을 생성할 수 있는 이유는 아직 연구 중에 있습니다. 이 능력은 물 주변에서 특히 위험합니다.\n\n\n\n그것과는 거리를 유지하는 것이 좋습니다. 벌집을 도난당하면 붉은벌 떼는 모든 생물을 공격하는 맹공격에 들어갑니다. 이 파괴적인 벌떼의 행동은 벌집을 찾거나 완전히 지칠 때까지 지속되며, 몇 시간에서 며칠이 걸릴 수 있습니다. 작은 설치류, 곤충, 심지어 일부 대형 포유류의 사체 뒤에 벌집을 남기는 것으로 알려져 있으며, 드물게는 화재를 일으키기도 합니다. 이 강력한 꿀벌의 생태계에 대한 이점과 단점에 대해 많은 논쟁이 있습니다. BEEbated! - 불굴의 시구르드";
                        break;
                    case "LocustFile":
                        node.displayText = "배회 메뚜기\n\n시구르드의 위험도: 0%\n\n학명: Anacridium-vega\n\n배회 메뚜기로 알려진 메뚜기의 일종입니다. 뛰어오르거나 날아다니는 경향이 있는 일부 종과 달리, 배회 메뚜기는 땅에 잘 붙어있지 않으며 숫자가 적을 때에도 서로 가까이 붙어 있습니다. 포식자가 방해하면 빠르게 흩어지지만 빛에 매우 끌립니다.\n\n";
                        break;
                    case "BaboonHawkFile":
                        node.displayText = "개코 매\n\n시구르드의 위험도: 75%\n\n학명: Papio-volturius\n개코 매는 긴꼬리원숭이과에 속하는 영장류입니다. 굽은 등을 가지고 있지만 평균적으로 8피트까지 서 있을 수 있습니다. 머리는 뼈로 이루어져 있고, 새처럼 생긴 부리와 긴 뿔을 꼬챙이처럼 사용하여 먹이를 잡아먹습니다. 뿔은 두개골의 나머지 부분처럼 뼈가 아닌 케라틴로 이루어져 있으며 신경이나 혈관이 없습니다. 따라서 개코 매는 강한 힘에 의해 뿔이 부러졌다가 같은 계절에 완전히 다시 자라는 경우가 많습니다. 개코 매의 이름은 부분적으로 그 큰 몸무게를 지탱할 수 없는 큰 날개 때문에 붙여진 이름이며, 대신 위협과 비바람으로부터 보호하는 데 사용됩니다.\n\n지금까지 관찰된 가장 큰 개코 매 무리는 18마리의 개코 매로 구성되었습니다. 그들은 영토를 느슨하게 갖고 있으며, 그들의 행동의 대부분은 위협과 과시에서 비롯됩니다. 그들은 자신의 영역을 표시하기 위해 화려하거나 다채로운 물건을 사용하기도 합니다. 고독한 정찰병인 개코 매는 일반적으로 소심하며 자극받지 않는 한 공격하지 않습니다. 개체 수가 많으면 위험할 수 있으므로 다른 사람과 함께 다녀서 자신이 위험해 보이게 만드는 것이 공격을 예방하는 가장 좋은 방법입니다. 그들은 더 작은 포유류를 선호하지만, 필요할 때에는 자신의 무리와 함께 눈 없는 개와 같이 크기가 두 배나 되는 동물도 공격하기도 합니다. 쟤네가 ㄴㅐ 피클 가져갔어\n\n";
                        break;
                    case "NutcrackerFile":
                        node.displayText = "호두까기 인형\n\n집을 지키는 수문장입니다.\n\n지칠 줄 모르는 하나의 눈으로 움직임을 감지하며, 마지막으로 감지한 생물의 움직임 여부를 기억합니다.";
                        break;
                    case "RadMechFile":
                        node.displayText = "올드 버드\\n\\n시구르드의 위험도: 95%  \\n\\n 올드 버드는 인간형 디자인의 자율적이고 공격적인 전쟁 병기입니다. 높이 19피트, 폭 11피트 크기의 이 로봇의 가장 큰 특징은 머리 부분에 위치한 10만 루멘의 빛을 발하는 스포트라이트와 가슴에 달린 장거리 음향 장치로, 사운드 캐논이라고도 불립니다. 어깨에는 먼 거리까지 소리를 내보내는 데 사용되는 스피커가 추가로 달려 있습니다. 올드 버드의 왼팔은 발톱으로, 오른팔은 로켓 추진 수류탄을 발사하고 근거리에서 매우 뜨거운 불꽃으로 불을 붙일 수 있는 노즐입니다. 올드 버드는 대량 생산된 최초의 궤도 외 병기 중 하나입니다.\\r\\n 올드 버드를 개발했는지에 대한 주제는 2143년 12월 18일, 50여 마리의 올드 버드가 앵글 수도를 침공한 기록이 처음 등장한 이후 격렬한 논쟁의 주제로 사용되었습니다. 이는 앵글렌 제국이 몰락한 주요 원인 중 하나로 꼽힙니다. 일반적으로 받아들여지는 이론은 2100년대 내내 앵글렌과 부에모흐 군대 사이의 긴장을 고려한 것이지만, 이후 수 세기 동안 입증된 것은 없습니다. 올드 버드의 디자인은 금속의 담금질까지 그 기원을 감추기 위한 것으로 보입니다. 그것은 \"걸어 다니는 랜섬 레터\"라고 불립니다. \\r\\n 올드 버드의 다리는 로켓처럼 작동하여 먼 거리를 이동하고 목표물을 효율적으로 찾을 수 있습니다. 하지만 이 기능이 존재하는 가장 큰 이유는 궤도에 진입하고 착륙하는 데 도움이 되기 때문입니다. 올드 버드가 사용하는 재료와 연료는 2130년경의 여객 우주선과 비슷합니다.\\r\\n올드 버드는 목표 행성에 착륙하여 영원히 머물러 있습니다. 한 번의 여행에 충분한 연료를 가지고 있는 경우가 많지만, 프로그램에서 '이주'를 선택할 수 있음을 시사하는 행동은 발견되지 않았습니다. 하지만 수백 년의 휴면기를 거쳐 새로운 행성으로 자율적으로 이동하는 올드 버드에 대한 검증되지 않은 설명이 존재합니다. \\r\\n올드 버드는 역사적으로 영국군이 부여한 암호명인 \"A16-L31\"을 따서 \"Al(알)\"로 불렸습니다. 그러나 2384년, 포스트 펑크 프로젝트 언더 레모라가 발표한 '올드 버드'라는 곡이 컬트 히트곡으로 발표되고, 불과 3년 후 거리 예술가 랜드 아이리가 동명의 유명한 작품에서 오토톤이 행성 사이를 비행하고 착륙하는 모습을 기러기 떼와 비슷한 배열로 묘사하면서 현대 문화에서 가장 상징적이고 영향력 있는 이미지로 여겨지며 현대적인 별명으로 굳혀지게 됩니다.\\r\\n 2356년, 5-엠브리언은 여행이나 정착을 목적으로 보트에 의해 정죄된 것으로 분류되었고, 목격자들에 따르면 올드 버드는 \"지평선을 따라 늘어서 있다\"고 묘사되었습니다. 오늘날에는 작동하지 않는 것처럼 보이지만, 많은 수가 여전히 수면 모드에 있을 가능성이 있으므로 행성은 그 상태를 유지할 가능성이 높습니다. 작은 달은 올드 버드의 비행 및 착륙 능력을 시험하는 장소로 사용되었을 가능성이 높습니다. 장난치지 마 그렇지 않으면 저 녀석들이 널 한번 태워다 줄 거야.ㄱㅒ들은 길도 빨리 잃고 회전도 엄청 빠르게 할 수 없고, 걔들은 바본데다가 닥치지도 않을 거야 미안.";
                        break;
                    case "ButlerFile":
                        node.displayText = "집사\n\n시구르드의 위험도: 70%\n\n이 녀석들에 대한 파일이 없어서 내가 직접 쓰고 있어. 저택에서 계속 이 구역질나는 놈들을 발견하고 있는데, 우리에게 대화도 하지 않고 관심도 주지 않아. 생긴 것도 마치 바람 빠진 풍선처럼 생겼고. 그리고 리처드보다 심한 썩은 고기 냄새가 나고, 몸 안에서 파리가 윙윙거리는 소리가 들리는 것 같아. 제스는 눈구멍에서 뭔가가 기어 나왔다가 다시 들어가는 걸 봤다고 했어. \n\n이 뭐라고 해야 할지 모르겠어\n\n그냥 돌아다니면서 바닥이나 청소하고 다니고 있어. 그래서 저택이 이렇게 깨끗한 거겠지? 적어도 그 녀석들을 자기 할 일만 하잖아. 하지만 내가 보지 않을 때 나를 째려보는 것 같은데\n\n추가: 그 녀석이 칼을 들고 리치를 쫓아왔어. 우린 못 봤지만 걔가 방에 들어가자마자 그 집사가 다시 빗자루를 꺼내 아무 일도 없었다는 듯이 바닥을 청소하고 있었다고 했어. 이제부터 우리 모두 같이 붙어다녀야 해. 이 망할 집사 녀석들! 이제 리치 좀 봐야겠어 아마 슈트에 오줌 지렸을 거야";
                        break;
                    case "MaskHornetsFile":
                        node.displayText = "위장 말벌\r\n\r\n위장 말벌";
                        break;
                }
            }
            foreach (TerminalKeyword keyword in terminalNodes.allKeywords)
            {
                switch (keyword.word)
                {
                    case "buy":
                        foreach (CompatibleNoun noun in keyword.compatibleNouns)
                        {
                            switch (noun.result.name)
                            {
                                case "buyProFlashlight1":
                                    noun.result.displayText = "프로 손전등을 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 프로 손전등을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    noun.result.terminalOptions[1].result.displayText = "주문을 취소했습니다.\n";
                                    break;
                                case "buyFlash":
                                    noun.result.displayText = "손전등을 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 손전등을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyLockpickers":
                                    noun.result.displayText = "자물쇠 따개를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 자물쇠 따개를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyBoombox":
                                    noun.result.displayText = "붐박스를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 붐박스를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyExtensLadder":
                                    noun.result.displayText = "연장형 사다리를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 연장형 사다리를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyJetpack":
                                    noun.result.displayText = "제트팩을 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 제트팩을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyRadarBooster":
                                    noun.result.displayText = "레이더 부스터를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 레이더 부스터를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyShovel":
                                    noun.result.displayText = "철제 삽을 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 철제 삽을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buySpraypaint":
                                    noun.result.displayText = "스프레이 페인트를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 스프레이 페인트를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyStunGrenade":
                                    noun.result.displayText = "기절 수류탄을 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 기절 수류탄을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyTZP":
                                    noun.result.displayText = "TZP-흡입제를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 TZP-흡입제를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyWalkieTalkie":
                                    noun.result.displayText = "무전기를 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 무전기를 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;
                                case "buyZapGun":
                                    noun.result.displayText = "잽건을 주문하려고 합니다. 수량: [variableAmount]. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "[variableAmount]개의 잽건을 주문했습니다. 당신의 현재 소지금은 [playerCredits]입니다.\n\n우리의 계약자는 작업 중에도 빠른 무료 배송 혜택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.\n\n\n";
                                    break;

                                case "CozyLightsBuy1":
                                    noun.result.creatureName = "아늑한 조명";
                                    noun.result.displayText = "아늑한 조명을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "아늑한 조명을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n전등 스위치를 사용해 아늑한 조명을 활성화하세요.\n\n";
                                    break;
                                case "GreenSuitBuy1":
                                    noun.result.creatureName = "초록색 슈트";
                                    noun.result.displayText = "초록색 슈트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "초록색 슈트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n\n";
                                    break;
                                case "HazardSuitBuy1":
                                    noun.result.creatureName = "방호복 슈트";
                                    noun.result.displayText = "방호복 슈트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "방호복 슈트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n\n";
                                    break;
                                case "LoudHornBuy1":
                                    noun.result.creatureName = "시끄러운 경적";
                                    noun.result.displayText = "시끄러운 경적을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "시끄러운 경적을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n코드를 길게 당겨 시끄러운 경적을 활성화합니다.\n";
                                    break;
                                case "PajamaSuitBuy1":
                                    noun.result.creatureName = "파자마 슈트";
                                    noun.result.displayText = "파자마 슈트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "파자마 슈트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n\n";
                                    break;
                                case "PurpleSuitBuy1":
                                    noun.result.creatureName = "보라색 슈트";
                                    noun.result.displayText = "보라색 슈트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "보라색 슈트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n\n";
                                    break;
                                case "RomTableBuy1":
                                    noun.result.creatureName = "로맨틱한 테이블";
                                    noun.result.displayText = "로맨틱한 테이블을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "로맨틱한 테이블을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "ShowerBuy1":
                                    noun.result.creatureName = "샤워 부스";
                                    noun.result.displayText = "샤워 부스를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "샤워 부스를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "TableBuy1":
                                    noun.result.creatureName = "테이블";
                                    noun.result.displayText = "테이블을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "테이블을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "TeleporterBuy1":
                                    noun.result.creatureName = "순간이동기";
                                    noun.result.displayText = "순간이동기를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "순간이동기를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n버튼을 눌러 순간이동기를 활성화합니다. 현재 함선의 레이더에 모니터링 중인 사람을 순간이동시킵니다. 순간이동기를 통해 보유한 아이템은 보관할 수 없습니다. 재충전하는 데 약 10초가 걸립니다.\n\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "TelevisionBuy1":
                                    noun.result.creatureName = "텔레비전";
                                    noun.result.displayText = "텔레비전을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "텔레비전을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "ToiletBuy1":
                                    noun.result.creatureName = "변기";
                                    noun.result.displayText = "변기를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "변기를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "WelcomeMatBuy":
                                    noun.result.creatureName = "웰컴 매트";
                                    noun.result.displayText = "웰컴 매트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "웰컴 매트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "SignalTranslatorBuy":
                                    noun.result.creatureName = "신호 해석기";
                                    noun.result.displayText = "신호 해석기를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "신호 해석기를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n\n\n신호 해석기는 'transmit' 명령 뒤에 10글자 미만의 메시지를 입력해서 사용할 수 있습니다.\n";
                                    break;
                                case "FishBowlBuy":
                                    noun.result.creatureName = "금붕어";
                                    noun.result.displayText = "금붕어 어항을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "금붕어 어항을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "DiscoBallBuy":
                                    noun.result.creatureName = "디스코 볼";
                                    noun.result.displayText = "디스코 볼을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "디스코 볼을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n전등 스위치를 사용해 디스코를 시작합니다.\n";
                                    break;
                                case "RecordPlayerBuy":
                                    noun.result.creatureName = "레코드 플레이어";
                                    noun.result.displayText = "레코드 플레이어를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "레코드 플레이어를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "JackOLanternBuy":
                                    noun.result.creatureName = "잭오랜턴";
                                    noun.result.displayText = "잭오랜턴을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "잭오랜턴을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                                case "BunnySuitBuy":
                                    noun.result.creatureName = "토끼 슈트";
                                    noun.result.displayText = "토끼 슈트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "토끼 슈트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n\n";
                                    break;
                                case "BeeSuitBuy":
                                    noun.result.creatureName = "꿀벌 슈트";
                                    noun.result.displayText = "꿀벌 슈트를 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "꿀벌 슈트를 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n\n";
                                    break;
                                case "PlushiePajamaManBuy":
                                    noun.result.creatureName = "인형 파자마 맨";
                                    noun.result.displayText = "인형 파자마 맨을 주문하려고 합니다. \n아이템의 총 가격: [totalCost].\n\nCONFIRM 또는 DENY를 입력해주세요.\n\n";
                                    noun.result.terminalOptions[0].result.displayText = "인형 파자마 맨을 주문했습니다! 당신의 현재 소지금은 [playerCredits]입니다.\n함선 안의 물체를 재배치하려면 [B]를 누르세요. 배치를 확정하려면 [V]를 누르세요.\n";
                                    break;
                            }
                        }
                        break;
                    case "route":

                        foreach (CompatibleNoun noun in keyword.compatibleNouns)
                        {
                            if (noun.result.displayText.Contains("The cost to route to"))
                            {

                                noun.result.displayText = noun.result.displayText.Replace("The Company is buying at ", "현재 회사가 물품을 ");
                                noun.result.displayText = noun.result.displayText.Replace(".\n\nDo you want to route the autopilot to the Company building?", "에 매입하고 있습니다.\n\n회사 건물로 이동할까요?");

                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 41-Experimentation is", "41-익스페리멘테이션의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 220-Assurance is", "220-어슈어런스의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 56-Vow is", "56-보우의 이동 비용은");

                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 21-Offense is", "21-오펜스의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 61-March is", "61-머치의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 20-Adamance is", "20-애더먼스의 이동 비용은");

                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 85-Rend is", "85-렌드의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 7-Dine is", "7-다인의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 8-Titan is", "8-타이탄의 이동 비용은");

                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 5-Embrion is", "5-엠브리언의 이동 비용은");
                                noun.result.displayText = noun.result.displayText.Replace("The cost to route to 68-Artifice is", "68-아터피스의 이동 비용은");

                                noun.result.displayText = noun.result.displayText.Replace(". It is \ncurrently", "입니다.\n 이 위성의 현재 날씨는");
                                noun.result.displayText = noun.result.displayText.Replace(" on this moon.", "입니다.");
                                noun.result.displayText = noun.result.displayText.Replace("Please CONFIRM or DENY.", "CONFIRM 또는 DENY를 입력하세요.");

                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 41-Experimentation.", "41-익스페리멘테이션으로 이동합니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 220-Assurance.", "220-어슈어런스로 이동합니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 56-Vow.", "56-보우로 이동합니다.");

                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 21-Offense.", "21-오펜스로 이동합니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 61-March.", "61-머치로 이동합니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 20-Adamance.", "20-애더먼스로 이동합니다.");

                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 85-Rend.", "85-렌드로 이동합니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 7-Dine.", "7-다인으로 이동합니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Routing autopilot to 8-Titan.", "8-타이탄으로 이동합니다.");

                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace("Your new balance is ", "당신의 현재 소지금은 ");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace(".\n\nGood luck.", "입니다.\n\n행운을 빕니다.");
                                noun.result.terminalOptions[1].result.displayText = noun.result.terminalOptions[1].result.displayText.Replace(".\n\nPlease enjoy your flight.", "입니다.\n\n편안한 비행 되세요.");
                            }
                        }
                        break;
                    case "help":
                        keyword.specialKeywordResult.displayText = ">MOONS\n항로를 지정할 위성 목록을 봅니다.\n\n>STORE\n회사 상점의 유용한 아이템 목록을 봅니다.\n\n>BESTIARY\n기록된 생명체 목록을 봅니다.\n\n>STORAGE\n저장소에 있는 물체에 접근합니다.\n\n>OTHER\n기타 명령어를 봅니다.\n\n[numberOfItemsOnRoute2]\n";
                        break;
                    case "moons":
                        keyword.specialKeywordResult.displayText = "위성 카탈로그에 오신 것을 환영합니다.\n함선의 경로를 지정하려면 ROUTE를 입력하세요.\n위성에 대해 알아보려면 INFO를 입력하세요.\n____________________________\n\n* 회사 건물   //   [companyBuyingPercent]에 매입 중.\n\n* 익스페리멘테이션 [planetTime]\n* 어슈어런스 [planetTime]\n* 보우 [planetTime]\n\n* 오펜스 [planetTime]\n* 머치 [planetTime]\n* 애더먼스 [planetTime]\n\n* 렌드 [planetTime]\n* 다인 [planetTime]\n* 타이탄 [planetTime]\n\n";
                        break;
                    case "store":
                        keyword.specialKeywordResult.displayText = "회사 상점에 오신 것을 환영합니다. \n아이템 이름 앞에 BUY와 INFO를 입력해보세요. \n숫자를 입력하여 도구를 여러 개 주문할 수 있습니다.\n____________________________\n\n[buyableItemsList]\n\n함선 강화:\n* 시끄러운 경적    //    가격: $100\n* 신호 해석기    //    가격: $255\n* 순간이동기    //    가격: $375\n* 역방향 순간이동기    //    가격: $425\n\n함선 장식 목록은 할당량별로 순환됩니다. 다음 주에 꼭 다시 확인해보세요:\n------------------------------\n[unlockablesSelectionList]\n\n";
                        break;
                    case "other":
                        keyword.specialKeywordResult.displayText = "기타 명령어:\n\n>VIEW MONITOR\n메인 모니터의 지도 카메라를 켜고 끕니다.\n\n>SWITCH [플레이어 이름]\n메인 모니터에서 볼 플레이어를 전환합니다.\n\n>PING [레이더 부스터 이름]\n레이더 부스터에 소음을 재생합니다.\n\n>TRANSMIT [메세지]\n신호 해석기로 메세지를 전송합니다\n\n>SCAN\n현재 행성에 남아 있는 아이템 개수를 스캔합니다.\n\n\n";
                        break;
                    case "sigurd":
                        keyword.specialKeywordResult.displayText = "시구르드의 일지 기록\n\n일지를 읽으려면, 이름 앞에 \"VIEW\"를 입력하세요.\n---------------------------------\n\n[currentUnlockedLogsList]\n\n\n\n";
                        break;
                    case "bestiary":
                        keyword.specialKeywordResult.displayText = "생명체 도감\n\n생명체 파일에 접근하려면, 이름 뒤에 \"INFO\"를 입력하세요.\n---------------------------------\n\n[currentScannedEnemiesList]\n\n\n";
                        break;
                    case "pro flashlight":
                        keyword.word = "프로";
                        break;
                    case "flashlight":
                        keyword.word = "손전등";
                        break;
                    case "lockpicker":
                        keyword.word = "자물쇠";
                        break;
                    case "shovel":
                        keyword.word = "철제";
                        break;
                    case "jetpack":
                        keyword.word = "제트팩";
                        break;
                    case "boombox":
                        keyword.word = "붐박스";
                        break;
                    case "stun":
                        keyword.word = "기절";
                        break;
                    case "vow":
                        keyword.word = "보우";
                        break;
                    case "experimentation":
                        keyword.word = "익스페리멘테이션";
                        break;
                    case "assurance":
                        keyword.word = "어슈어런스";
                        break;
                    case "offense":
                        keyword.word = "오펜스";
                        break;
                    case "adamance":
                        keyword.word = "애더먼스";
                        break;
                    case "television":
                        keyword.word = "텔레비전";
                        break;
                    case "teleporter":
                        keyword.word = "순간이동기";
                        break;
                    case "rend":
                        keyword.word = "렌드";
                        break;
                    case "march":
                        keyword.word = "머치";
                        break;
                    case "dine":
                        keyword.word = "다인";
                        break;
                    case "titan":
                        keyword.word = "타이탄";
                        break;
                    case "artifice":
                        keyword.word = "아터피스";
                        break;
                    case "embrion":
                        keyword.word = "엠브리언";
                        break;
                    case "company":
                        keyword.word = "회사";
                        break;
                    case "walkie-talkie":
                        keyword.word = "무전기";
                        break;
                    case "spray paint":
                        keyword.word = "스프레이";
                        break;

                    //적
                    case "brackens":
                        keyword.word = "브래컨";
                        break;
                    case "forest keeper":
                        keyword.word = "숲지기";
                        break;
                    case "earth leviathan":
                        keyword.word = "육지";
                        break;
                    case "lasso":
                        keyword.word = "올가미";
                        break;
                    case "spore lizards":
                        keyword.word = "포자";
                        break;
                    case "snare fleas":
                        keyword.word = "올무";
                        break;
                    case "eyeless dogs":
                        keyword.word = "눈없는";
                        break;
                    case "hoarding bugs":
                        keyword.word = "비축";
                        break;
                    case "bunker spiders":
                        keyword.word = "벙커";
                        break;
                    case "hygroderes":
                        keyword.word = "하이그로디어";
                        break;
                    case "coil-heads":
                        keyword.word = "코일";
                        break;
                    case "manticoils":
                        keyword.word = "만티코일";
                        break;
                    case "baboon hawks":
                        keyword.word = "개코";
                        break;
                    case "nutcracker":
                        keyword.word = "호두까기";
                        break;
                    case "old birds":
                        keyword.word = "올드";
                        break;
                    case "butler":
                        keyword.word = "집사";
                        break;
                    case "circuit bees":
                        keyword.word = "회로";
                        break;
                    case "locusts":
                        keyword.word = "배회";
                        break;
                    case "thumpers":
                        keyword.word = "덤퍼";
                        break;
                    case "jester":
                        keyword.word = "광대";
                        break;


                    case "green suit":
                        keyword.word = "초록색";
                        break;
                    case "hazard suit":
                        keyword.word = "방호복";
                        break;
                    case "pajama suit":
                        keyword.word = "파자마";
                        break;
                    case "cozy lights":
                        keyword.word = "아늑한";
                        break;
                    case "signal":
                        keyword.word = "신호";
                        break;
                    case "toilet":
                        keyword.word = "변기";
                        break;
                    case "record":
                        keyword.word = "레코드";
                        break;
                    case "shower":
                        keyword.word = "샤워";
                        break;
                    case "table":
                        keyword.word = "테이블";
                        break;
                    case "romantic table":
                        keyword.word = "로맨틱한";
                        break;
                    case "file cabinet":
                        keyword.word = "파일";
                        break;
                    case "cupboard":
                        keyword.word = "수납장";
                        break;
                    case "bunkbeds":
                        keyword.word = "벙커침대";
                        break;

                    case "first":
                        keyword.word = "첫번째";
                        break;
                    case "smells":
                        keyword.word = "냄새";
                        break;
                    case "swing":
                        keyword.word = "상황의";
                        break;
                    case "shady":
                        keyword.word = "수상한";
                        break;
                    case "sound":
                        keyword.word = "너머의";
                        break;
                    case "goodbye":
                        keyword.word = "작별";
                        break;
                    case "screams":
                        keyword.word = "비명";
                        break;
                    case "golden":
                        keyword.word = "황금빛";
                        break;
                    case "idea":
                        keyword.word = "아이디어";
                        break;
                    case "nonsense":
                        keyword.word = "헛소리";
                        break;
                    case "hiding":
                        keyword.word = "숨어";
                        break;
                    case "desmond":
                        keyword.word = "데스몬드";
                        break;
                    case "real":
                        keyword.word = "진짜";
                        break;


                    case "zap gun":
                        keyword.word = "잽건";
                        break;
                    case "loud horn":
                        keyword.word = "시끄러운";
                        break;
                    case "extension ladder":
                        keyword.word = "연장형";
                        break;
                    case "inverse teleporter":
                        keyword.word = "역방향";
                        break;
                    case "jackolantern":
                        keyword.word = "잭오랜턴";
                        break;
                    case "radar":
                        keyword.word = "레이더";
                        break;
                    case "welcome mat":
                        keyword.word = "웰컴";
                        break;
                    case "goldfish":
                        keyword.word = "금붕어";
                        break;
                    case "plushie pajama man":
                        keyword.word = "인형";
                        break;
                    case "purple suit":
                        keyword.word = "보라색 슈트";
                        break;
                    case "purple":
                        keyword.word = "보라색";
                        break;
                    case "bee":
                        keyword.word = "꿀벌";
                        break;
                    case "bunny":
                        keyword.word = "토끼";
                        break;
                    case "disco":
                        keyword.word = "디스코";
                        break;
                }
            }
        }
    }
}
