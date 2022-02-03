﻿using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Injury;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.Vehicles;
using GVRP.Module.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GVRP.Module.Sync
{
    public class AsyncThread
    {
        private static Thread m_AsyncThread;
        private static List<Task> m_AsyncTasks = new List<Task>();
        private static object l_Lock = new object();

        public AsyncThread()
        {
            m_AsyncThread = new Thread(new ThreadStart(StartAsyncThread));
            m_AsyncThread.Start();
        }

        private void StartAsyncThread()
        {
            m_AsyncThread.IsBackground = true;
            m_AsyncThread.Priority = ThreadPriority.BelowNormal;

            AppDomain l_CurrentDomain = AppDomain.CurrentDomain;
            l_CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);

            while (true)
            {
                try
                {
                    var l_List = new List<Task>();
                    lock (l_Lock)
                    {
                        l_List = m_AsyncTasks.ToList();
                        m_AsyncTasks.Clear();
                    }

                    foreach (var l_Task in l_List)
                        l_Task.RunSynchronously();
                }
                catch (Exception e)
                {
                    Logger.Print(e.ToString());
                }
            }
        }

        private void ExceptionHandler(object p_Sender, UnhandledExceptionEventArgs p_Args)
        {
            Exception l_E = (Exception)p_Args.ExceptionObject;
            Logger.Crash(l_E);
        }

        public void AddToAsyncThread(Task l_Task)
        {
            lock (l_Lock)
            {
                m_AsyncTasks.Add(l_Task);
            }
        }
    }

    public class SyncThread
    {
        private static SyncThread _instance;

        public static SyncThread Instance => _instance ?? (_instance = new SyncThread());

        private static DateTime lastMinCheck = DateTime.Now;

        public class VehicleWorker
        {
            public static void UpdateVehicleData()
            {
                foreach (SxVehicle sxVeh in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (sxVeh == null || !sxVeh.IsValid()) return;

                    if (sxVeh == null || sxVeh.entity == null) continue;

                    var occupants = sxVeh.entity.Occupants;
                    var occupants2 = Players.Players.Instance.FindPlayer(occupants, true);

                    if (!sxVeh.respawnInteractionState && occupants.Count <= 0 &&
                        !sxVeh.entity.HasData("isLoaded") && sxVeh.jobid > 0)
                    {
                        sxVeh.respawnInterval++;
                        if (sxVeh.respawnInterval >= 10 && sxVeh.jobid > 0) // 10 min
                        {
                            CheckDeletion(sxVeh);
                        }
                        else if (sxVeh.respawnInterval >= 3 && sxVeh.IsPlayerVehicle())
                        {
                            CheckDeletion(sxVeh);
                        }
                        else if (sxVeh.jobid > 0) // Nur Jobfahrzeuge resetten
                        {
                            sxVeh.respawnInterval = 0;
                            sxVeh.respawnInteractionState = false;
                        }
                    }

                    if (sxVeh.SyncExtension.EngineOn)
                    {
                        //Wichtig @Kayano
                        //DbPlayer player = occupants.GetPlayer();

                        var driver = occupants2.Player.VehicleSeat == 0;
                        if (!driver)
                        {
                            sxVeh.fuel -= sxVeh.Data.FuelConsumption / 100.0;
                            if (sxVeh.fuel <= 0)
                            {
                                sxVeh.fuel = 0;
                                sxVeh.SyncExtension.SetEngineStatus(false);
                            }
                        }
                    }
                }
            }

            private static void CheckDeletion(SxVehicle sxVeh)
            {
                try
                {
                    if (sxVeh == null || !sxVeh.IsValid()) return;

                    if (sxVeh.IsPlayerVehicle())
                        sxVeh.SetPrivateCarGarage(1);
                    else if (sxVeh.IsTeamVehicle())
                        sxVeh.SetTeamCarGarage(true);
                    else
                        VehicleHandler.Instance.DeleteVehicle(sxVeh);
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"Failure in RemoveVehiclesSync: " + e.Message);
                }
            }

            public static void AntiFlightSystemIsland()
            {
                foreach (var vehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (!vehicle.IsInAntiFlight()) continue;

                    vehicle.entity.Locked = true;
                    vehicle.entity.SetData("Door_KRaum", 0);

                    vehicle.SyncExtension.SetEngineStatus(false);
                    vehicle.entity.Locked = true;
                    vehicle.entity.SetData("EMPWarning", 0);
                    vehicle.entity.ResetData("EMPWarning");

                    foreach (var player in vehicle.entity.Occupants)
                    {
                        if (player == null) continue;
                        //var dbPlayer = player.GetPlayer();
                        var dbPlayer = Players.Players.Instance.FindPlayer(player, true);
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        dbPlayer.SendNewNotification("FLUGABWEHR - EMP | SIE BETRETEN EINE SPERRZONE!", notificationType: PlayerNotification.NotificationType.ERROR);
                    }
                }
            }
        }

        private class SystemMinWorkers
        {
            public static void CheckMin()
            {
                if (lastMinCheck.AddSeconds(50) <= DateTime.Now)
                {
                    lastMinCheck = DateTime.Now;

                    try
                    {
                        Modules.Instance.OnPlayerMinuteUpdate();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    try
                    {
                        Modules.Instance.OnMinuteUpdate();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }
            }

            public static async Task CheckMinAsync()
            {
                try
                {
                    await Modules.Instance.OnMinuteUpdateAsync();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckTwoMin()
            {
                try
                {
                    Modules.Instance.OnTwoMinutesUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckTenSec()
            {
                try
                {
                    Modules.Instance.OnTenSecUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static async Task CheckTenSecAsync()
            {
                try
                {
                    await Modules.Instance.OnTenSecUpdateAsync();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckFiveSec()
            {
                try
                {
                    Modules.Instance.OnFiveSecUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckFiveMin()
            {
                try
                {
                    Modules.Instance.OnFiveMinuteUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            public static void CheckFifteenMin()
            {
                try
                {
                    Modules.Instance.OnFifteenMinuteUpdate();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
        }

        public class PlayerWorker
        {
            private const int RpMultiplikator = 4;

            public static readonly Random Rnd = new Random();

            public static void ChangeDbPositions()
            {
                foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (iPlayer == null || !iPlayer.IsValid()) return;

                    if (iPlayer == null || !iPlayer.IsValid() || iPlayer.Player == null) continue;

                    // Jumppoints unfreeze
                    if (iPlayer.FreezedUntil != null && iPlayer.FreezedUntil < DateTime.Now)
                    {
                        iPlayer.FreezedUntil = null;
                    }
                    if (!iPlayer.MetaData.SaveBlocked)
                    {
                        if (iPlayer.HasData("lastPosition"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("lastPosition");
                        }
                        else if (iPlayer.HasData("CamperEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("CamperEnterPos");
                        }
                        else if (iPlayer.HasData("AirforceEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("AirforceEnterPos");
                        }
                        else if (iPlayer.HasData("SubmarineEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("SubmarineEnterPos");
                        }
                        else if (iPlayer.HasData("NSAEnterPos"))
                        {
                            iPlayer.MetaData.Position = iPlayer.GetData("NSAEnterPos");
                        }
                        else if (iPlayer.Player.Dimension == 0 && iPlayer.DimensionType[0] != DimensionType.House)
                        {
                            iPlayer.MetaData.Dimension = iPlayer.Player.Dimension;
                            iPlayer.MetaData.Heading = iPlayer.Player.Heading;
                            iPlayer.MetaData.Position = iPlayer.Player.Position;
                        }
                        iPlayer.MetaData.Armor = iPlayer.Player.Armor;
                        iPlayer.MetaData.Health = iPlayer.Player.Health;
                    }
                }
            }

            public static void DoAnimations()
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                    {
                        if (iPlayer == null || !iPlayer.IsValid()) continue;

                        if (!iPlayer.AnimationScenario.Active) continue;
                        if (!iPlayer.AnimationScenario.Repeat &&
                            iPlayer.AnimationScenario.StartTime.AddSeconds(iPlayer.AnimationScenario.Lifetime) <
                            DateTime.Now)
                        {
                            iPlayer.StopAnimation();
                        }

                        // to far from packed player
                        if (iPlayer.HasData("follow"))
                        {
                            DbPlayer followedPlayer = Players.Players.Instance.FindPlayer(iPlayer.GetData("follow"));
                            if (followedPlayer != null && followedPlayer.IsValid() && !followedPlayer.isInjured())
                            {
                                if (followedPlayer.Player.Position.DistanceTo(iPlayer.Player.Position) > 10.0f)
                                {
                                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
                                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                                    await Task.Delay(2000);
                                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                                }
                            }
                        }
                    }
                }));
            }

            public static void AntiCheatCheck()
            {
                foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (iPlayer == null || !iPlayer.IsValid()) continue;

                    WeaponsModule.Instance.CheckPlayerDisableDrivebyWeapons(iPlayer);
                }
            }
        }


        public SyncThread()
        {
        }

        public static void Init()
        {
            _instance = new SyncThread();
        }

        public async Task Start()
        {
            /*
             * DO ColShapes
             * 4700 MS
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        PlayerWorker.DoAnimations();
                        VehicleWorker.AntiFlightSystemIsland();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(4700);
                }
            }, TaskCreationOptions.LongRunning);


            /*
             * DO 
             * 5000 MS
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckFiveSec();
                        PlayerWorker.ChangeDbPositions();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(5000);
                }
            }, TaskCreationOptions.LongRunning);

            /*
             * DO 
             * 10000 MS
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        await SystemMinWorkers.CheckTenSecAsync();
                        SystemMinWorkers.CheckTenSec();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(10000);
                }
            }, TaskCreationOptions.LongRunning);

            /*
             * UPDATING Server messages
             * 1 Minute
             */
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        await SystemMinWorkers.CheckMinAsync();
                        //VehicleWorker.UpdateVehicleData(); Funktioniert zur Zeit eh nicht
                        await Main.OnUpdateHandler();
                        await Main.OnMinHandler();
                        SystemMinWorkers.CheckMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(60000);
                }
            }, TaskCreationOptions.LongRunning);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckTwoMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(120000);
                }
            }, TaskCreationOptions.LongRunning);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckFiveMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(300000);
                }
            }, TaskCreationOptions.LongRunning);


            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SchwarzgeldModule.Instance.SchwarzgeldContainerCheck();
                        //       MeertraeubelModul.Instance.UpdateMeertraubelContainer();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(300000);
                }
            }, TaskCreationOptions.LongRunning);

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        SystemMinWorkers.CheckFifteenMin();
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    await Task.Delay(900000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}