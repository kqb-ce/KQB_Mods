using HarmonyLib;
using LiquidBit.KillerQueenX.Utility.Sanitizers;
using System.Text.RegularExpressions;

namespace StripName
{
    [HarmonyPatch(typeof(PcStringSanitizer))]
    [HarmonyPatch("SanitizeString")]
    public static class SetText_Patch
    {

        public static bool Prefix(ref string inText)
        {
            if (inText != null && inText.Length > 0)
            {
                inText = Regex.Replace(inText, "<.*?>", string.Empty);
            }
            return true;
        }
    }
}
