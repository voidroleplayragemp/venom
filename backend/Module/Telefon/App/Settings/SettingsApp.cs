﻿using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Voice;

namespace GVRP.Module.Telefon.App
{
    public class SettingsApp : SimpleApp
    {
        public SettingsApp() : base("SettingsApp")
        {
        }

        [RemoteEvent]
        public void requestPhoneSettings(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            TriggerEvent(player, "responsePhoneSettings", dbPlayer.phoneSetting.flugmodus, dbPlayer.phoneSetting.lautlos, dbPlayer.phoneSetting.blockCalls);
        }

        [RemoteEvent]
        public void savePhoneSettings(Player player, bool flugmodus, bool lautlos, bool blockCalls)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.phoneSetting.flugmodus = flugmodus;
            dbPlayer.phoneSetting.lautlos = lautlos;
            dbPlayer.phoneSetting.blockCalls = blockCalls;

            if (flugmodus)
            {
                VoiceModule.Instance.ChangeFrequenz(dbPlayer, 0, true);
                VoiceModule.Instance.turnOffFunk(dbPlayer);
                TriggerEvent(player, "responseApps", "[{\"id\":\"GpsApp\",\"name\":\"GPS\",\"icon\": \"GpsApp.png\"}, {\"id\":\"ContactsApp\",\"name\":\"Kontakte\",\"icon\": \"ContactsApp.png\"}, {\"id\":\"ProfileApp\",\"name\":\"Profil\",\"icon\": \"ProfilApp.png\"}, {\"id\":\"SettingsApp\",\"name\":\"Settings\",\"icon\": \"SettingsApp.png\"}, {\"id\":\"CalculatorApp\",\"name\":\"Rechner\",\"icon\": \"CalculatorApp.png\"}, {\"id\":\"ServiceRequestApp\",\"name\":\"Services\",\"icon\": \"ServiceApp.png\"}]");

            }
        }
    }
}
