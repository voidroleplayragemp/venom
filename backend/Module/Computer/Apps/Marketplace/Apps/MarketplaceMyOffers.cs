﻿using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.Marketplace;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using static GVRP.Module.Computer.Apps.MarketplaceApp.MarketplaceCategoryApp;

namespace GVRP.Module.Computer.Apps.MarketplaceApp
{
    public class MarketplaceMyOffersApp : SimpleApp
    {
        public MarketplaceMyOffersApp() : base("MarketplaceMyOffers") { }

        [RemoteEvent]
        public async void deleteOffer(Player client, int offerId)
        {

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MarketplaceOfferModule.Instance.OfferObjects.ContainsKey(offerId)) return;

            OfferObject offerObject = MarketplaceOfferModule.Instance.OfferObjects[offerId];

            MySQLHandler.ExecuteAsync($"DELETE FROM `marketplace_offers` WHERE player_id = '{dbPlayer.Id}' AND name = '{offerObject.name}' AND category_id = '{offerObject.CategoryId}'");

            MarketplaceOfferModule.Instance.OfferObjects.Remove(offerId);

            dbPlayer.SendNewNotification("Angebot gelöscht.");

        }

        [RemoteEvent]
        public async void requestMyOffers(Player client)
        {

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var offerList = new List<OfferObject>();
            var offers = MarketplaceOfferModule.Instance.OfferObjects.Where(x => x.Value.phone == dbPlayer.handy[0]);

            foreach (KeyValuePair<int, OfferObject> item in offers)
            {
                offerList.Add(item.Value);
            }
            var offerJson = NAPI.Util.ToJson(offerList);
            TriggerEvent(client, "responseMyOffers", offerJson);
            Console.WriteLine(offerJson);

        }
    }
}
