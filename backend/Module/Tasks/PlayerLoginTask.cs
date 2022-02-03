﻿using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Clothes;
using GVRP.Module.Configurations;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Time;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace GVRP.Module.Tasks
{
    public class PlayerLoginTask : SqlResultTask
    {
        private readonly Player player;

        public PlayerLoginTask(Player player)
        {
            this.player = player;
        }

        public override string GetQuery()
        {
            return $"SELECT * FROM `player` WHERE `Name` = '{MySqlHelper.EscapeString(player.Name)}' LIMIT 1;";
        }

        public override async void OnFinished(MySqlDataReader reader)
        {
            try
            {


                if (reader.HasRows)
                {
                    //DbPlayer iPlayer = player.GetPlayer();
                    DbPlayer iPlayer = null;
                    DbPlayer dPlayer = player.GetPlayer();
                    while (reader.Read())
                    {
                        //Kayano
                        //if (player.SocialClubName == null)

                        /*if (reader.GetInt32("Pw") == 0)
                        {
                            ComponentManager.Get<TextInputBoxWindow>().Show()(dPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });
                            return;
                            //addpw(iPlayer);
                        }*/


                        //if (reader.GetInt32("pw") == 0)
                        //player.SetData("KeinPW", true);

                        if (player == null) return;
                        //Bei Warn hau wech
                        if (reader.GetInt32("warns") >= 3)
                        {
                            player.TriggerEvent("freezePlayer", true);
                            //player.Freeze(true);
                            player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                            PlayerLoginDataValidationModule.SyncUserBanToForum(reader.GetInt32("forumid"));

                            player.SendNotification($"Dein Venom (IC-)Account wurde gesperrt. Melde dich im Teamspeak!");
                            player.Kick();
                            return;
                        }

                        if (!PlayerLoginDataValidationModule.HasValidForumAccount(reader.GetInt32("forumid")))
                        {
                            player.TriggerEvent("freezePlayer", true);

                            player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                            player.Kick("Dein Forumaccount ist nicht für das Spiel freigeschaltet!");
                            return;
                        }


                        // Check Timeban
                        if (reader.GetInt32("timeban") != 0 && reader.GetInt32("timeban") > DateTime.Now.GetTimestamp())
                        {
                            player.SendNotification("Ban aktiv");
                            player.Kick("Ban aktiv");
                            return;
                        }



                        /*if (reader.GetString("Pass") == null && reader.GetString("Salt") == null)
                        {
                            ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });
                            return;
                            //addpw(iPlayer);
                        }*/

                        iPlayer = Players.Players.Instance.Load(reader, player);


                        //     iPlayer.Freezed = false;
                        iPlayer.watchDialog = 0;
                        iPlayer.Firstspawn = false;
                        iPlayer.PassAttempts = 0;
                        iPlayer.TempWanteds = 0;
                        iPlayer.PlayerPet = null;

                        iPlayer.adminObject = null;
                        iPlayer.adminObjectSpeed = 0.5f;

                        iPlayer.AccountStatus = AccountStatus.Registered;

                        iPlayer.Character = ClothModule.Instance.LoadCharacter(iPlayer);

                        await VehicleKeyHandler.Instance.LoadPlayerVehicleKeys(iPlayer);

                        //           iPlayer.SetPlayerCurrentJobSkill();
                        //iPlayer.ClearChat();

                        // Check Socialban
                        if (SocialBanHandler.Instance.IsPlayerSocialBanned(iPlayer.Player))
                        {
                            player.SendNotification("Bitte melde dich beim Support im Teamspeak (Social-Ban)");
                            player.Kick();
                            return;
                        }

                        // Check Social Name
                        if (!Configurations.Configuration.Instance.Ptr && iPlayer.SocialClubName != "" && iPlayer.SocialClubName != iPlayer.Player.SocialClubName)
                        {
                            //DBLogging.LogAcpAdminAction("System", player.Name, adminLogTypes.perm, $"Social-Club-Name-Changed DB - {iPlayer.SocialClubName} - CLIENT - {iPlayer.Player.SocialClubName}");
                            iPlayer.Player.SendNotification("Bitte melde dich beim Support im Teamspeak (Social-Name-Changed)");
                            iPlayer.Player.Kick("Bitte melde dich beim Support im Teamspeak (Social-Name-Changed)");
                            return;
                        }


                        if (Players.Players.Instance.players.ToList().Count >= Configuration.Instance.MaxPlayers)
                        {
                            iPlayer.Player.SendNotification($"Server voll! ({Configuration.Instance.MaxPlayers.ToString()})");
                            iPlayer.Player.Kick("Server voll");
                        }

                        if (reader.GetInt32("Pw") == 0)
                        {
                            iPlayer.Player.SetData("KeinPW", true);
                            //ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });
                            //return;
                            //addpw(iPlayer);
                        }

                        //            player.FreezePosition = true;

                        if (iPlayer == null) return;
                        player.TriggerEvent("setPlayerHealthRechargeMultiplier");
                        ComponentManager.Get<LoginWindow>().Show()(iPlayer);

                        if (Configuration.Instance.IsUpdateModeOn)
                        {
                            new LoginWindow().TriggerEvent(iPlayer.Player, "status", "Der Server befindet sich derzeit im Update Modus!");
                            if (iPlayer.Rank.Id < 1) iPlayer.Kick();
                        }

                        if (!Configuration.Instance.IsUpdateModeOn && iPlayer.HasData("KeinPW"))
                        {
                            new LoginWindow().TriggerEvent(iPlayer.Player, "status", "Bitte gebe dein Wunsch Passwort ein!");
                        }

                    }
                }
                else
                {
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = $"SELECT * FROM player WHERE SCName = '{MySqlHelper.EscapeString(player.SocialClubName)}';";
                        using (var reader2 = cmd.ExecuteReader())
                        {
                            if (reader2.HasRows)
                            {
                                player.Kick("Du hast bereits einen Account!");
                                conn.Close();
                                return;

                            }
                        }
                        conn.Close();
                    }
                    DbPlayer iPlayer = player.GetPlayer();


                    //ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Account Eingabe", Callback = "addacc", Message = "Gebe einen Benutzernamen ein! " });
                    TextInputBoxObject textInputBoxObject = new TextInputBoxObject
                    {
                        Title = "Anmeldeformular",
                        Message =
                        "Gebe bitte deinen Benutzernamen ein (Beispiel: Vorname_Nachname). Falls du noch nicht registriert bist, wirst du automatisch registriert.",
                        Callback = "addacc"
                    };
                    return;
                    //player.SendNotification("Sie benoetigen einen Account (Beta Zugang)! Name richtig gesetzt? Vorname_Nachname");
                    //player.Kick(
                    //    "Sie benoetigen einen Account (Beta Zugang)! Name richtig gesetzt? Vorname_Nachname " + player.SocialClubName + " " + player.Name);
                    //Logger.Debug($"Player was kicked, no Account found for {player.Name}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        [RemoteEvent]
        public void addacc(MySqlDataReader reader, Player player, string returnstring)
        {


            if (returnstring == null)
                player.Kick();

            MySQLHandler.ExecuteAsync($"INSERT INTO player (Name, SCName, ) Values({returnstring}, {player.SocialClubName})");

            //var Sha = HashThis.CreateSHA256Hash(returnstring);
            //var MDNigger = HashThis.CreateMD5Hash(returnstring);

            //MySQLHandler.ExecuteAsync($"UPDATE player SET Password = '{Sha}' WHERE id = '{iPlayer.Id}';");



            //if (reader.GetString("SCName") == player.SocialClubName)



            //var query = MySQLHandler.ExecuteAsync("SELECT * FROM player WHERE Social = @user LIMIT 1");
            player.Kick("Account erfolgreich erstellt!");

        }
    }
}