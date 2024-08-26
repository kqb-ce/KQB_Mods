using HarmonyLib;
using LiquidBit.KillerQueenX;
using Steamworks;
namespace FatLobbies
{
    [HarmonyPatch(typeof(SteamLobbyManager))]
    [HarmonyPatch("CreateLobby")]
    public static class CreateLobby_Patch
    {
        public static bool Prefix(ELobbyType lobbyType, ref int maxMembers)
        {
            maxMembers = 12;
            return true;
        }

    }

}