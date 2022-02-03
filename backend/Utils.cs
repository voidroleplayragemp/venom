﻿using GTANetworkAPI;
using GVRP.Module.Players.Db;
using System;
using System.Security.Cryptography;
using System.Text;


namespace GVRP
{
    public class Utils
    {
        public static Random random = new Random();

        public static String HexConverter(Color c)
        {
            return "#" + c.Red.ToString("X2") + c.Green.ToString("X2") + c.Blue.ToString("X2");
        }

        public static Vector3 GenerateRandomPosition(Vector3 position)
        {
            bool add = random.NextDouble() >= 0.5; //Kann das nicht raus, Chris? mit random.Next(-30,31) deckst du doch schon den ganzen Bereich ab / Ja theoretisch schon
            int x = random.Next(-30, 31);
            int y = random.Next(-30, 31);

            if (add)
                position += new Vector3(x, y, 0);
            else
                position -= new Vector3(x, y, 0);

            return position;
        }

        public static Vector3 GenerateRandomPosition(Vector3 position, int range)
        {
            int rnd_x = random.Next(-range, range + 1);
            int rnd_y = random.Next(-range, range + 1);
            return position += new Vector3(rnd_x, rnd_y, 0);
        }

        public static string BuildFormatedString(string str, string additem, int amount)
        {
            string newstring = "";
            bool found = false;

            if (str != "")
            {
                string[] Items = str.Split(',');
                foreach (string item in Items)
                {
                    string[] parts = item.Split(':');
                    string vorhanden = parts[0];
                    if (vorhanden == additem)
                    {
                        //Setzt das Amount, falls ein ItemData vorhanden ist ++
                        amount = amount + Convert.ToInt32(parts[1]);

                        if (newstring == "")
                        {
                            newstring = additem + ":" + Convert.ToString(amount);
                        }
                        else
                        {
                            newstring = newstring + "," + additem + ":" + Convert.ToString(amount);
                        }
                        found = true;
                    }
                    else
                    {
                        if (newstring == "")
                        {
                            newstring = parts[0] + ":" + parts[1];
                        }
                        else
                        {
                            newstring = newstring + "," + parts[0] + ":" + parts[1];
                        }
                    }
                }
                if (!found)
                {
                    if (newstring == "")
                    {
                        newstring = additem + ":" + amount;
                    }
                    else
                    {
                        newstring = newstring + "," + additem + ":" + amount;
                    }
                }
            }
            else
            {
                newstring = additem + ":" + amount;
            }

            return newstring;
        }

        public static double GetDistance(Vector3 pos1, Vector3 pos2)
        {
            return pos1.DistanceTo(pos2);
        }

        public static bool IsPointNearPoint(float range, Vector3 pos1, Vector3 pos2)
        {
            return GetDistance(pos1, pos2) <= range;
        }

        public static bool CheckPlayerName(string name)
        {
            System.Text.RegularExpressions.Regex pattern =
                new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9 ._]+$");
            return pattern.IsMatch(name.Trim());
        }

        public static int GetTicketPrice(int wantedpoints)
        {
            int defaultprice = 50;

            int ticketprice = defaultprice;

            for (int i = 0; i < wantedpoints; i++)
            {
                ticketprice = ticketprice + (defaultprice * wantedpoints);
            }

            return ticketprice;
        }

        public static int RandomNumber(int min, int max)
        {
            return random.Next(min, max + 1);
        }
        public static double RandomDoubleNumber(int min, int max)
        {
            return min + (random.NextDouble() * (max - min));
        }
        public static int GeneratePassword(DbPlayer dbPlayer)
        {
            return (((int)dbPlayer.Id % 2345) + 1234) << 4;
        }
    }



    public static class HashThis
    {
        public static string GetSha256Hash(string password)
        {
            SHA256Managed crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto =
                crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static string CreateMD5Hash(string input)
        {
            // Create MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // @Kayano
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /*public static string CreateSHA256Hash(string input)
        {
            //Erstellt sozusagen den Sha hash
            //using SHA256 sha256Hash = SHA256.Create();
            SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder stringbuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                stringbuilder.Append(bytes[i].ToString("x2"));
            }
        }*/

    }
}