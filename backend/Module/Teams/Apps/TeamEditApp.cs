﻿using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;

namespace GVRP.Module.Teams.Apps
{
    public class TeamEditApp : SimpleApp
    {
        public TeamEditApp() : base("TeamEditApp")
        {
        }

        [RemoteEvent]
        public void leaveTeam(Player player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.SetTeam((uint)TeamList.Zivilist);
        }
    }
}