using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using PakGaymers.Data.Table;
using PakGaymers.Data.Table.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PakGaymers.Modules
{
    // Create a module with the 'sample' prefix
    [Group("user")]
    public class UserInfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<UserInfoModule> _logger;
        private readonly ITableStorage _tableStorage;

        public UserInfoModule(
            ILogger<UserInfoModule> logger,
            ITableStorage tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

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
            await AddUser(userInfo);
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator} has the roles {string.Join(", ", ((SocketGuildUser)userInfo).Roles.Where(x => !x.Name.Equals("@everyone")))}");
        }

        private async Task AddUser(SocketUser userInfo)
        {
            var table = await _tableStorage.CreateTableAsync(nameof(userInfo));
            var newUser = new UserEntity(userInfo.Discriminator, userInfo.Username);
            await _tableStorage.InsertOrMergeEntityAsync(table, newUser);
        }
    }
}