using HarmonyLib;
using LCOffice.Components;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(LungProp))]
    public class LungPropPatch
    {
        [HarmonyPatch("EquipItem")]
        [HarmonyPrefix]
        public static void patchEquipItem(LungProp __instance)
        {
            if (ElevatorSystem.System != null && ElevatorSystem.System.inserted == __instance)
            {
                ElevatorSystem.System.PullLungServerRpc();
            }
        }
    }
}
