﻿using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Players;

namespace GVRP.Module.FIB
{
    public class FIBEvents : Script
    {
        public static DiscordHandler Discord = new DiscordHandler();

        [RemoteEvent]
        public void FIBSetUnderCoverName(Player player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (!returnstring.Contains("_") || returnstring.Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }

            string[] ucName = returnstring.Split("_");

            if (ucName.Length < 2 || ucName[0].Length < 3 || ucName[1].Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }

            dbPlayer.SendNewNotification($"Sie sind nun als {ucName[0]}_{ucName[1]} im Undercover dienst!");
            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} ist nun als {ucName[0]}_{ucName[1]} im Undercover dienst!", 5000, 10);
            Discord.SendMessage(dbPlayer.GetName() + $" hat den Undercover Dienst als {ucName[0]}_{ucName[1]} betreten.", "FIB-LOG", DiscordHandler.Channels.FIB);
            dbPlayer.SetUndercover(ucName[0], ucName[1]);
            return;
        }
    }
}
