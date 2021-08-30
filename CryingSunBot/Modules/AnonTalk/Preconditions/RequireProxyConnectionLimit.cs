using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using Discord.Commands;

namespace CryingSunBot.Modules.AnonTalk.Preconditions
{
    internal class RequireProxyConnectionLimit : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            Account account = await Account.GetByDiscordUserIdAsync(context.User.Id); if (account is null) { return await Task.FromResult(PreconditionResult.FromError($@"RequireProxyConnectionLimit - no account")); }
            Character character = await account.GetLoggedInCharacterAsync(); if (character is null) { return await Task.FromResult(PreconditionResult.FromError($@"RequireProxyConnectionLimit - no logged in character")); }

            int count = 0;

            using (var db = new Context())
            {
                count += await db.Proxies.CountAsync(prop => prop.CharacterId == character.ID);
                count += await db.PrivateConnections.CountAsync(prop => prop.Character1ID == character.ID && prop.Character1SourceDiscordChannelId != 0);
                count += await db.PrivateConnections.CountAsync(prop => prop.Character2ID == character.ID && prop.Character2SourceDiscordChannelId != 0);
            }
            if (count >= CryingSunNet.Limits.MaxProxyConnections)
            {
                await context.Channel.SendMessageAsync($@"Osiągnięto limit połączeń wynoszący {CryingSunNet.Limits.MaxProxyConnections}");
                await context.Message.DeleteAsync();
                return await Task.FromResult(PreconditionResult.FromError($@"Osiągnięto limit połączeń wynoszący {CryingSunNet.Limits.MaxProxyConnections}"));
            }
            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}