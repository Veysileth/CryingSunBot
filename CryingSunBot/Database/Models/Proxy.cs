using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Modules.AnonTalk;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class Proxy
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int CharacterId { get; set; }
        public ulong SourceDiscordChannelId { get; set; }
        public ulong TargetDiscordChannelId { get; set; }
        public string Password { get; set; }

        public Proxy()
        {
        }

        public Proxy(int AcccountID, int CharacterID, Channel channel)
        {
        }

        public async Task SaveAsync()
        {
            var character = await Character.GetCharacterByIDAsync(CharacterId);
            if (character is not null)
            {
                await SendMessageToListeners($"*{character.Nickname} has connected*");
                await SendMessageToTarget($"*{character.Nickname} has connected* (SaveAsync)");
            }

            using (var db = new Context())
            {
                await db.Proxies.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            var character = await Character.GetCharacterByIDAsync(CharacterId);
            if (character is not null)
            {
                await SendMessageToListeners($"*{character.Nickname} has disconnected*");
                await SendMessageToTarget($"*{character.Nickname} has disconnected* (DeleteAsync)");
            }

            var chan = CryingSunNet.Guilds.Guild.GetTextChannel(SourceDiscordChannelId);
            if (chan is not null) { await chan.DeleteAsync(); }

            using (var db = new Context())
            {
                db.Proxies.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Proxy proxy)
        {
            using (var db = new Context())
            {
                await db.Database.ExecuteSqlRawAsync($"UPDATE Proxies SET AccountID = {proxy.AccountID}, CharacterId = {proxy.CharacterId}, SourceDiscordChannelId = {proxy.SourceDiscordChannelId}, TargetDiscordChannelId = {proxy.TargetDiscordChannelId} WHERE ID = {ID};");
                await db.SaveChangesAsync();
            }
        }

        public async Task SendMessageToTarget(string message)
        {
            var channel = CryingSunNet.Guilds.LogGuild.GetTextChannel(TargetDiscordChannelId);
            if (channel is not null) { await channel.SendMessageAsync(message); }
        }

        public async Task SendMessageToSource(string message)
        {
            var channel = CryingSunNet.Guilds.Guild.GetTextChannel(SourceDiscordChannelId);
            if (channel is not null) { await channel.SendMessageAsync(message); }
        }

        public async Task SendMessageToListeners(string message)
        {
            var proxies = await GetProxiesByTargetAsync(TargetDiscordChannelId);
            foreach (Proxy proxy in proxies)
            {
                if (proxy.ID == ID || proxy.SourceDiscordChannelId == 0) { continue; }
                var channel = CryingSunNet.Guilds.Guild.GetTextChannel(proxy.SourceDiscordChannelId);
                if (channel is not null) { await channel.SendMessageAsync(message); }
            }
        }

        public async Task<Channel> GetChannel()
        {
            using (var db = new Context())
            {
                Channel channel = await db.Channels.FirstOrDefaultAsync(prop => prop.DiscordChannelID == TargetDiscordChannelId);
                return channel;
            }
        }

        public async static Task<Proxy> GetProxyBySourceChannelId(ulong sourceDiscordChannelId)
        {
            using (var db = new Context())
            {
                var proxy = await db.Proxies.FirstOrDefaultAsync(prop => prop.SourceDiscordChannelId == sourceDiscordChannelId);
                return proxy;
            }
        }

        public async static Task<bool> IsConnectionEstabilished(Character character, ulong targetDiscordChannelId)
        {
            using (var db = new Context())
            {
                var proxy = await db.Proxies.FirstOrDefaultAsync(prop => prop.CharacterId == character.ID && prop.TargetDiscordChannelId == targetDiscordChannelId);
                return proxy is not null;
            }
        }

        public async static Task<List<Proxy>> GetProxiesByTargetAsync(ulong targetDiscordChannel)
        {
            List<Proxy> proxies = new List<Proxy>();
            using (var db = new Context())
            {
                proxies = await db.Proxies.FromSqlRaw(sql: $"SELECT * FROM Proxies WHERE TargetDiscordChannelId = {targetDiscordChannel}").ToListAsync();
            }
            return proxies;
        }

        public async static Task NewProxyAsync(Account account, Character character, Channel channel)
        {
            Proxy proxy = new Proxy() { AccountID = account.ID, CharacterId = character.ID, Password = channel.Password };

            var discordUser = CryingSunNet.Guilds.Guild.GetUser(account.DiscordUserId);
            if (discordUser is null) { return; }

            var source = await CryingSunNet.Guilds.Guild.CreateTextChannelAsync($"channel-{channel.Name}");
            await source.ModifyAsync(prop => prop.Topic = $"AID: {account.ID} ANAME: {account.Username} CID: {character.ID} CNAME: {character.Name} CNICKNAME: {character.Nickname} CHID: {channel.ID} CHNAME: {channel.Name}");
            await CryingSunHelper.DenyAllPermissionsToEveryoneRole(source);
            await CryingSunHelper.GrantPresetPermissionsToUser(source, discordUser);
            await CryingSunHelper.SetMessageInterval(source);

            proxy.SourceDiscordChannelId = source.Id;
            proxy.TargetDiscordChannelId = channel.DiscordChannelID;
            await proxy.SaveAsync();
        }
    }
}