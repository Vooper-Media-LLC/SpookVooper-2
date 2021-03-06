using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;

namespace SV2.API
{
    public class EntityAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/entity/{svid}/name", GetName);
            app.MapGet   ("api/entity/{svid}/credits", GetCredits);
        }

        private static async Task GetName(HttpContext ctx, VooperDB db, string svid)
        {
            IEntity? entity = IEntity.Find(svid);
            if (entity == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {svid}");
                return;
            }

            await ctx.Response.WriteAsync(entity.Name);
        }

        private static async Task GetCredits(HttpContext ctx, VooperDB db, string svid)
        {
            IEntity? account = IEntity.Find(svid);
            if (account == null)
            {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {svid}");
                return;
            }

            await ctx.Response.WriteAsync(account.Credits.ToString());
        }

    }
}