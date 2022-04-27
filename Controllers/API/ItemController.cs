using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.API
{
    public class ItemAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/item/{itemid}", GetItem);
            app.MapGet   ("api/item/{itemid}/give", Give);
            app.MapGet   ("api/item/{itemid}/owner", GetOwner);
            app.MapGet   ("api/definition/{definitionid}/items", GetItemsFromDefinition);
        }

        private static async Task GetItemsFromDefinition(HttpContext ctx, VooperDB db, string definitionid)
        {
            IEnumerable<TradeItem> definitions = DBCache.GetAll<TradeItem>().Where(x => x.Definition_Id == definitionid);

            await ctx.Response.WriteAsJsonAsync(definitions);
        }

        private static async Task GetItem(HttpContext ctx, VooperDB db, string itemid)
        {
            // find Item
            TradeItem? item = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            await ctx.Response.WriteAsJsonAsync(item);
        }

        private static async Task GetOwner(HttpContext ctx, VooperDB db, string itemid)
        {
            // find Item
            TradeItem? item = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            await ctx.Response.WriteAsJsonAsync(item.Owner);
        }

        private static async Task Give(HttpContext ctx, VooperDB db, string itemid, string apikey, string fromid, string toid, int amount)
        {
            // find Item
            TradeItem? item = DBCache.GetAll<TradeItem>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            // get Entity with the api key
            IEntity? entity = IEntity.FindByApiKey(apikey);

            if (entity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Invalid api key!");
                return;
            }

            IEntity? fromentity = IEntity.Find(fromid);
            if (fromentity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {fromid}!");
                return;
            }

            if (fromentity.Id != entity.Id) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"The apikey must be the same as the owner of this item's!");
                return;
            }

            // check if entity is the owner of the item
            // TODO: add checking for oauth key
            
            if (fromentity.Id != item.OwnerId) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"You do not own this item!");
                return;
            }

            // find toentity

            IEntity? toentity = IEntity.Find(toid);

            if (entity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {toid}!");
                return;
            }

            if (amount <= 0) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Amount must be greater than 0!");
                return;
            }

            await item.Give(toentity!, amount);

            await ctx.Response.WriteAsync($"Successfully gave {amount} of {item.Definition.Name} to {toentity!.Name}.");
        }
    }
}