using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using Valour.Api.Items.Messages;

namespace SV2.VoopAI.Commands;

class TestCommands : CommandModuleBase
{
    [Command("ping")]
    public async Task Ping(CommandContext ctx) 
    {
        ctx.ReplyAsync("Pong!");
    }
}