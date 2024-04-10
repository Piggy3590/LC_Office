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
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix()
        {
            TranslateItem();
            TranslateDialogue();
            TranslatePlanet();
            TranslateUnlockableList();
        }

        static void TranslateUnlockableList()
        {
            foreach (UnlockableItem unlockableItem in StartOfRound.Instance.unlockablesList.unlockables)
            {
                switch (unlockableItem.unlockableName)
                {
                    case "Orange suit":
                        unlockableItem.unlockableName = "주황색 슈트";
                        break;
                    case "Green suit":
                        unlockableItem.unlockableName = "초록색 슈트";
                        break;
                    case "Hazard suit":
                        unlockableItem.unlockableName = "방호복 슈트";
                        break;
                    case "Pajama suit":
                        unlockableItem.unlockableName = "파자마 슈트";
                        break;
                    case "Cozy lights":
                        unlockableItem.unlockableName = "아늑한 조명";
                        break;
                    case "Teleporter":
                        unlockableItem.unlockableName = "순간이동기";
                        break;
                    case "Television":
                        unlockableItem.unlockableName = "텔레비전";
                        break;
                    case "Cupboard":
                        unlockableItem.unlockableName = "수납장";
                        break;
                    case "File Cabinet":
                        unlockableItem.unlockableName = "파일 캐비닛";
                        break;
                    case "Toilet":
                        unlockableItem.unlockableName = "변기";
                        break;
                    case "Shower":
                        unlockableItem.unlockableName = "샤워 부스";
                        break;
                    case "Light switch":
                        unlockableItem.unlockableName = "전등 스위치";
                        break;
                    case "Record player":
                        unlockableItem.unlockableName = "레코드 플레이어";
                        break;
                    case "Table":
                        unlockableItem.unlockableName = "테이블";
                        break;
                    case "Romantic table":
                        unlockableItem.unlockableName = "로맨틱한 테이블";
                        break;
                    case "Bunkbeds":
                        unlockableItem.unlockableName = "벙커침대";
                        break;
                    case "Terminal":
                        unlockableItem.unlockableName = "터미널";
                        break;
                    case "Signal translator":
                        unlockableItem.unlockableName = "신호 해석기";
                        break;
                    case "Loud horn":
                        unlockableItem.unlockableName = "시끄러운 경적";
                        break;
                    case "Inverse Teleporter":
                        unlockableItem.unlockableName = "역방향 순간이동기";
                        break;
                    case "JackOLantern":
                        unlockableItem.unlockableName = "잭오랜턴";
                        break;
                    case "Welcome mat":
                        unlockableItem.unlockableName = "웰컴 매트";
                        break;
                    case "Goldfish":
                        unlockableItem.unlockableName = "금붕어";
                        break;
                    case "Plushie pajama man":
                        unlockableItem.unlockableName = "인형 파자마 맨";
                        break;
                    case "Purple Suit":
                        unlockableItem.unlockableName = "보라색 슈트";
                        break;
                    case "Bee Suit":
                        unlockableItem.unlockableName = "꿀벌 슈트";
                        break;
                    case "Bunny Suit":
                        unlockableItem.unlockableName = "토끼 슈트";
                        break;
                    case "Disco Ball":
                        unlockableItem.unlockableName = "디스코 볼";
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
        private static void SetMapScreenInfoToCurrentLevel_Postfix(ref TextMeshProUGUI ___screenLevelDescription)
        {
            TranslatePlanet();
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("Orbiting", "공전 중");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("71 Gordion", "71 고르디온");

            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("41 Experimentation", "41 익스페리멘테이션");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("220 Assurance", "220 어슈어런스");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("56 Vow", "56 보우")
                ;
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("21 Offense", "21 오펜스");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("61 March", "61 머치");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("20 Adamance", "20 애더먼스");

            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("85 Rend", "85 렌드");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("7 Dine", "7 다인");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("8 Titan", "8 타이탄");
            
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("67 Artifice", "67 아터피스");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("5 Embrion", "5 엠브리언");
            ___screenLevelDescription.text = ___screenLevelDescription.text.Replace("44 Liquidation", "44 리퀴데이션");
        }

        [HarmonyPrefix]
        [HarmonyPatch("WritePlayerNotes")]
        public static void WritePlayerNotes_Prefix(ref EndOfGameStats ___gameStats, ref PlayerControllerB[] ___allPlayerScripts, ref int ___connectedPlayersAmount,
            ref bool ___localPlayerWasMostProfitableThisRound)
        {
            for (int i = 0; i < ___gameStats.allPlayerStats.Length; i++)
            {
                ___gameStats.allPlayerStats[i].isActivePlayer = ___allPlayerScripts[i].disconnectedMidGame || ___allPlayerScripts[i].isPlayerDead || ___allPlayerScripts[i].isPlayerControlled;
            }
            int num = 0;
            int num2 = 0;
            for (int j = 0; j < ___gameStats.allPlayerStats.Length; j++)
            {
                if (___gameStats.allPlayerStats[j].isActivePlayer && (j == 0 || ___gameStats.allPlayerStats[j].stepsTaken < num))
                {
                    num = ___gameStats.allPlayerStats[j].stepsTaken;
                    num2 = j;
                }
            }
            if (___connectedPlayersAmount > 0 && num > 10)
            {
                ___gameStats.allPlayerStats[num2].playerNotes.Add("가장 게으른 직원.");
            }
            num = 0;
            for (int k = 0; k < ___gameStats.allPlayerStats.Length; k++)
            {
                if (___gameStats.allPlayerStats[k].isActivePlayer && ___gameStats.allPlayerStats[k].turnAmount > num)
                {
                    num = ___gameStats.allPlayerStats[k].turnAmount;
                    num2 = k;
                }
            }
            if (___connectedPlayersAmount > 0)
            {
                ___gameStats.allPlayerStats[num2].playerNotes.Add("가장 피해망상이 심한 직원.");
            }
            num = 0;
            for (int l = 0; l < ___gameStats.allPlayerStats.Length; l++)
            {
                if (___gameStats.allPlayerStats[l].isActivePlayer && !___allPlayerScripts[l].isPlayerDead && ___gameStats.allPlayerStats[l].damageTaken > num)
                {
                    num = ___gameStats.allPlayerStats[l].damageTaken;
                    num2 = l;
                }
            }
            if (___connectedPlayersAmount > 0)
            {
                ___gameStats.allPlayerStats[num2].playerNotes.Add("가장 많은 부상을 입음.");
            }
            num = 0;
            for (int m = 0; m < ___gameStats.allPlayerStats.Length; m++)
            {
                if (___gameStats.allPlayerStats[m].isActivePlayer && ___gameStats.allPlayerStats[m].profitable > num)
                {
                    num = ___gameStats.allPlayerStats[m].profitable;
                    num2 = m;
                }
            }
            if (___connectedPlayersAmount > 0 && num > 50)
            {
                if (num2 == (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    ___localPlayerWasMostProfitableThisRound = true;
                }
                ___gameStats.allPlayerStats[num2].playerNotes.Add("가장 수익성이 높음");
            }

            return;
        }
        
        static void TranslatePlanet()
        {
            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                level.LevelDescription = level.LevelDescription.Replace("POPULATION: Abandoned", "인구: 버려짐");
                level.LevelDescription = level.LevelDescription.Replace("POPULATION: None", "인구: 없음");
                level.LevelDescription = level.LevelDescription.Replace("POPULATION: Unknown", "인구: 알 수 없음");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: A landscape of deep valleys and mountains.",
                    "조건: 깊은 계곡과 산이 어우러짐.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Waning forests. Abandoned facilities littered across the landscape.",
                    "조건: 숲이 점점 쇠퇴하고 있으며, 버려진 시설이 곳곳에 흩어져 있습니다.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Jagged and weathered terrain.",
                    "조건: 들쭉날쭉하고 풍화됨.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: No land masses. Continual storms.",
                    "조건: 육지가 없습니다. 지속적으로 폭풍이 일어납니다.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Frozen, rocky. Its planet orbits a white dwarf star.",
                    "조건: 얼어붙은 바위로 이루어졌으며, 백색 왜성 주위를 공전하고 있습니다.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Desolate, made of amethyst.",
                    "조건: 황폐함, 자수정으로 이루어짐.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Arid. Thick haze, worsened by industrial artifacts.",
                    "조건: 건조하며, 거주 가능성이 낮습니다. 산업 인공물로 인해 악화되었습니다.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Previously mined for its rich industrial resources, Liquidation is now largely an ocean moon.",
                    "조건: 풍부한 산업 자원으로 채굴되던 리퀴데이션은 이제는 대부분 해양 위성으로 남아 있습니다.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Expansive. Constant rain.",
                    "조건: 방대함. 지속적으로 비가 내림.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Frozen, rocky. This moon was mined for resources. It's easy to get lost here.",
                    "조건: 얼어붙은 바위로 이루어졌습니다. 이 달은 자원을 얻기 위해 채굴되었습니다. 이곳에서는 길을 잃기 쉽습니다.");

                level.LevelDescription = level.LevelDescription.Replace("CONDITIONS: Humid. Rough terrain. Teeming with plant-life.",
                    "조건: 습함. 거친 지형. 식물이 많습니다.");

                //
                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Home to a lively, diverse ecosystem of smaller-sized omnivores",
                    "동물군: 활기차고 다양한 생태계의 본거지이며, 작은 크기의 잡식성 동물로 구성되어 있습니다.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Rumored active machinery left behind.",
                    "동물군: 아직 작동 중인 기계가 방치되어 있다는 소문이 있습니다.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Ecosystem supports territorial behaviour.",
                    "동물군: 영역 행동이 빈번함.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Unknown",
                    "동물군: 알 수 없음");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Unlikely for complex life to exist",
                    "동물군: 이곳에 다세포 생명체가 존재할 가능성은 거의 없습니다.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Embrion is devoid of biological life.",
                    "동물군: 엠브리언에는 생물학적 생명체가 존재하지 않습니다.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Dominated by a few species.",
                    "동물군: 몇몇 종이 지배하고 있음.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Diverse",
                    "동물군: 다양함");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: A competitive ecosystem supports aggressive lifeforms.",
                    "동물군: 경쟁적인 생태계 때문에 공격적인 생명체가 많음.");

                level.LevelDescription = level.LevelDescription.Replace("FAUNA: Dangerous entities have been rumored to take residence in the vast network of tunnels.",
                    "동물군: 위험한 존재들이 광대한 터널 네트워크에 거주한다는 소문이 돌았습니다.");
                if (Plugin.translatePlanet)
                {
                    if (level.PlanetName == "71 Gordion")
                    {
                        level.PlanetName = "71 고르디온";
                    }else if (level.PlanetName == "41 Experimentation")
                    {
                        level.PlanetName = "41 익스페리멘테이션";
                    }
                    else if (level.PlanetName == "220 Assurance")
                    {
                        level.PlanetName = "220 어슈어런스";
                    }
                    else if (level.PlanetName == "56 Vow")
                    {
                        level.PlanetName = "56 보우";
                    }
                    else if (level.PlanetName == "61 March")
                    {
                        level.PlanetName = "61 머치";
                    }
                    else if (level.PlanetName == "21 Offense")
                    {
                        level.PlanetName = "21 오펜스";
                    }
                    else if (level.PlanetName == "85 Rend")
                    {
                        level.PlanetName = "85 렌드";
                    }
                    else if (level.PlanetName == "7 Dine")
                    {
                        level.PlanetName = "7 다인";
                    }
                    else if (level.PlanetName == "8 Titan")
                    {
                        level.PlanetName = "8 타이탄";
                    }
                    
                    else if (level.PlanetName == "20 Adamance")
                    {
                        level.PlanetName = "20 애더먼스";
                    }
                    else if (level.PlanetName == "67 Artifice")
                    {
                        level.PlanetName = "67 아터피스";
                    }
                    else if (level.PlanetName == "5 Embrion")
                    {
                        level.PlanetName = "5 엠브리언";
                    }
                    else if (level.PlanetName == "44 Liquidation")
                    {
                        level.PlanetName = "44 리퀴데이션";
                    }
                }
            }
        }
        static void TranslateDialogue()
        {
            foreach (DialogueSegment dialogue in StartOfRound.Instance.openingDoorDialogue)
            {
                switch (dialogue.speakerText)
                {
                    case "PILOT COMPUTER":
                        dialogue.speakerText = "파일럿 컴퓨터";
                        break;
                }
                switch (dialogue.bodyText)
                {
                    case "Warning! No response from crew, which has not returned. Emergency code activated.":
                        dialogue.bodyText = "경고! 모든 팀원이 응답하지 않으며 함선에 돌아오지 않았습니다. 긴급 코드가 활성화되었습니다.";
                        break;
                    case "The autopilot will now attempt to fly to the closest safe spaceport. Your items have been lost.":
                        dialogue.bodyText = "가까운 기지로 이동합니다. 모든 폐품을 분실했습니다.";
                        break;
                    case "Alert! The autopilot is leaving due to dangerous conditions.":
                        dialogue.bodyText = "경고! 위험한 상황으로 인해 함선이 이륙하고 있습니다.";
                        break;
                    case "The Company must minimize risk of damage to proprietary hardware. Goodbye!":
                        dialogue.bodyText = "우리 회사는 독점 하드웨어에 대한 손상 위험을 최소화해야 합니다. 안녕히 계세요!\r\n";
                        break;
                }
            }
        }

        static void TranslateItem()
        {
            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                switch (item.itemName)
                {
                    case "Boombox":
                        item.itemName = "붐박스";
                        item.toolTips[0] = "음악 전환하기 : [RMB]";
                        break;
                    case "Flashlight":
                        item.itemName = "손전등";
                        item.toolTips[0] = "전등 전환하기 : [RMB]";
                        break;
                    case "Jetpack":
                        item.itemName = "제트팩";
                        item.toolTips[0] = "제트팩 사용하기 : [RMB]";
                        break;
                    case "Key":
                        item.itemName = "열쇠";
                        item.toolTips[0] = "열쇠 사용하기 : [RMB]";
                        break;
                    case "Lockpicker":
                        item.itemName = "자물쇠 따개";
                        item.toolTips[0] = "문에 설치하기 : [RMB]";
                        break;
                    case "Apparatus":
                        item.itemName = "장치";
                        break;
                    case "Pro-flashlight":
                        item.itemName = "프로 손전등";
                        item.toolTips[0] = "전등 전환하기 : [RMB]";
                        break;
                    case "Shovel":
                        item.itemName = "철제 삽";
                        item.toolTips[0] = "삽 휘두르기: [RMB]";
                        break;
                    case "Stun grenade":
                        item.itemName = "기절 수류탄";
                        item.toolTips[0] = "수류탄 사용하기 : [RMB]";
                        break;
                    case "Extension ladder":
                        item.itemName = "연장형 사다리";
                        item.toolTips[0] = "사다리 꺼내기 : [RMB]";
                        break;
                    case "TZP-Inhalant":
                        item.itemName = "TZP-흡입제";
                        item.toolTips[0] = "TZP 흡입하기 : [RMB]";
                        break;
                    case "Walkie-talkie":
                        item.itemName = "무전기";
                        item.toolTips[0] = "전원 버튼 : [Q]";
                        item.toolTips[1] = "목소리 송신하기 : [RMB]";
                        break;
                    case "Zap gun":
                        item.itemName = "잽건";
                        item.toolTips[0] = "위협 감지하기 : [RMB]";
                        break;
                    case "Magic 7 ball":
                        item.itemName = "마법의 7번 공";
                        break;
                    case "Airhorn":
                        item.itemName = "에어혼";
                        item.toolTips[0] = "에어혼 사용하기 : [RMB]";
                        break;
                    case "Bell":
                        item.itemName = "종";
                        break;
                    case "Big bolt":
                        item.itemName = "큰 나사";
                        break;
                    case "Bottles":
                        item.itemName = "병 묶음";
                        break;
                    case "Brush":
                        item.itemName = "빗";
                        break;
                    case "Candy":
                        item.itemName = "사탕";
                        break;
                    case "Cash register":
                        item.itemName = "금전 등록기";
                        item.toolTips[0] = "금전 등록기 사용하기 : [RMB]";
                        break;
                    case "Chemical jug":
                        item.itemName = "화학 용기";
                        break;
                    case "Clown horn":
                        item.itemName = "광대 나팔";
                        item.toolTips[0] = "광대 나팔 사용하기 : [RMB]";
                        break;
                    case "Large axle":
                        item.itemName = "대형 축";
                        break;
                    case "Teeth":
                        item.itemName = "틀니";
                        break;
                    case "Dust pan":
                        item.itemName = "쓰레받기";
                        break;
                    case "Egg beater":
                        item.itemName = "달걀 거품기";
                        break;
                    case "V-type engine":
                        item.itemName = "V형 엔진";
                        break;
                    case "Golden cup":
                        item.itemName = "황금 컵";
                        break;
                    case "Fancy lamp":
                        item.itemName = "멋진 램프";
                        break;
                    case "Painting":
                        item.itemName = "그림";
                        break;
                    case "Plastic fish":
                        item.itemName = "플라스틱 물고기";
                        break;
                    case "Laser pointer":
                        item.itemName = "레이저 포인터";
                        item.toolTips[0] = "레이저 전환하기 : [RMB]";
                        break;
                    case "Gold bar":
                        item.itemName = "금 주괴";
                        break;
                    case "Hairdryer":
                        item.itemName = "헤어 드라이기";
                        item.toolTips[0] = "헤어 드라이기 사용하기 : [RMB]";
                        break;
                    case "Magnifying glass":
                        item.itemName = "돋보기";
                        break;
                    case "Metal sheet":
                        item.itemName = "금속 판";
                        break;
                    case "Cookie mold pan":
                        item.itemName = "쿠키 틀";
                        break;
                    case "Mug":
                        item.itemName = "머그잔";
                        break;
                    case "Perfume bottle":
                        item.itemName = "향수 병";
                        break;
                    case "Old phone":
                        item.itemName = "구식 전화기";
                        break;
                    case "Jar of pickles":
                        item.itemName = "피클 병";
                        break;
                    case "Pill bottle":
                        item.itemName = "약 병";
                        break;
                    case "Remote":
                        item.itemName = "리모컨";
                        item.toolTips[0] = "리모컨 사용하기 : [RMB]";
                        break;
                    case "Ring":
                        item.itemName = "반지";
                        break;
                    case "Robot toy":
                        item.itemName = "로봇 장난감";
                        break;
                    case "Rubber Ducky":
                        item.itemName = "고무 오리";
                        break;
                    case "Red soda":
                        item.itemName = "빨간색 소다";
                        break;
                    case "Steering wheel":
                        item.itemName = "운전대";
                        break;
                    case "Stop sign":
                        item.itemName = "정지 표지판";
                        item.toolTips[0] = "표지판 사용하기 : [RMB]";
                        break;
                    case "Tea kettle":
                        item.itemName = "찻주전자";
                        break;
                    case "Toothpaste":
                        item.itemName = "치약";
                        break;
                    case "Toy cube":
                        item.itemName = "장난감 큐브";
                        break;
                    case "Hive":
                        item.itemName = "벌집";
                        break;
                    case "Radar-booster":
                        item.itemName = "레이더 부스터";
                        item.toolTips[0] = "부스터 켜기 : [RMB]";
                        break;
                    case "Yield sign":
                        item.itemName = "양보 표지판";
                        item.toolTips[0] = "표지판 사용하기 : [RMB]";
                        break;
                    case "Shotgun":
                        item.itemName = "산탄총";
                        item.toolTips[0] = "격발 : [RMB]";
                        item.toolTips[1] = "재장전 : [E]";
                        item.toolTips[2] = "안전 모드 해제 : [Q]";
                        break;
                    case "Ammo":
                        item.itemName = "탄약";
                        break;
                    case "Spray paint":
                        item.itemName = "스프레이 페인트";
                        item.toolTips[0] = "스프레이 뿌리기 : [RMB]";
                        item.toolTips[1] = "캔 흔들기 : [Q]";
                        break;
                    case "Homemade flashbang":
                        item.itemName = "사제 섬광탄";
                        item.toolTips[0] = "사제 섬광탄 사용하기 : [RMB]";
                        break;
                    case "Gift":
                        item.itemName = "선물";
                        item.toolTips[0] = "선물 열기 : [RMB]";
                        break;
                    case "Flask":
                        item.itemName = "플라스크";
                        break;
                    case "Tragedy":
                        item.itemName = "비극";
                        item.toolTips[0] = "가면 쓰기 : [RMB]";
                        break;
                    case "Comedy":
                        item.itemName = "희극";
                        item.toolTips[0] = "가면 쓰기 : [RMB]";
                        break;
                    case "Whoopie cushion":
                        item.itemName = "방귀 쿠션";
                        break;
                    case "Kitchin knife":
                        item.itemName = "식칼";
                        item.toolTips[0] = "찌르기 : [RMB]";
                        break;
                    case "Easter egg":
                        item.itemName = "부활절 달걀";
                        break;
                }
            }
        }
    }
}
