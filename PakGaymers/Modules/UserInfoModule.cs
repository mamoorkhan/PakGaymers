using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace GaymersBot.Modules
{
    // Create a module with the 'sample' prefix
    [Group("user")]
    public class UserInfoModule : ModuleBase<SocketCommandContext>
    {
        // ~user info --> foxbot#0282
        // ~user info @Khionu --> Khionu#8708
        // ~user info Khionu#8708 --> Khionu#8708
        // ~user info Khionu --> Khionu#8708
        // ~user info 96642168176807936 --> Khionu#8708
        // ~user whois 96642168176807936 --> Khionu#8708
        [Command("info")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("whois")]
        public async Task UserInfoAsync([Summary("The (optional) user to get info from")] SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator} has the roles {string.Join(", ", ((SocketGuildUser)userInfo).Roles.Where(x => !x.Name.Equals("@everyone")))}");
        }
    }
}