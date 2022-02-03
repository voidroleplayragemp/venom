﻿using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Players.Db;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GVRP.Module.Node
{
    public static class NodeExtension
    {
        public static void Call(this Player player, NodeCall nodeCall)
        {
            Request.CallEntity(JsonConvert.SerializeObject(new NodeArg("player", player.Handle.Value)), nodeCall.Name, nodeCall.Args);
        }

        public static void Call(this Vehicle vehicle, NodeCall nodeCall)
        {
            Request.CallEntity(JsonConvert.SerializeObject(new NodeArg("vehicle", vehicle.Handle.Value)), nodeCall.Name, nodeCall.Args);
        }

        public static void Call(this DbPlayer player, NodeCall nodeCall)
        {
            Request.CallEntity(JsonConvert.SerializeObject(new NodeArg("player", player.Player.Handle.Value)), nodeCall.Name, nodeCall.Args);
        }

        public static void Call(this SxVehicle vehicle, NodeCall nodeCall)
        {
            Request.CallEntity(JsonConvert.SerializeObject(new NodeArg("vehicle", vehicle.entity.Handle.Value)), nodeCall.Name, nodeCall.Args);
        }

        public static void Set(this Player player, string name, object value)
        {
            Request.SetEntity(JsonConvert.SerializeObject(new NodeArg("player", player.Handle.Value)), name, JsonConvert.SerializeObject(new List<NodeArg>() { new NodeArg("", value) }));
        }

        public static void Set(this Vehicle vehicle, string name, object value)
        {
            Request.SetEntity(JsonConvert.SerializeObject(new NodeArg("vehicle", vehicle.Handle.Value)), name, JsonConvert.SerializeObject(new List<NodeArg>() { new NodeArg("", value) }));
        }

        public static void Set(this DbPlayer player, string name, object value)
        {
            Request.SetEntity(JsonConvert.SerializeObject(new NodeArg("player", player.Player.Handle.Value)), name, JsonConvert.SerializeObject(new List<NodeArg>() { new NodeArg("", value) }));
        }

        public static void Set(this SxVehicle vehicle, string name, object value)
        {
            Request.SetEntity(JsonConvert.SerializeObject(new NodeArg("vehicle", vehicle.entity.Handle.Value)), name, JsonConvert.SerializeObject(new List<NodeArg>() { new NodeArg("", value) }));
        }
    }
}
