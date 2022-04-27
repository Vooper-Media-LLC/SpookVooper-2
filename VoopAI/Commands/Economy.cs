using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Items.Messages;
using Valour.Api.Items.Planets.Members;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;

namespace SV2.VoopAI.Commands;

class EconomyCommands : CommandModuleBase
{
    [Command("balance")]
    [Alias("bal")]
    public async Task Balance(CommandContext ctx) 
    {
        User? user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (user is null) {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }
        await ctx.ReplyAsync($"{ctx.Member.Nickname}'s balance: ¢{Math.Round(user.Credits, 2)}");
    }

    [Command("money")]
    public async Task CreateAccount(CommandContext ctx, decimal amount) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        User? user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (user is not null) {
            user.Credits += amount;
        }
    }

    [Command("pay")]
    public async Task Pay(CommandContext ctx, decimal amount, PlanetMember member) 
    {
        User? from = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (from is null) {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        User? to = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == member.User_Id);
        if (from is null) {
            await ctx.ReplyAsync("The user you are trying to send credits to lacks a SV account!");
            return;
        }
        Transaction tran = new Transaction(from.Id, to!.Id, amount, TransactionType.Payment, "Payment from Valour");
        await ctx.ReplyAsync((await tran.Execute()).Info);
    }

    [Command("pay")]
    public async Task Pay(CommandContext ctx, decimal amount, PlanetMember member, [Remainder] string groupname) 
    {
        User? fromuser = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (fromuser is null) {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        Group? from = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
        if (from is null) {
            await ctx.ReplyAsync($"Could not find {groupname}");
            return;
        }

        if (!from.HasPermission(fromuser, GroupPermissions.Eco)) {
            await ctx.ReplyAsync($"You lack permission to send credits using this group!");
            return;
        }

        User? to = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == member.User_Id);
        if (to is null) {
            await ctx.ReplyAsync("The user you are trying to send credits to lacks a SV account!");
            return;
        }
        Transaction tran = new Transaction(from!.Id, to!.Id, amount, TransactionType.Payment, "Payment from Valour");
        await ctx.ReplyAsync((await tran.Execute()).Info);
    }

    [Command("pay")]
    public async Task Pay(CommandContext ctx, decimal amount, [Remainder] string groupname) 
    {
        User? fromuser = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (fromuser is null) {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        Group? to = null;
        Transaction transaction = null;

        if (groupname.Contains(",")) {
            string[] splited = groupname.Split(",");
            to = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == splited[0]);
            if (to is null) {
                await ctx.ReplyAsync($"Could not find {splited[0]}");
                return;
            }
            Group? from = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == splited[1] || (splited[1][0] == ' ' && x.Name == splited[1].Substring(1, splited[1].Length-1)));
            if (from is null) {
                await ctx.ReplyAsync($"Could not find {splited[1]}");
                return;
            }

            if (!from.HasPermission(fromuser, GroupPermissions.Eco)) {
                await ctx.ReplyAsync($"You lack permission to send credits using this group!");
                return;
            }
            transaction = new Transaction(from!.Id, to!.Id, amount, TransactionType.Payment, "Payment from Valour");
            await ctx.ReplyAsync((await transaction.Execute()).Info);
            return;
        }

        to = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
        if (to is null) {
            await ctx.ReplyAsync($"Could not find {groupname}");
            return;
        }
        transaction = new Transaction(fromuser.Id, to!.Id, amount, TransactionType.Payment, "Payment from Valour");
        await ctx.ReplyAsync((await transaction.Execute()).Info);
    }

    [Command("forceubiupdate")]
    public async Task forceubiupdate(CommandContext ctx) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            await ctx.ReplyAsync("Only Jacob can use this command!");
            return;
        }
        List<UBIPolicy>? UBIPolicies = DBCache.GetAll<UBIPolicy>().ToList();

        foreach(UBIPolicy policy in UBIPolicies) {
            List<User> effected = DBCache.GetAll<User>().ToList();
            string fromId = "";
            if (policy.DistrictId != null) {
                effected = effected.Where(x => x.DistrictId == policy.DistrictId).ToList();
                fromId = "g-"+policy.DistrictId;
            }
            else {
                fromId = "g-vooperia";
            }
            if (policy.ApplicableRank != null) {
                effected = effected.Where(x => x.Rank == policy.ApplicableRank).ToList();
            }
            foreach(User user in effected) {
                Transaction tran = new Transaction(fromId, user.Id, policy.Rate/24.0m, TransactionType.Paycheck, $"UBI for rank {policy.ApplicableRank.ToString()}");
                TaskResult result = await tran.Execute();
                if (!result.Succeeded) {
                    // no sense to keep paying these members since the group has ran out of credits
                    break;
                }
            }
        }

        ctx.ReplyAsync("Forced UBI Payout!");
    }
}