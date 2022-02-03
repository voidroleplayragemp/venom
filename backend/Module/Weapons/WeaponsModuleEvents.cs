﻿using GTANetworkAPI;
using GVRP.Module.Players;

namespace GVRP.Module.Weapons
{
    public class WeaponsModuleEvents : Script
    {
        [RemoteEvent]
        public void getWeaponAmmoAnswer(Player p_Player, int p_WeaponID, int p_Ammo)
        {
            var l_Player = p_Player.GetPlayer();

            foreach (var l_Detail in l_Player.Weapons)
            {
                if (l_Detail.WeaponDataId != p_WeaponID)
                    continue;

                l_Detail.Ammo = p_Ammo;
                break;
            }
        }
    }
}
