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
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("DisplayTip")]
        private static void DisplayTip_Prefix(string headerText, string bodyText)
        {
            switch (headerText)
            {
                case "To read the manual:":
                    headerText = "지침서 읽기";
                    bodyText = "Z를 눌러 자세히 봅니다. Q와 E를 눌러 페이지를 넘깁니다.";
                    break;
                case "TIP:":
                    headerText = "팁:";
                    if (bodyText == "Use the ship computer terminal to access secure doors.")
                    {
                        bodyText = "함선 내 컴퓨터 터미널을 사용하여 보안 문에 접근하세요.";
                    }else
                    {
                        bodyText = "잠긴 문을 효율적으로 지나가려면 함선 터미널에서 <u>자물쇠 따개</u>를 주문하세요.";
                    }
                    break;
                case "Welcome!":
                    headerText = "환영합니다!";
                    bodyText = "우클릭을 눌러 함선 내부의 물체를 스캔하고 정보를 확인할 수 있습니다.";
                    break;
                case "Got scrap!":
                    headerText = "폐품을 얻었습니다!";
                    bodyText = "판매하려면 터미널을 이용해 함선을 회사 건물로 이동시키세요.";
                    break;
                case "Items missed!":
                    headerText = "아이템을 놓쳤습니다!";
                    bodyText = "수송선이 구매하신 상품과 함께 돌아갔습니다. 비용은 환불되지 않습니다.";
                    break;
                case "Item stored!":
                    headerText = "아이템을 보관했습니다!";
                    bodyText = "보관된 물품은 터미널에서 'STORAGE' 명령을 사용하여 확인할 수 있습니다.";
                    break;
                case "HALT!":
                    headerText = "기다리세요!";
                    bodyText = "할당량을 충족시키기 위한 날이 0일 남았습니다. 터미널을 이용하여 회사로 이동한 후 아이템을 판매하세요.";
                    break;
                case "Weather alert!":
                    headerText = "날씨 경고!";
                    bodyText = "당신은 현재 일식이 일어난 위성에 착륙했습니다. 주의하세요!";
                    break;
                case "???":
                    bodyText = "입구가 막힌 것 같습니다.";
                    break;
                case "ALERT!":
                    headerText = "경고!";
                    bodyText = "당장 모든 금속성 아이템을 떨어뜨리세요! 정전기가 감지되었습니다. 당신은 몇 초 뒤에 사망할 것입니다.";
                    break;
                case "Welcome back!":
                    headerText = "돌아오신 것을 환영합니다!";
                    bodyText = "이전에 구입한 도구에 대한 보상을 받았습니다. 받으려면 다시 구매하셔야 합니다.";
                    break;
                case "Tip":
                    headerText = "팁";
                    bodyText = "함선의 물체를 재배치하려면 B를 누르세요. 취소하려면 E를 누르세요.";
                    break;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("DisplayNewScrapFound")]
        private static void DisplayNewScrapFound_Postfix()
        {
            HUDManager.Instance.ScrapItemBoxes[0].headerText.text = HUDManager.Instance.ScrapItemBoxes[0].headerText.text.Replace("collected", "수집함");
            HUDManager.Instance.ScrapItemBoxes[0].valueText.text = HUDManager.Instance.ScrapItemBoxes[0].valueText.text.Replace("Value", "가격");

            HUDManager.Instance.ScrapItemBoxes[1].headerText.text = HUDManager.Instance.ScrapItemBoxes[1].headerText.text.Replace("collected", "수집함");
            HUDManager.Instance.ScrapItemBoxes[1].valueText.text = HUDManager.Instance.ScrapItemBoxes[1].valueText.text.Replace("Value", "가격");

            HUDManager.Instance.ScrapItemBoxes[2].headerText.text = HUDManager.Instance.ScrapItemBoxes[2].headerText.text.Replace("collected", "수집함");
            HUDManager.Instance.ScrapItemBoxes[2].valueText.text = HUDManager.Instance.ScrapItemBoxes[2].valueText.text.Replace("Value", "가격");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ChangeControlTipMultiple")]
        private static void ChangeControlTipMultiple_Postfix(string[] allLines, bool holdingItem, Item itemProperties, ref TextMeshProUGUI[] ___controlTipLines)
        {
            if (holdingItem)
            {
                ___controlTipLines[0].text = itemProperties.itemName + " 떨어뜨리기 : [G]";
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        private static void Update_Prefix(ref TextMeshProUGUI ___planetInfoHeaderText, ref TextMeshProUGUI ___globalNotificationText)
        {
            if (___planetInfoHeaderText.text.Contains("CELESTIAL BODY:"))
            {
                ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("CELESTIAL BODY:", "천체:");
                if (___planetInfoHeaderText.text.Contains("Experimentation"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Experimentation", "익스페리멘테이션");
                }
                else if (___planetInfoHeaderText.text.Contains("Assurance"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Assurance", "어슈어런스");
                }
                else if (___planetInfoHeaderText.text.Contains("Offense"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Offense", "오펜스");
                }
                else if (___planetInfoHeaderText.text.Contains("Adamance"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Adamance", "애더먼스");
                }
                else if (___planetInfoHeaderText.text.Contains("Rend"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Rend", "렌드");
                }
                else if (___planetInfoHeaderText.text.Contains("Dine"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Dine", "다인");
                }
                else if (___planetInfoHeaderText.text.Contains("March"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("March", "머치");
                }
                else if (___planetInfoHeaderText.text.Contains("Vow"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Vow", "보우");
                }
                else if (___planetInfoHeaderText.text.Contains("Titan"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Titan", "타이탄");
                }
                else if (___planetInfoHeaderText.text.Contains("Artifice"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Artifice", "아터피스");
                }
                else if (___planetInfoHeaderText.text.Contains("Embrion"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Embrion", "엠브리언");
                }
                else if (___planetInfoHeaderText.text.Contains("Gordion"))
                {
                    ___planetInfoHeaderText.text = ___planetInfoHeaderText.text.Replace("Gordion", "고르디온");
                }
            }

            if (___globalNotificationText.text == "New creature data sent to terminal!")
            {
                ___globalNotificationText.text = "새로운 생명체 데이터가 터미널에 전송되었습니다!";
            }
            else if (___globalNotificationText.text.Contains("Found journal entry:"))
            {
                ___globalNotificationText.text = ___globalNotificationText.text.Replace("Found journal entry:", "일지를 찾았습니다:");
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch("AttemptScanNode")]
        private static void AttemptScanNode_Prefix(ScanNodeProperties node)
        {
            if (node.headerText.Contains("Baboon hawk"))
            {
                node.headerText = "개코 매";
            }
            else if (node.headerText.Contains("Hygrodere"))
            {
                node.headerText = "하이그로디어";
            }
            else if (node.headerText.Contains("Mask Hornets"))
            {
                node.headerText = "위장 말벌";
            }
            else if (node.headerText.Contains("Butler"))
            {
                node.headerText = "집사";
            }
            else if (node.headerText.Contains("Snare flea"))
            {
                node.headerText = "올무 벼룩";
            }
            else if (node.headerText.Contains("Half"))
            {
                if (Plugin.thumperTranslation)
                {
                    node.headerText = "썸퍼";
                }else
                {
                    node.headerText = "덤퍼";
                }
            }
            else if (node.headerText.Contains("Roaming locusts"))
            {
                node.headerText = "배회 메뚜기";
            }
            else if (node.headerText.Contains("Manticoil"))
            {
                node.headerText = "만티코일";
            }
            else if (node.headerText.Contains("Bracken"))
            {
                node.headerText = "브래컨";
            }
            else if (node.headerText.Contains("Forest Giant"))
            {
                node.headerText = "숲 거인";
            }
            else if (node.headerText.Contains("Hoarding bug"))
            {
                node.headerText = "비축 벌레";
            }
            else if (node.headerText.Contains("Jester"))
            {
                node.headerText = "광대";
            }
            else if (node.headerText.Contains("Lasso"))
            {
                node.headerText = "올가미 인간";
            }
            else if (node.headerText.Contains("Eyeless dog"))
            {
                node.headerText = "눈없는 개";
            }
            else if (node.headerText.Contains("Nutcracker"))
            {
                node.headerText = "호두까기 인형";
            }
            else if (node.headerText.Contains("Spore lizard"))
            {
                node.headerText = "포자 도마뱀";
            }
            else if (node.headerText.Contains("Old Bird"))
            {
                node.headerText = "올드 버드";
            }
            else if (node.headerText.Contains("Circuit bees"))
            {
                node.headerText = "회로 벌";
            }
            else if (node.headerText.Contains("Bunker spider"))
            {
                node.headerText = "벙커 거미";
            }
            else if (node.headerText.Contains("Earth Leviathan"))
            {
                node.headerText = "육지 레비아탄";
            }
            else if (node.headerText.Contains("Coil-head"))
            {
                node.headerText = "코일 헤드";
            }


            if (node.headerText.Contains("Coil"))
            {
                node.headerText = "코일";
                node.subText = "배터리를 충전합니다";
            }
            else if (node.headerText.Contains("Terminal"))
            {
                node.headerText = "터미널";
                node.subText = "손전등을 구매하고 행성을 여행하세요!";
            }
            else if (node.headerText.Contains("Brake lever"))
            {
                node.headerText = "브레이크 레버";
                node.subText = "함선을 착륙시킵니다.";
            }
            else if (node.headerText.Contains("Training manual"))
            {
                node.headerText = "교육용 지침서";
                node.subText = "유용한 정보들!";
            }
            else if (node.headerText.Contains("Door controls"))
            {
                node.headerText = "문 제어기";
                node.subText = "영원히 닫을 수는 없습니다";
            }
            else if (node.headerText.Contains("Quota"))
            {
                node.headerText = "할당량";
                node.subText = "폐품을 현금으로 판매하세요.";
            }
            else if (node.headerText.Contains("Ship"))
            {
                node.headerText = "함선";
                node.subText = "당신의 기지";
            }
            else if (node.headerText.Contains("Main entrance"))
            {
                node.headerText = "정문";
            }
            else if (node.headerText.Contains("Data chip"))
            {
                node.headerText = "데이터 칩";
            }

            if (node.headerText.Contains("Key"))
            {
                node.headerText = "열쇠";
                node.subText = "가격: $3";
            }
            else if (node.headerText.Contains("Apparatice"))
            {
                node.headerText = "장치";
                node.subText = "가격: ???";
            }
            else if (node.headerText.Contains("Magic 7 ball"))
            {
                node.headerText = "마법의 7번 공";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Airhorn"))
            {
                node.headerText = "에어혼";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Brass bell"))
            {
                node.headerText = "황동 종";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Big bolt"))
            {
                node.headerText = "큰 나사";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Bottles"))
            {
                node.headerText = "병 묶음";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Hair brush"))
            {
                node.headerText = "빗";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Candy"))
            {
                node.headerText = "사탕";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Cash register"))
            {
                node.headerText = "금전 등록기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Chemical jug"))
            {
                node.headerText = "화학 용기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Clown horn"))
            {
                node.headerText = "광대 나팔";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Large axle"))
            {
                node.headerText = "대형 축";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Teeth"))
            {
                node.headerText = "틀니";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Dust pan"))
            {
                node.headerText = "쓰레받기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Egg beater"))
            {
                node.headerText = "달걀 거품기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("V-type engine"))
            {
                node.headerText = "V형 엔진";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Golden cup"))
            {
                node.headerText = "황금 컵";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Fancy lamp"))
            {
                node.headerText = "멋진 램프";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Painting"))
            {
                node.headerText = "그림";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Plastic fish"))
            {
                node.headerText = "플라스틱 물고기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Laser pointer"))
            {
                node.headerText = "레이저 포인터";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Gold Bar"))
            {
                node.headerText = "금 주괴";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Hairdryer"))
            {
                node.headerText = "헤어 드라이기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Magnifying glass"))
            {
                node.headerText = "돋보기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Tattered metal sheet"))
            {
                node.headerText = "너덜너덜한 금속 판";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Cookie mold pan"))
            {
                node.headerText = "쿠키 틀";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Coffee mug"))
            {
                node.headerText = "커피 머그잔";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Perfume bottle"))
            {
                node.headerText = "향수 병";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Old phone"))
            {
                node.headerText = "구식 전화기";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Jar of pickles"))
            {
                node.headerText = "피클 병";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Pill Bottle"))
            {
                node.headerText = "약 병";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Remote"))
            {
                node.headerText = "리모컨";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Wedding ring"))
            {
                node.headerText = "결혼 반지";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Robot Toy"))
            {
                node.headerText = "로봇 장난감";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Rubber Ducky"))
            {
                node.headerText = "고무 오리";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Red soda"))
            {
                node.headerText = "빨간색 소다";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Steering Wheel"))
            {
                node.headerText = "운전대";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Stop Sign"))
            {
                node.headerText = "정지 표지판";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Tea Kettle"))
            {
                node.headerText = "찻주전자";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Toothpaste"))
            {
                node.headerText = "치약";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Toy cube"))
            {
                node.headerText = "장난감 큐브";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Bee hive"))
            {
                node.headerText = "벌집";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.subText.Contains("(Radar booster)"))
            {
                node.subText = "(레이더 부스터)";
            }
            else if (node.headerText.Contains("Yield Sign"))
            {
                node.headerText = "양보 표지판";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Shotgun"))
            {
                node.headerText = "더블-배럴";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Ammo"))
            {
                node.headerText = "산탄총 탄약";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Homemade Flashbang"))
            {
                node.headerText = "사제 섬광탄";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Gift"))
            {
                node.headerText = "선물 상자";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Flask"))
            {
                node.headerText = "플라스크";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Tragedy"))
            {
                node.headerText = "비극";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Comedy"))
            {
                node.headerText = "희극";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Whoopie cushion"))
            {
                node.headerText = "방퀴 쿠션";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Kitchin knife"))
            {
                node.headerText = "식칼";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
            else if (node.headerText.Contains("Easter egg"))
            {
                node.headerText = "부활절 달걀";
                node.subText = node.subText.Replace("Value:", "가격:");
            }
        }

        [HarmonyPostfix, HarmonyPatch("OnEnable")]
        static void PostfixOnEnable()
        {
            HUDManager.Instance.chatTextField.onSubmit.AddListener(OnSubmitChat);
        }

        [HarmonyPostfix, HarmonyPatch("OnDisable")]
        static void PrefixOnDisable()
        {
            HUDManager.Instance.chatTextField.onSubmit.RemoveListener(OnSubmitChat);
        }

        static void OnSubmitChat(string chatString)
        {
            var localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (!string.IsNullOrEmpty(chatString) && chatString.Length < 50)
            {
                HUDManager.Instance.AddTextToChatOnServer(chatString, (int)localPlayer.playerClientId);
            }
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position) > 24.4f && (!GameNetworkManager.Instance.localPlayerController.holdingWalkieTalkie || !StartOfRound.Instance.allPlayerScripts[i].holdingWalkieTalkie))
                {
                    HUDManager.Instance.playerCouldRecieveTextChatAnimator.SetTrigger("ping");
                    break;
                }
            }
            localPlayer.isTypingChat = false;
            HUDManager.Instance.chatTextField.text = "";
            EventSystem.current.SetSelectedGameObject(null);
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat);
            HUDManager.Instance.typingIndicator.enabled = false;
        }

        [HarmonyPrefix, HarmonyPatch("SubmitChat_performed")]
        static void PrefixSubmitChat_performed(
            ref bool __runOriginal
        )
        {
            __runOriginal = false;
        }
    }
}
