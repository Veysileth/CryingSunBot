using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Modules.AnonTalk;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class PrivateConnection
    {
        public int ID { get; set; }

        public int Account1ID { get; set; }
        public int Character1ID { get; set; }
        public ulong Character1SourceDiscordChannelId { get; set; }
        public bool Character1InvitationSent { get; set; }
        public int Account2ID { get; set; }
        public int Character2ID { get; set; }
        public ulong Character2SourceDiscordChannelId { get; set; }
        public bool Character2InvitationSent { get; set; }
        public ulong TargetDiscordChannelId { get; set; }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.PrivateConnections.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            SocketTextChannel chan;
            chan = CryingSunNet.Guilds.Guild.GetTextChannel(Character1SourceDiscordChannelId);
            if (chan is not null) { await chan.DeleteAsync(); }

            chan = CryingSunNet.Guilds.Guild.GetTextChannel(Character2SourceDiscordChannelId);
            if (chan is not null) { await chan.DeleteAsync(); }

            await SendMessageToTarget("Connection killed.");

            using (var db = new Context())
            {
                db.PrivateConnections.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(PrivateConnection privateConnection)
        {
            using (var db = new Context())
            {
                Account1ID = privateConnection.Account1ID;
                Character1ID = privateConnection.Character1ID;
                Character1SourceDiscordChannelId = privateConnection.Character1SourceDiscordChannelId;
                Character1InvitationSent = privateConnection.Character1InvitationSent;
                Account2ID = privateConnection.Account2ID;
                Character2SourceDiscordChannelId = privateConnection.Character2SourceDiscordChannelId;
                Character2InvitationSent = privateConnection.Character2InvitationSent;
                TargetDiscordChannelId = privateConnection.TargetDiscordChannelId;
                db.PrivateConnections.Update(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task SendMessageToTarget(string message)
        {
            var channel = CryingSunNet.Guilds.LogGuild.GetTextChannel(TargetDiscordChannelId);
            if (channel is not null) { await channel.SendMessageAsync(message); }
        }

        public async Task SendMessageToCharacter1(string message)
        {
            var channel = CryingSunNet.Guilds.Guild.GetTextChannel(Character1SourceDiscordChannelId);
            if (channel is not null) { await channel.SendMessageAsync(message); }
        }

        public async Task SendMessageToCharacter2(string message)
        {
            var channel = CryingSunNet.Guilds.Guild.GetTextChannel(Character2SourceDiscordChannelId);
            if (channel is not null) { await channel.SendMessageAsync(message); }
        }

        public static async Task<PrivateConnection> GetConnectionBySourceChannelId(ulong sourceChannelId)
        {
            using (var db = new Context())
            {
                PrivateConnection optionA =  await db.PrivateConnections.FirstOrDefaultAsync(prop => prop.Character1SourceDiscordChannelId == sourceChannelId);
                if (optionA is not null) { return optionA; }
                PrivateConnection optionB =  await db.PrivateConnections.FirstOrDefaultAsync(prop => prop.Character2SourceDiscordChannelId == sourceChannelId);
                if (optionB is not null) { return optionB; }
                return null;
            }
        }

        public static async Task<PrivateConnection> GetConnectionBetweenCharacters(Character character1, Character character2)
        {
            using (var db = new Context())
            {
                PrivateConnection optionA = await db.PrivateConnections.FirstOrDefaultAsync(prop => prop.Character1ID == character1.ID && prop.Character2ID == character2.ID);
                if (optionA is not null) { return optionA; }
                PrivateConnection optionB = await db.PrivateConnections.FirstOrDefaultAsync(prop => prop.Character1ID == character2.ID && prop.Character2ID == character1.ID);
                if (optionB is not null) { return optionB; }
                return null;
            }
        }
    }
}