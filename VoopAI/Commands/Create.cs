using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Items.Messages;
using Valour.Shared.Items.Messages.Embeds;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;

namespace SV2.VoopAI.Commands;

class CreateCommands : CommandModuleBase
{
    [Group("create")]
    public class CreateCommandGroup : CommandModuleBase
    {

        [Command("group")]
        public async Task CreateGroup(CommandContext ctx, [Remainder] string groupname)
        {
            User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
            if (_user is null)
            {
                await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
                return;
            }

            // check if user has more than 6 groups
            int groups = DBCache.GetAll<Group>().Where(x => x.OwnerId == _user.Id).Count();

            if (groups >= 6) {
                await ctx.ReplyAsync("You can not create more than 6 groups!");
                return;
            }

            if (DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname) is not null) {
                await ctx.ReplyAsync("There is already a group that has the same name!");
                return;
            }

            Group group = new Group(groupname, _user.Id);
            group.DistrictId = _user.DistrictId;
            
            await DBCache.Put<Group>(group.Id, group);
            await VooperDB.Instance.Groups.AddAsync(group);
            await VooperDB.Instance.SaveChangesAsync();

            await ctx.ReplyAsync($"Successfully created {groupname}!");
            return;
        }

        [Command("account")]
        public async Task CreateAccount(CommandContext ctx) 
        {
            User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
            if (_user is not null) {
                await ctx.ReplyAsync("You already have a SV account!");
                return;
            }
            User user = new User(ctx.Member.Nickname, ctx.Member.User_Id);
            await DBCache.Put<User>(user.Id, user);
            await VooperDB.Instance.Users.AddAsync(user);
            await VooperDB.Instance.SaveChangesAsync();
            await ctx.ReplyAsync("Successfully created SV account.");
        }
    }
}