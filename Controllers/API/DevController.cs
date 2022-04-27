using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using SV2.Web;
using SV2.Database.Models.Economy;
using Microsoft.EntityFrameworkCore;

namespace SV2.API
{
    public class DevAPI : BaseAPI
    {
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/dev/database/sql", GetSQL);
        }

        private static async Task GetSQL(HttpContext ctx, VooperDB db, bool drop = false)
        {
            if (drop && false) {
                VooperDB.Instance.Database.EnsureDeleted();
                VooperDB.Instance.Database.EnsureCreated();
                await VooperDB.Instance.SaveChangesAsync();
            }
            await ctx.Response.WriteAsync(VooperDB.GenerateSQL());
        }
    }
}