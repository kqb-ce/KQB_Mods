using GameLogic;
using GameLogic.ClientCommands;
using GameLogic.ServerCommands;
using GameLogic.Stats;
using HarmonyLib;
using HarmonyLib.Public.Patching;
using System.Drawing;
using System.Reflection;
using static GameLogic.GameServer;

namespace LosersCanDance
{
    [HarmonyPatch]
    public class Patch_ProcessServerCommand
    {
        static MethodBase TargetMethod()
        {
            // Find the internal class and method by name
            var type = AccessTools.TypeByName("GameLogic.PostMatchGameMode");
            return AccessTools.Method(type, "ProcessServerCommand");
        }

        static bool Prefix(ref Game game,ref ServerCommand command, GameLogic.Actor actor)
        {
            UnityEngine.Debug.Log("Running");
            if (command.type == ServerCommand.Type.PostMatchCommunication)
            {
                GameLogic.ServerCommands.PostMatchCommunicationCommand command2 = (GameLogic.ServerCommands.PostMatchCommunicationCommand)command.command;
                if (game.gameState.GetPlayer(actor.actorNr, command2.postMatchCommunication.inputID) == null)
                    return true;

                //get the actors team
                Team.Color actorTeamColor = Team.Color.None;
                foreach (PlayerMatchStats stats in game.statTracker.playerMatchStats)
                {
                    if (stats.actorNr == actor.actorNr)
                    {
                        actorTeamColor = stats.team;
                    }
                }

                //find the winning team
                int blueWins = 0;
                int redWins = 0;

                foreach (Team.Color color in game.statTracker.gameWinners)
                {
                    if (color == Team.Color.Red)
                    {
                        redWins += 1;
                    }
                    if (color == Team.Color.Blue)
                    {
                        blueWins += 1;
                    }
                }
                if (actorTeamColor == Team.Color.Blue && (blueWins > redWins))
                {
                    return true;
                }
                if (actorTeamColor == Team.Color.Red && (redWins > blueWins))
                {
                    return true;
                }
                else
                {
                    ClientCommand command3 = new ClientCommand(-1, ClientCommand.Type.PostMatchCommunicationCommand, (object)new GameLogic.ClientCommands.PostMatchCommunicationCommand()
                    {
                        command = {
                      inputID = command2.postMatchCommunication.inputID,
                      actorNr = actor.actorNr,
                      type = (int)PostMatchCommunication.Type.Clap
                    }
                    });
                    game.gameState.AddClientCommand(command3);
                    return false;
                }
            }
            return true;

            }
    }

    [HarmonyPatch(typeof(PostMatchGameMode))]
    [HarmonyPatch("HandlePostMatchCommunication")]
    public static class ProcessLocalCommand_Patch
    {
        public static bool Prefix(PostMatchGameMode __instance, int inputID, int actorNr, ref PostMatchCommunication.Type type, ref PostMatchUI ___postMatchUI, ref Team ___winningTeam)
        {
            type = PostMatchCommunication.Type.Clap;
            PostMatchUI_Results_PlayerStats playerStats = ___postMatchUI.GetPlayerStats(inputID, actorNr);
            if ((UnityEngine.Object)playerStats != (UnityEngine.Object)null && playerStats.teamColor == ___winningTeam.color)
                type = PostMatchCommunication.Type.Dance;
            return true;
        }

    }

}
