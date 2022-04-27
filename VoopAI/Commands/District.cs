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
using SV2.Database.Models.Districts;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;

namespace SV2.VoopAI.Commands;

class DistrictCommands : CommandModuleBase
{
    [Group("district")]
    public class DistrictGroup : CommandModuleBase
    {

        [Command("budget")]
        public async Task CreateGroup(CommandContext ctx, [Remainder] string districtName)
        {
            User? user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
            if (user is null)
            {
                await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
                return;
            }

            District? district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name == districtName);

            if (district is null)
            {
                await ctx.ReplyAsync($"Could not find district with name {districtName}");
                return;
            }

            // jacob needs to make Text Sections & dropdowns for Valour Embeds first
        }
    }
}