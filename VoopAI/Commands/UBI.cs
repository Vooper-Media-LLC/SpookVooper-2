using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Items.Messages;
using Valour.Shared.Items.Messages.Embeds;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Districts;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using SV2.Database.Models.Entities;
using System.Linq;
using SV2.Web;

namespace SV2.VoopAI.Commands;

class UBICommands : CommandModuleBase
{
    public static string GetRankColor(Rank? rank)
    {
        if (rank is null) {
            return "ffffff";
        }
        switch(rank)
        {
            case Rank.Spleen:
                return "414aff";
            case Rank.Crab:
                return "e05151";
            case Rank.Gaty:
                return "00ff23";
            case Rank.Corgi:
                return "b400ff";
            case Rank.Oof:
                return "f1ff00";
            case Rank.Unranked:
                return "ffffff";
        }
        return "ffffff";
    }

    [Command("changeubi")]
    public async Task ChangeUBI(CommandContext ctx) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        await ChangeUBI(ctx, null);
    }

    [Interaction("")]
    public async Task ChangeUBIInteraction(InteractionContext ctx)
    {
        // Element_Id is the id of the button that was clicked
        string EventId = ctx.Event.Element_Id;
        if (EventId.Contains("ChangeUBI")) {
            await ctx.ReplyAsync(ctx.Event.ToString());
        }
    }

    public async Task ChangeUBI(CommandContext ctx, string? districtid)
    {        
        EmbedBuilder builder = new();
        EmbedPageBuilder page = new EmbedPageBuilder();
        string name = "";
        IEntity? entity = DBCache.FindEntity(districtid);
        if (entity is not null) {
            name = entity.Name;
        }
        else {
            name = "Vooperia";
        }
        UBIPolicy? policy = null;
        page.AddText("", $"UBI For {name}");
        foreach (Rank rank in Enum.GetValues<Rank>()) {
            policy = DBCache.GetAll<UBIPolicy>().FirstOrDefault(x => x.DistrictId == districtid && x.ApplicableRank == rank);
            if (policy is null) {
                policy = new() {Rate = 0.0m};
            }
            page.AddText(text:"&nbsp;");
            page.AddText(null, rank.ToString(), textColor:GetRankColor(rank));
            page.AddInputBox(policy.Rate.ToString(), rank.ToString(), GetRankColor(rank), rank.ToString());
        }
        policy = DBCache.GetAll<UBIPolicy>().FirstOrDefault(x => x.DistrictId == districtid && x.ApplicableRank == null);
        if (policy is null) {
            policy = new() {Rate = 0.0m};
        }
        page.AddText(text:"&nbsp;");
        page.AddText(null, "Everyone");
        page.AddInputBox(policy.Rate.ToString(), "Everyone", "ffffff", "Everyone");
        page.AddButton("ChangeUBI", "Submit");
        builder.AddPage(page);
        await ctx.ReplyAsync(builder);
    }

    [Command("UBI")]
    public async Task ViewUBI(CommandContext ctx) 
    {
        await ShowUBI(ctx, null);
    }

    [Command("UBI")]
    public async Task ViewUBI(CommandContext ctx, [Remainder] string district) 
    {
        District? _district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name == district);
        if (_district == null) {
            await ctx.ReplyAsync($"Could not find district {district}!");
            return;
        }
        await ShowUBI(ctx, _district.Id);
    }

    public async Task ShowUBI(CommandContext ctx, string? districtid)
    {
        IEnumerable<UBIPolicy> policies = DBCache.GetAll<UBIPolicy>().Where(x => x.DistrictId == districtid);
        
        EmbedBuilder builder = new();
        EmbedPageBuilder page = new EmbedPageBuilder();
        string name = "";
        District? district = null;
        if (districtid is not null) {
            district = DBCache.Get<District>(districtid);
        }
        if (district is not null) {
            name = district.Name;
        }
        else {
            name = "Vooperia";
        }
        page.AddText("", $"UBI For {name}");
        foreach (UBIPolicy policy in policies.OrderByDescending(x => x.Rate))
        {
            string rankname = "";
            string rankcolor = "";
            if (policy.ApplicableRank is null) {
                rankname = "Everyone";
                rankcolor = "ffffff";
            }
            else {
                rankname = policy.ApplicableRank.ToString()!;
                rankcolor = GetRankColor(policy.ApplicableRank);
            }
            page.AddText("", rankname, textColor: rankcolor);
            page.AddText("", $"¢{Math.Round(policy.Rate)} daily");
        }
        builder.AddPage(page);
        await ctx.ReplyAsync(builder);
    }
}