using GameLogic.ServerCommands;
using HarmonyLib;
using LiquidBit.KillerQueenX;
using UnityEngine;

namespace SteamlessClientMod
{
    [HarmonyPatch(typeof(PostMatchGameMode))]
    [HarmonyPatch("ProcessLocalCommand")]
    public static class ProcessLocalCommand_Patch
    {
        public static bool Prefix(PostMatchGameMode __instance, MatchManager matchManager, MatchManager.LocalCommand localCommand, int actorNr, int inputID)    
        {
            if (localCommand == MatchManager.LocalCommand.Anim2) {
                PostMatchCommunicationCommand postMatchCommunicationCommand2 = new PostMatchCommunicationCommand();
                postMatchCommunicationCommand2.postMatchCommunication.inputID = inputID;
                GameLogic.PostMatchCommunication.Type type = GameLogic.PostMatchCommunication.Type.Dance;
                postMatchCommunicationCommand2.postMatchCommunication.type = (int)type;
                GameManager.GMInstance.networkManager.SendCommand(postMatchCommunicationCommand2.Pack(), SendCommandFlags.Reliable);
                return false;
            }
            return true;
        }

    }

}
