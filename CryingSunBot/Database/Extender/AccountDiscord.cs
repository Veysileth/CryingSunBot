using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database.Models;
using CryingSunBot.Modules.AnonTalk;
using Discord.Rest;
using Discord.WebSocket;

namespace CryingSunBot.Database
{
    internal class AccountDiscord
    {
        private readonly Account account;

        public AccountDiscord(Account account)
        {
            this.account = account;
        }

        public async Task LogoutWithDeconstructionAsync()
        {
            var characters = await account.GetCharactersAsync();
            foreach (Character character in characters)
            {
                await character.Logout();
                var proxies = await character.GetProxiesAsync();
                foreach (Proxy proxy in proxies)
                {
                    var discordChannel = CryingSunNet.Guilds.Guild.GetTextChannel(proxy.SourceDiscordChannelId);
                    if (discordChannel is null) { continue; }

                    proxy.SourceDiscordChannelId = 0;
                    await proxy.UpdateAsync(proxy);

                    await discordChannel.DeleteAsync();
                }
            }
        }

        public async Task LoginWithConstructionAsync(Character character)
        {
            await character.Login();
            var proxies = await character.GetProxiesAsync();
            foreach (Proxy proxy in proxies)
            {
                SocketTextChannel discordSocketChannel = CryingSunNet.Guilds.Guild.GetTextChannel(proxy.SourceDiscordChannelId);
                if (discordSocketChannel is not null) { continue; }

                Channel channel = await Channel.GetChannelByDiscordId(proxy.TargetDiscordChannelId);
                Account account = await character.GetAccount();
                SocketGuildUser discordUser = CryingSunNet.Guilds.Guild.GetUser(account.DiscordUserId);

                RestTextChannel discordRestChannel = await CryingSunNet.Guilds.Guild.CreateTextChannelAsync($"channel-{channel.Name}");

                proxy.SourceDiscordChannelId = discordRestChannel.Id;
                await proxy.UpdateAsync(proxy);

                await CryingSunHelper.DenyAllPermissionsToEveryoneRole(discordRestChannel);
                await CryingSunHelper.SetMessageInterval(discordRestChannel);
                await CryingSunHelper.GrantPresetPermissionsToUser(discordRestChannel, discordUser);
            }
        }
    }
}