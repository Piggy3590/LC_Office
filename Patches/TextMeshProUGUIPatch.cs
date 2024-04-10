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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(TextMeshProUGUI))]
    internal class TextMeshProUGUIPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void Awake_Postfix(TextMeshProUGUI __instance)
        {
            switch (__instance.text)
            {
                case "  Online":
                    __instance.text = "  온라인";
                    break;
                case "> Online":
                    __instance.text = "> 온라인";
                    break;
                case "(Recommended)":
                    __instance.text = "(권장)";
                    break;
                case " SET-UP":
                    __instance.text = "설정";
                    break;

                case "> Host":
                    __instance.text = "> 호스트";
                    break;
                case "> Join a crew":
                    __instance.text = "> 팀에 합류하기";
                    break;
                case "> Join LAN session":
                    __instance.text = "> LAN 세션 합류하기";
                    break;
                case "> Settings":
                    __instance.text = "> 설정";
                    break;
                case "> Credits":
                    __instance.text = "> 크레딧";
                    break;
                case "> Quit":
                    __instance.text = "> 종료";
                    break;

                case "ACCESSIBILITY":
                    __instance.text = "접근성";
                    break;
                case "Unconfirmed changes!":
                    __instance.text = "변경 사항이 저장되지 않음!";
                    break;
                case "CONTROLS":
                    __instance.text = "조작";
                    break;
                case "REMAP CONTROLS":
                    __instance.text = "조작 키 재설정";
                    break;
                case "DISPLAY":
                    __instance.text = "디스플레이";
                    break;
                case "ENABLED":
                    __instance.text = "활성화됨";
                    break;
                case "Save File":
                    __instance.text = "저장 파일";
                    break;
                case "Server name:":
                    __instance.text = "서버 이름:";
                    break;
                case "Host LAN Server:":
                    __instance.text = "LAN 서버 호스트하기:";
                    break;

                case "Sort: worldwide":
                    __instance.text = "정렬: 전 세계";
                    break;
                case "Sort: Friends":
                    __instance.text = "정렬: 친구";
                    break;
                case "Sort: near":
                    __instance.text = "정렬: 가까운 서버";
                    break;
                case "Sort: far":
                    __instance.text = "정렬: 먼 서버";
                    break;

                case "Fullscreen":
                    __instance.text = "전체 화면";
                    break;
                case "Use monitor (V-Sync)":
                    __instance.text = "모니터 사용 (수직 동기화)";
                    break;

                case "Display mode:":
                    __instance.text = "디스플레이 모드:";
                    break;
                case "프레임 제한:":
                    __instance.text = "프레임 제한:";
                    break;

                case "(Launched in LAN mode)":
                    __instance.text = "(LAN 모드로 실행됨)";
                    break;

                case "Servers":
                    __instance.text = "서버";
                    break;
                case "Weekly Challenge Results":
                    __instance.text = "주간 챌린지 결과";
                    break;
                case "Loading server list...":
                    __instance.text = "서버 목록 불러오는 중...";
                    break;
                case "Loading ranking...":
                    __instance.text = "순위 불러오는 중...";
                    break;
                case "Loading...":
                    __instance.text = "로딩 중...";
                    break;
                case "Join":
                    __instance.text = "참가";
                    break;

                case "Version 50 is here!":
                    __instance.text = "버전 50이 나왔습니다!";
                    break;
                case "Credits":
                    __instance.text = "크레딧";
                    break;
                case "An error occured!":
                    __instance.text = "오류가 발생했습니다!";
                    break;
                case "Do you want to delete File 1?":
                    __instance.text = "정말 파일 1을 삭제할까요?";
                    break;
                case "Confirm changes?":
                    __instance.text = "변경 사항을 저장할까요?";
                    break;
                case "You are in LAN mode. When allowing remote connections through LAN, please ensure you have sufficient network security such as a firewall and/or VPN.":
                    __instance.text = "LAN 모드에 있습니다. LAN을 통한 원격 연결을 허용하는 경우 방화벽 및/또는 VPN과 같은 네트워크 보안이 충분한지 확인하십시오.";
                    break;
                case "Enter a tag...":
                    __instance.text = "태그를 입력하세요...";
                    break;
                case "Enter server tag...":
                    __instance.text = "서버 태그를 입력하세요...";
                    break;
                case "Name your server...":
                    __instance.text = "서버 이름을 입력하세요...";
                    break;
                case "PRIVATE means you must send invites through Steam for players to join.":
                    __instance.text = "비공개로 설정하면 Steam을 통해 플레이어에게 초대를 보내야 합니다.";
                    break;
                case "MODE: Voice activation":
                    __instance.text = "모드: 음성 감지";
                    break;
                case "Push to talk:":
                    __instance.text = "눌러서 말하기";
                    break;
                case "Gamma/Brightness:":
                    __instance.text = "감마/밝기:";
                    break;
                case "Master volume:":
                    __instance.text = "주 음량:";
                    break;
                case "Look sensitivity:":
                    __instance.text = "마우스 감도:";
                    break;
                case "Invert Y-Axis":
                    __instance.text = "Y축 반전";
                    break;
                case "Arachnophobia Mode":
                    __instance.text = "거미공포증 모드";
                    break;
                case "Discard":
                    __instance.text = "취소";
                    break;
                case "Confirm":
                    __instance.text = "확인";
                    break;
                case "> Set to defaults":
                    __instance.text = "> 기본값으로 설정";
                    break;
                case "> Reset all to default":
                    __instance.text = "> 기본값으로 재설정";
                    break;
                case "> Back":
                    __instance.text = "> 뒤로";
                    break;
                case "> Confirm changes":
                    __instance.text = "> 변경 사항 저장";
                    break;
                case "> Change keybinds":
                    __instance.text = "> 조작 키 변경";
                    break;
                case "[ Refresh ]":
                    __instance.text = "[ 새로고침 ]";
                    break;
                case "> Back to menu":
                    __instance.text = "> 메뉴로 돌아가기";
                    break;
                case "With challenge moon":
                    __instance.text = "챌린지 달 포함";
                    break;
                case "[ Back ]":
                    __instance.text = "[ 뒤로 ]";
                    break;
                case "[ Confirm ]":
                    __instance.text = "[ 확인 ]";
                    break;
                case "[ Remove my score ]":
                    __instance.text = "[ 점수 삭제하기 ]";
                    break;
                case "[ Play again ]":
                    __instance.text = "[ 다시 하기 ]";
                    break;
                case "Local-only":
                    __instance.text = "로컬 전용";
                    break;
                case "File 1":
                    __instance.text = "파일 1";
                    break;
                case "File 2":
                    __instance.text = "파일 2";
                    break;
                case "File 3":
                    __instance.text = "파일 3";
                    break;
                case "Input: ":
                    __instance.text = "입력: ";
                    break;
                case "CHALLENGE":
                    __instance.text = "챌린지";
                    break;
                case "[ Continue ]":
                    __instance.text = "[ 계속 ]";
                    break;
                case "Delete":
                    __instance.text = "삭제";
                    break;
                case "Public":
                    __instance.text = "공개";
                    break;
                case "Friends-only":
                    __instance.text = "친구 전용";
                    break;
                case "Allow remote connections":
                    __instance.text = "원격 연결 허용";
                    break;
                case "Go back":
                    __instance.text = "뒤로 가기";
                    break;
                case "File incompatible!":
                    __instance.text = "파일 호환되지 않음!";
                    break;
                case "Waiting for input":
                    __instance.text = "입력 대기 중";
                    break;

                case "HANDS FULL":
                    __instance.text = "양 손 사용 중";
                    break;
                case "Walk : [W/A/S/D]":
                    __instance.text = "걷기 : [W/A/S/D]";
                    break;
                case "Sprint: [Shift]":
                    __instance.text = "달리기: [Shift]";
                    break;
                case "Scan : [RMB]":
                    __instance.text = "스캔 : [RMB]";
                    break;
                case "SYSTEMS ONLINE":
                    __instance.text = "시스템 온라인";
                    break;
                case "Typing...":
                    __instance.text = "입력 중...";
                    break;
                case "(Some were too far to receive your message.)":
                    __instance.text = "(일부는 너무 멀어 메세지를 받지 못했습니다.)";
                    break;
                case "Confirm: [V]   |   Rotate: [R]   |   Store: [X]":
                    __instance.text = "확정: [V]   |   회전: [R]   |   보관: [X]";
                    break;
                case "CRITICAL INJURY":
                    __instance.text = "치명적인 부상";
                    break;
                case "Paycheck!":
                    __instance.text = "급여를 받았습니다!";
                    break;
                case "TOTAL:":
                    __instance.text = "합계:";
                    break;
                case "YOU ARE FIRED.":
                    __instance.text = "해고당했습니다.";
                    break;
                case "You will keep your employee rank. Your ship and credits will be reset.":
                    __instance.text = "직원 계급은 유지되지만, 당신의 함선과 크레딧은 초기화됩니다.";
                    break;
                case "You did not meet the profit quota before the deadline.":
                    __instance.text = "마감일 전까지 수익 할당량을 충족하지 못했습니다.";
                    break;
                case "Stats":
                    __instance.text = "통계";
                    break;
                case "[LIFE SUPPORT: OFFLINE]":
                    __instance.text = "[생명 신호: 오프라인]";
                    break;
                case "(Dead)":
                    __instance.text = "(사망)";
                    break;
                case "Tell autopilot ship to leave early : [RMB] (Hold)":
                    __instance.text = "함선에게 일찍 출발하라고 지시하기\n: [RMB] (Hold)";
                    break;
                case "HAZARD LEVEL:":
                    __instance.text = "       위험 수준:";
                    break;
            }
            __instance.text.Replace(" collected!", " 수집함!");
            __instance.text.Replace("Value: ", "가치: ");

            if (__instance.text.Contains("Boot Distributioner Application v0.04"))
            {
                __instance.text = "      BG IG, 시스템 행동 연합\r\n      Copyright (C) 2084-2108, Halden Electronics Inc.\r\n\r\nCPU 종류       :     BORSON 300 CPU at 2500 MHz\r\n메모리 테스트  :      4521586K OK\r\n\r\n부트 분배기 애플리케이션 v0.04\r\nCopyright (C) 2107 Distributioner\r\n    Sting X 롬 감지\r\n    웹 LNV 확장기 감지\r\n    심박수 감지 OK\r\n\r\n\r\nUTGF 장치 수신 중...\r\n\r\n신체    ID     신경     장치 클래스\r\n________________________________________\r\n\r\n2      52   Jo152       H515\r\n2      52   Sa5155      H515\r\n2      52   Bo75        H515\r\n2      52   Eri510      H515\r\n1      36   Ell567      H515\r\n1      36   Jos912      H515\r\n0\r\n";
            }else if (__instance.text.Contains("This is the weekly challenge moon. You have one day to make as much profit as possible."))
            {
                __instance.text = "주간 챌린지 달입니다. 하루 안에 가능한 한 많은 수익을 얻으세요. 원하는 만큼 다시 시도할 수 있습니다.";
            }
        }
    }
}
