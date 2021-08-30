using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CryingSunBot.Modules.AnonTalk
{
    public static class CryingSunHelper
    {
        public static SocketGuild Guild = CryingSunNet.Guilds.Guild;
        public static DiscordSocketClient Client = CryingSunNet.Client;

        public async static Task MoveChannelToCategory(RestTextChannel restTextChannel, ulong categoryId)
        {
            await restTextChannel.ModifyAsync(prop => prop.CategoryId = categoryId);
        }

        public async static Task MoveChannelToCategory(SocketGuildChannel socketGuildChannel, ulong categoryId)
        {
            await socketGuildChannel.ModifyAsync(prop => prop.CategoryId = categoryId);
        }

        public async static Task DenyAllPermissionsToEveryoneRole(RestTextChannel restTextChannel)
        {
            await restTextChannel.AddPermissionOverwriteAsync(CryingSunNet.Guilds.Guild.EveryoneRole, OverwritePermissions.DenyAll(restTextChannel));
        }

        public async static Task DenyAllPermissionsToEveryoneRole(SocketGuildChannel socketGuildChannel)
        {
            await socketGuildChannel.AddPermissionOverwriteAsync(CryingSunNet.Guilds.Guild.EveryoneRole, OverwritePermissions.DenyAll(socketGuildChannel));
        }

        public async static Task RemovePermissionsOverwritesForChannel(ulong discordChannelId, ulong discordUserId)
        {
            var discordChannel = CryingSunNet.Guilds.Guild.GetTextChannel(discordChannelId);
            var discordUser = CryingSunNet.Guilds.Guild.GetUser(discordUserId);

            await discordChannel.RemovePermissionOverwriteAsync(discordUser);
        }

        public async static Task SyncPermissionsForChannel(RestTextChannel socketGuildChannel)
        {
            await socketGuildChannel.SyncPermissionsAsync();
        }

        public async static Task GrantPresetPermissionsToUser(ulong restTextChannelId, ulong userId)
        {
            var discordUser = CryingSunNet.Guilds.Guild.GetUser(userId);
            var discordChannel = CryingSunNet.Guilds.Guild.GetTextChannel(restTextChannelId);

            await discordChannel.AddPermissionOverwriteAsync(discordUser, OverwritePermissions.DenyAll(discordChannel).Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow));
        }

        public async static Task GrantPresetPermissionsToUser(RestTextChannel restTextChannel, IUser userId)
        {
            await restTextChannel.AddPermissionOverwriteAsync(userId, OverwritePermissions.DenyAll(restTextChannel).Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow));
        }

        public async static Task GrantPresetPermissionsToUser(SocketGuildChannel socketGuildChannel, IUser userId)
        {
            await socketGuildChannel.AddPermissionOverwriteAsync(userId, OverwritePermissions.DenyAll(socketGuildChannel).Modify(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow));
        }

        public static string MessageModify(string msg)
        {
            string newmsg;
            newmsg = msg.Replace(@"@", "").Replace(@"#", "").Replace(@"/", "").Replace(@"*", "").Replace(@"`", "").Replace(@"~", "").Replace(@"_", "");

            return newmsg;
        }

        internal static async Task SetMessageInterval(RestTextChannel source)
        {
            await source.ModifyAsync(prop => prop.SlowModeInterval = 2);
        }
    }
}