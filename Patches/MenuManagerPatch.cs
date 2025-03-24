using HarmonyLib;
using LCOffice.Components;

namespace LCOffice.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void patchStart()
        {
            ElevatorSystem.Despawn();
        }
    }
}
