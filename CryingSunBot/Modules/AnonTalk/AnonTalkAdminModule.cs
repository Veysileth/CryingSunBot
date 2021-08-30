using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Modules.AnonTalk
{
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    [Group("!admin")]
    public class AnonTalkAdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("integrity")]
        public async Task CheckIntegrity()
        {
            List<Channel> channels = new List<Channel>();
            List<Proxy> proxies = new List<Proxy>();
            List<PrivateConnection> privateConnections = new List<PrivateConnection>();
            using (var db = new Context())
            {
                await db.Channels.ForEachAsync(prop => channels.Add(prop));
                await db.Proxies.ForEachAsync(prop => proxies.Add(prop));
                await db.PrivateConnections.ForEachAsync(prop => privateConnections.Add(prop));
            }

            List<Log> logs = new List<Log>();

            foreach (var channel in channels)
            {
                var chan = CryingSunNet.Guilds.LogGuild.GetTextChannel(channel.DiscordChannelID);
                if (chan is null)
                {
                    Log log = new Log() { DiscordChannelID = channel.DiscordChannelID, Type = Log.Types.Channel, Message = $"ID:{channel.ID} CHNAME:{channel.Name} CHTYPE:{channel.Type}" };
                    logs.Add(log);
                }
            }
            foreach (var proxy in proxies)
            {
                var chan1 = CryingSunNet.Guilds.LogGuild.GetTextChannel(proxy.SourceDiscordChannelId);
                var chan2 = CryingSunNet.Guilds.LogGuild.GetTextChannel(proxy.TargetDiscordChannelId);
                if (chan1 is null)
                {
                    Log log = new Log() { DiscordChannelID = proxy.SourceDiscordChannelId, Type = Log.Types.Proxy, Message = $"SourceDiscordChannelId ID:{proxy.ID} ACCID:{proxy.AccountID} CHRID:{proxy.CharacterId}" };
                    logs.Add(log);
                }
                if (chan2 is null)
                {
                    Log log = new Log() { DiscordChannelID = proxy.TargetDiscordChannelId, Type = Log.Types.Proxy, Message = $"TargetDiscordChannelId ID:{proxy.ID} ACCID{proxy.AccountID} CHRID:{proxy.CharacterId}" };
                    logs.Add(log);
                }
            }
            foreach (var privateConnection in privateConnections)
            {
                var chan1 = CryingSunNet.Guilds.LogGuild.GetTextChannel(privateConnection.Character1SourceDiscordChannelId);
                var chan2 = CryingSunNet.Guilds.LogGuild.GetTextChannel(privateConnection.Character2SourceDiscordChannelId);
                var chan3 = CryingSunNet.Guilds.LogGuild.GetTextChannel(privateConnection.TargetDiscordChannelId);
                if (chan1 is null)
                {
                    Log log = new Log() { DiscordChannelID = privateConnection.Character1SourceDiscordChannelId, Type = Log.Types.PrivateConnection, Message = $"Character1SourceDiscordChannelId ID:{privateConnection.ID} ACC1ID:{privateConnection.Account1ID} CHR1ID:{privateConnection.Character1ID}" };
                    logs.Add(log);
                }
                if (chan2 is null)
                {
                    Log log = new Log() { DiscordChannelID = privateConnection.Character2SourceDiscordChannelId, Type = Log.Types.PrivateConnection, Message = $"Character2SourceDiscordChannelId ID:{privateConnection.ID} ACC2ID:{privateConnection.Account2ID} CHR2:ID{privateConnection.Character2ID}" };
                    logs.Add(log);
                }
                if (chan3 is null)
                {
                    Log log = new Log() { DiscordChannelID = privateConnection.TargetDiscordChannelId, Type = Log.Types.Channel, Message = $"TargetDiscordChannelId ID:{privateConnection.ID}" };
                    logs.Add(log);
                }
            }

            foreach (var log in logs)
            {
                await Context.Channel.SendMessageAsync($"{log.Type} : {log.DiscordChannelID} : {log.Message}");
            }
        }

        [Command("deleteaccount")]
        public async Task DeleteAccount(string name)
        {
            Account account = await Account.GetByUsernameAsync(name);
            if (account is null) { await Context.Message.AddReactionAsync(Usables.Emojis.RedCircle); return; }

            account.GetCharactersAsync().Result.ForEach(character => character.GetProxiesAsync().Result.ForEach(async prop => await prop.DeleteAsync()));
            account.GetCharactersAsync().Result.ForEach(character => character.GetPrivateConnectionsAsync().Result.ForEach(async prop => await prop.DeleteAsync()));
            account.GetCharactersAsync().Result.ForEach(async character => await character.DeleteAsync());

            await Context.Message.AddReactionAsync(Usables.Emojis.GreenCircle);
            await account.DeleteAsync();
        }

        [Command("deletecharacter")]
        public async Task DeleteCharacter(string name)
        {
            Character character = await Character.GetCharacterByNameAsync(name);
            if (character is null) { await Context.Message.AddReactionAsync(Usables.Emojis.RedCircle); return; }

            character.GetProxiesAsync().Result.ForEach(async prop => await prop.DeleteAsync());
            character.GetPrivateConnectionsAsync().Result.ForEach(async prop => await prop.DeleteAsync());

            await Context.Message.AddReactionAsync(Usables.Emojis.GreenCircle);
            await character.DeleteAsync();
        }

        [Command("deletechannel")]
        public async Task DeleteChannel(string name)
        {
            Channel channel = await Channel.GetChannelByNameAsync(name);
            if (channel is null) { await Context.Message.AddReactionAsync(Usables.Emojis.RedCircle); return; }

            channel.GetConnectedProxiesList().Result.ForEach(async prop => await prop.DeleteAsync());

            await Context.Message.AddReactionAsync(Usables.Emojis.GreenCircle);
            await channel.DeleteAsync();
        }

        [Command("connectionlimit")]
        public void ChangeConnectionLimitNoSafe(int kunut)
        {
            CryingSunNet.Limits.MaxProxyConnections = kunut;
        }
    }
}