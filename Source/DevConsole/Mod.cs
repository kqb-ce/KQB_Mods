using HarmonyLib;


namespace DevConsole
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("Update")]
    public static class GPUpdate_Patch
    {
        public static bool Prefix(GameManager __instance)
        {
            AccessTools.Method(typeof(GameManager), "CheckDevConInput").Invoke(__instance, new object[] { });
            return true;
        }

    }
}
