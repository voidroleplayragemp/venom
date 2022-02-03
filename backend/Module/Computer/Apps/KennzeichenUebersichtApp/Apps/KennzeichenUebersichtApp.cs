﻿using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.LeitstellenPhone;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using System;
using System.Threading.Tasks;

namespace GVRP.Module.Computer.Apps.KennzeichenUebersichtApp.Apps
{
    public class KennzeichenUebersichtApp : SimpleApp
    {
        public KennzeichenUebersichtApp() : base("KennzeichenUebersichtApp") { }

        public enum SearchType
        {
            PLATE = 0,
            VEHICLEID = 1
        }

        [RemoteEvent]
        public async Task requestVehicleOverviewByPlate(Player client, String plate)
        {
            await HandleVehicleOverview(client, plate, SearchType.PLATE);
        }

        [RemoteEvent]
        public async Task requestVehicleOverviewByVehicleId(Player client, int vehicleId)
        {
            await HandleVehicleOverview(client, vehicleId.ToString(), SearchType.VEHICLEID);

        }


        private async Task HandleVehicleOverview(Player p_Client, String information, SearchType type)
        {
            DbPlayer p_DbPlayer = p_Client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            if (LeitstellenPhoneModule.Instance.GetByAcceptor(p_DbPlayer) == null)
            {
                p_DbPlayer.SendNewNotification("Sie müssen als Leitstelle angemeldet sein", PlayerNotification.NotificationType.ERROR);
                return;
            }

            var l_Overview = KennzeichenUebersichtFunctions.GetVehicleInfoByPlateOrId(p_DbPlayer, type, information);
            TriggerEvent(p_Client, "responsePlateOverview", NAPI.Util.ToJson(l_Overview));
        }


    }
}
