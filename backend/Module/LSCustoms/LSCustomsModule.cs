﻿using GTANetworkAPI;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using System;

namespace GVRP.Module.LSCustoms
{
    public class LSCustomsModule : SqlModule<LSCustomsModule, LSCustoms, uint>
    {
        ColShape LSCInteriorColshape;

        protected override string GetQuery()
        {
            return "SELECT * FROM `ls_customs`;";
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new Menu.LSCTuningSelectionBuilder());
            MenuManager.Instance.AddBuilder(new Menu.LSCVehicleSelectionMenuBuilder());
            NAPI.Task.Run(() => LSCInteriorColshape = NAPI.ColShape.CreateSphereColShape(new Vector3(-1267.12, -3013.25, -48.4902), 100.0f, 0));
        }

        // Colshape events
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            try
            {


                if (dbPlayer == null || !dbPlayer.IsValid()) return false;
                if (LSCInteriorColshape == null) return false;
                if (colShapeState == ColShapeState.Enter)
                {
                    if (LSCInteriorColshape == colShape)
                    {
                        dbPlayer.Player.TriggerEvent("loadlschangar");
                        return true;
                    }
                }
                else if (colShapeState == ColShapeState.Exit)
                {
                    if (LSCInteriorColshape == colShape)
                    {
                        dbPlayer.Player.TriggerEvent("unloadlschangar");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
            }
            return false;
        }
    }
}
