﻿using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Gangwar;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using System.Threading.Tasks;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> UnderArmor(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Player.IsInVehicle) return true;

            Chats.sendProgressBar(iPlayer, 4000);
            iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            iPlayer.SetCannotInteract(true);
            await Task.Delay(4000);
            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            iPlayer.SetCannotInteract(false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            iPlayer.SetArmor(90, true);

            return true;
        }

        public static async Task<bool> Armor(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Player.IsInVehicle) return true;

            Chats.sendProgressBar(iPlayer, 4000);
            iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            iPlayer.SetCannotInteract(true);
            await Task.Delay(4000);
            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            iPlayer.SetCannotInteract(false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            if (iPlayer.VisibleArmorType != 0)
                iPlayer.SaveArmorType(1);
            iPlayer.SetArmor(100);

            return true;
        }

        public static async Task<bool> BArmor(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Player.IsInVehicle || !iPlayer.IsACop()) return true;

            Chats.sendProgressBar(iPlayer, 4000);
            iPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            iPlayer.SetCannotInteract(true);
            await Task.Delay(4000);
            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            iPlayer.SetCannotInteract(false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            iPlayer.SetArmor(100, true);

            return true;
        }

        public static async Task<bool> FArmor(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Player.IsInVehicle) return true;
            //if (!iPlayer.Team.IsInTeamfight()) return false;
            if (!GangwarTownModule.Instance.IsTeamInGangwar(iPlayer.Team)) return false;

            Chats.sendProgressBar(iPlayer, 4000);
            iPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            iPlayer.SetCannotInteract(true);
            await Task.Delay(4000);
            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            iPlayer.SetCannotInteract(false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            if (iPlayer.VisibleArmorType != 0)
                iPlayer.SaveArmorType(30);
            iPlayer.VisibleArmorType = 30;
            iPlayer.SetArmor(120, true);

            return true;
        }
    }
}