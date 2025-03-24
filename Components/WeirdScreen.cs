using LethalLevelLoader;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace LCOffice.Components
{
    public class WeirdScreen : MonoBehaviour
    {
        private readonly Dictionary<string, string> MoonMessages = new()
        {
            { "Gorgonzola", "Needs more cheese..." },
            { "Embrion", "Data has been Successfully Erased." },
            { "Titan", "Please Evacuate this area" },
            { "Icebound", "Contacting Jermey..." },
            { "Siechi", "Samouri Online" },
            { "Hyve", "Ya like Jaz?" },
            { "Calist", "Connection Terminated" },
            { "Asteroid-13", "They will find you" },
            { "Motra", "Intruders Detected" },
            { "Filitrios", "Batteries not Included" },
            { "Alcatras", "Banana?" },
            { "Fission-C", "Exterior Damage Detected\nRadiation Levels still Increasing" },
            { "Etern", "Launch Sequence Initialized" },
            { "Polarus", "Creature Detected Nearby" },
            { "Oldred", "oil oil oil oil oil oil oil\noil oil oil oil oil oil oil oil\noil oil oil oil oil oil oil oil oil oil oil oil oil oil\noil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil oil" },
            { "Trite", "Defense Turrets Online" },
            { "Demetrica", "Life Support Offline" },
            { "Cubatres", "■■■ ■■■■■■■ ■■■■■■" },
            { "Cosmocos", "I can see you." },
            { "Hyx", "You should not be here." },
            { "Summit", "Bird Activation Scheduled" },
            { "Pinnacle", "Fortitude Communications Offline" },
            { "Nadir", "Canyon Defenses Online\nIntruders Detected" },
            { "Conspire", "Clockwork Operational" },
            { "Obstruction", "Signal Received\nSecure Encrypted Connection Established" },
            { "Fortitude", "Lifeforms Detected\nAdvanced Security System Online" }
        };

        public TextMeshProUGUI screenText;

        void Start()
        {
            screenText.text = GetMessage(LevelManager.CurrentExtendedLevel);
            screenText.color = GetColor(LevelManager.CurrentExtendedLevel);
        }

        public void DisableScreen() => screenText.text = string.Empty;

        public string GetMessage(ExtendedLevel level)
        {
            if (MoonMessages.TryGetValue(level.NumberlessPlanetName, out string message)) return message;
            return "Terminal Offline";
        }

        public Color GetColor(ExtendedLevel level)
        {
            if (level.TryGetTag("Cheese") || level.TryGetTag("Honey")) return Color.yellow;
            if (level.TryGetTag("Fun")) return Color.Lerp(Color.red, Color.white, .5f);
            if (level.TryGetTag("Argon")) return Color.Lerp(Color.red, Color.blue, .5f);
            if (level.TryGetTag("Tundra")) return Color.cyan;
            if (level.TryGetTag("Ocean")) return Color.blue;
            return Color.red;
        }
    }
}
