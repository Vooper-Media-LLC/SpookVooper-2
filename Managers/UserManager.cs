using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace SV2.Managers;

public static class UserManager
{
    static List<String> LoginCodes = new();
    static Dictionary<String, String> SessionIdsToSvids = new();

    public static User? GetUser(HttpContext ctx)
    {
        string? d = null;
        ctx.Request.Cookies.TryGetValue("svid", out d);
        if (d is null) {
            return null;
        }
        return DBCache.Get<User>(d!);
    }
    
    public static void AddLogin(string code, string id)
    {
        SessionIdsToSvids.Add(code, id);
    }

    public static string? GetSvidFromSession(HttpContext ctx)
    {
        string? svid = "";
        SessionIdsToSvids.Remove(ctx.Session.GetString("code"), out svid);
        return svid;
    }

    public static string GetCode(HttpContext ctx)
    {
        string code = Guid.NewGuid().ToString();
        ctx.Session.SetString("code", code);
        return code;
    }
}