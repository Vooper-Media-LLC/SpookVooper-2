using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Items.Messages;
using Valour.Shared.Items.Messages.Embeds;
using SV2.Database.Models.Groups;
using Valour.Api.Items.Planets.Members;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using System.Linq;
using SV2.Web;

namespace SV2.VoopAI.Commands;

class GroupCommands : CommandModuleBase
{
    [Command("groups")]
    public async Task GroupsOwned(CommandContext ctx)
    {
        User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
        if (_user is null)
        {
            await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
            return;
        }

        IEnumerable<Group> groups = DBCache.GetAll<Group>().Where(x => x.OwnerId == _user.Id);

        if (groups.Count() == 0) {
            await ctx.ReplyAsync("You do not own any groups!");
            return;
        }

        EmbedBuilder builder = new();
        EmbedPageBuilder page = new();

        int i = 0;
        foreach(Group group in groups)
        {
            page.AddText(group.Name, $"¢{Math.Round(group.Credits, 2)}", true);
            i += 1;
            if (i >= 3) {
                page.AddText("", "", false);
            }
        }

        builder.AddPage(page);
        await ctx.ReplyAsync(builder);
    }   

    [Group("group")]
    public class GroupCommandGroup : CommandModuleBase
    {
        [Command("pay")]
        public async Task PayUsingGroup(CommandContext ctx)
        {
            EmbedBuilder builder = new();
            EmbedPageBuilder page = new();
            builder.AddPage(page);
            await ctx.ReplyAsync(builder);
        }

        [Interaction("")]
        public async Task GroupInteractions(InteractionContext ctx)
        {
            // Element_Id is the id of the button that was clicked
            string EventId = ctx.Event.Element_Id;
            if (EventId.Contains("CreateRole")) {
                string groupid = EventId.Split(":")[1];
                User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
                if (_user is null)
                {
                    return;
                }
                Group? group = DBCache.GetAll<Group>().FirstOrDefault(x => x.Id == groupid);
                if (group is null)
                {
                    return;
                }

                if (!group.HasPermission(_user, GroupPermissions.ManageRoles))
                {
                    return;
                }

                string name = ctx.Event.Form_Data.FirstOrDefault(x => x.Element_Id == "Name")!.Value;
                string color = ctx.Event.Form_Data.FirstOrDefault(x => x.Element_Id == "Color")!.Value;
                int authority = int.Parse(ctx.Event.Form_Data.FirstOrDefault(x => x.Element_Id == "Authority")!.Value);
                decimal salary = decimal.Parse(ctx.Event.Form_Data.FirstOrDefault(x => x.Element_Id == "Salary")!.Value)/24.0m;

                GroupRole role = new GroupRole(name, group.Id, salary, authority);
                role.Color = color;
                if (color == " ") {
                    role.Color = "ffffff";
                }
                
                await DBCache.Put<GroupRole>(role.Id, role);
                await VooperDB.Instance.GroupRoles.AddAsync(role);
                await VooperDB.Instance.SaveChangesAsync();
                await ctx.ReplyAsync($"Successfully added role {role.Name} to {group.Name}.");
            }
            if (EventId.Contains("AddToRole")) {
                string roleid = EventId.Split(":")[1];
                string targetid = EventId.Split(":")[2];
                User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
                if (_user is null)
                {
                    return;
                }

                GroupRole? role = DBCache.GetAll<GroupRole>().FirstOrDefault(x => x.Id == roleid);
                if (role is null) {
                    return;
                }

                Group? group = DBCache.GetAll<Group>().FirstOrDefault(x => x.Id == role.GroupId);
                if (group is null)
                {
                    return;
                }

                User? target = DBCache.GetAll<User>().FirstOrDefault(x => x.Id == targetid);
                if (_user is null)
                {
                    return;
                }

                if (!group.HasPermission(_user, GroupPermissions.ManageRoles))
                {
                    return;
                }

                if (group.OwnerId != _user.Id) {
                    // get the authority of the highest role that this user has that has ManageRoles permission
                    int authority = group.GetHighestRoleWithPermission(_user, GroupPermissions.ManageRoles).Authority;
                    
                    // ADD MORE
                }

                if (!role.Members.Contains(targetid)) 
                {
                    role.Members.Add(targetid);
                }

                else {
                    await ctx.ReplyAsync($"The user already has the {role.Name} role!");
                    return;
                }

                await ctx.ReplyAsync($"Successfully added {target.Name} to the {role.Name} role.");

            }
        }

        [Command("manageroles")]
        public async Task CreateGroupRole(CommandContext ctx, PlanetMember member, [Remainder] string groupname)
        {
            User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
            if (_user is null)
            {
                await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
                return;
            }
            Group? group = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
            if (group is null)
            {
                await ctx.ReplyAsync($"Could not find {groupname}!");
                return;
            }

            User? target = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == member.User_Id);
            if (target is null) {
                await ctx.ReplyAsync("The user you are trying to manage their roles does not have a SV account!");
                return;
            }

            if (!group.HasPermission(_user, GroupPermissions.ManageRoles))
            {
                await ctx.ReplyAsync("You lack permission to manage roles for this group!");
                return;
            }
            EmbedBuilder builder = new();
            EmbedPageBuilder page = new();
            page.AddText(null, $"Manage roles for {target.Name}");
            foreach(GroupRole role in DBCache.GetAll<GroupRole>().Where(x => x.GroupId == group.Id)) {
                page.AddText(null, role.Name, textColor:role.Color);
                if (role.Members.Contains(target.Id)) {
                    page.AddButton($"AddToRole:{role.Id}:{target.Id}", "Add", inline:true);
                    page.AddButton($"RemoveFromRole:{role.Id}:{target.Id}", "Remove", inline:true, color:"7F0000");
                }
                else {
                    page.AddButton($"AddToRole:{role.Id}:{target.Id}", "Add", inline:true, color:"007F0E");
                    page.AddButton($"RemoveFromRole:{role.Id}:{target.Id}", "Remove", inline:true);
                }
            }
            builder.AddPage(page);
            await ctx.ReplyAsync(builder);

        }

        [Command("createrole")]
        public async Task CreateGroupRole(CommandContext ctx, [Remainder] string groupname)
        {
            User? _user = DBCache.GetAll<User>().FirstOrDefault(x => x.ValourId == ctx.Member.User_Id);
            if (_user is null)
            {
                await ctx.ReplyAsync("You do not have a SV account! Create one by doing /create account");
                return;
            }
            Group? group = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
            if (group is null)
            {
                await ctx.ReplyAsync($"Could not find {groupname}!");
                return;
            }

            EmbedBuilder builder = new();
            EmbedPageBuilder page = new();
            page.AddText(null, "Role Name");
            page.AddInputBox("", "Name", id: "Name");
            page.AddText(null, "Role Color");
            page.AddInputBox("", "Color", id: "Color");
            page.AddText(null, "Authority (ex: 100 has control over every role with <100 authority)");
            page.AddInputBox("", "Authority", id: "Authority");
            page.AddText(null, "Salary (daily)");
            page.AddInputBox("", "Salary", id: "Salary");
            page.AddButton($"CreateRole:{group.Id}", "Submit", "Submit");
            builder.AddPage(page);
            await ctx.ReplyAsync(builder);
        }

        [Command("roles")]
        public async Task ViewRoles(CommandContext ctx, [Remainder] string groupname)
        {
            Group? group = DBCache.GetAll<Group>().FirstOrDefault(x => x.Name == groupname);
            if (group is null)
            {
                await ctx.ReplyAsync($"Could not find {groupname}!");
                return;
            }

            EmbedBuilder builder = new();
            EmbedPageBuilder page = new();

            page.AddText(null, $"{groupname}'s Roles");

            foreach(GroupRole role in DBCache.GetAll<GroupRole>().Where(x => x.GroupId == group.Id)) {
                string extra = "";
                if (role.Salary > 0.0m) {
                    extra += $" (¢{Math.Round(role.Salary*24.0m,2)} daily)";
                }
                page.AddText(null, role.Name+extra, textColor:role.Color);
                string members = "";
                foreach(string userid in role.Members) {
                    members += $"{DBCache.Get<User>(userid).Name}, ";
                }
                if (members != "") {
                    members = members.Substring(0, members.Length-2);
                }
                page.AddText(null, members);
            }
            
            builder.AddPage(page);
            await ctx.ReplyAsync(builder);
        }
    }
}