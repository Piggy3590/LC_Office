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
using UnityEngine.UI;

namespace LCKorean.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("DisplayMenuNotification")]
        private static void DisplayMenuNotification_Prefix(string notificationText, string buttonText,
            ref GameObject ___menuNotification, ref TextMeshProUGUI ___menuNotificationText, ref TextMeshProUGUI ___menuNotificationButtonText)
        {
            if (notificationText.Contains("Some of your save files may not be compatible"))
            {
                Debug.Log("Displaying menu notification: " + $"일부 저장 파일은 버전 {GameNetworkManager.Instance.compatibleFileCutoffVersion}과 호환되지 않을 수 있으며, 플레이할 경우 손상될 수 있습니다.");
                ___menuNotificationText.text = $"일부 저장 파일은 버전 {GameNetworkManager.Instance.compatibleFileCutoffVersion}과 호환되지 않을 수 있으며, 플레이할 경우 손상될 수 있습니다.";
                ___menuNotificationButtonText.text = "[ 닫기 ]";
                ___menuNotification.SetActive(value: true);
                EventSystem.current.SetSelectedGameObject(___menuNotification.GetComponentInChildren<Button>().gameObject);
            }
            return;
        }
        [HarmonyPostfix]
        [HarmonyPatch("HostSetLobbyPublic")]
        private static void HostSetLobbyPublic_Postfix(ref TextMeshProUGUI ___privatePublicDescription)
        {
            if (___privatePublicDescription.text.Contains("PUBLIC"))
            {
                ___privatePublicDescription.text = "공개로 설정하면 모든 사람이 볼 수 있도록 서버가 서버 목록에 표시됩니다.";
            }else
            {
                ___privatePublicDescription.text = "비공개로 설정하면 Steam을 통해 플레이어에게 초대를 보내야 합니다.";
            }
        }
    }
}
