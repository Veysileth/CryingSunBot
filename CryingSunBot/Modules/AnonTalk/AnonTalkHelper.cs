using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace CryingSunBot.Modules.AnonTalk
{
    public class AnonTalkHelper
    {
        private readonly SocketCommandContext context;

        public AnonTalkHelper(SocketCommandContext socketCommandContext)
        {
            this.context = socketCommandContext;
        }

        public async Task SendMessage(string message)
        {
            await context.Channel.SendMessageAsync($@"{message}");
        }

        internal async Task<bool> GuardCharacterNameAlreadyHasAccountAsync(ulong id, string characterName)
        {
            Account account = await Account.GetByDiscordUserIdAsync(id);
            Character character;
            if (account is null) { return true; }

            using (var db = new Context())
            {
                character = await db.Characters.FirstOrDefaultAsync(prop => prop.Name == characterName);
            }
            if (character is null) { return false; }

            await SendMessage($"{characterName} posiada już przydzielony nickname.");
            return true;
        }

        internal async Task<bool> GuardCharacterNicknameAlreadyHasAccountAsync(ulong id, string anonNickname)
        {
            Account account = await Account.GetByDiscordUserIdAsync(id);
            Character character;
            if (account is null) { return true; }

            using (var db = new Context())
            {
                character = await db.Characters.FirstOrDefaultAsync(prop => prop.Nickname == anonNickname);
            }
            if (character is null) { return false; }

            await SendMessage($"{anonNickname} posiada już przypisaną postać.");
            return true;
        }

        internal async Task<bool> GuardAccountAlreadyExistsForThisForumUsernameAsync(string sourceForumUsername)
        {
            Account account;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.Username == sourceForumUsername);
                await db.SaveChangesAsync();
            }

            if (account is null) { return false; }

            await SendMessage("Takie konto już istnieje.");
            return true;
        }

        internal async Task<bool> GuardApplicationForAccountStillInProgressAsync(ulong sourceDiscordUserId)
        {
            AccountApplication accountApplication;
            using (var db = new Context())
            {
                accountApplication = await db.AccountApplications.FirstOrDefaultAsync(prop => prop.DiscordUserId == sourceDiscordUserId);
            }
            if (accountApplication is null) { return false; }

            await SendMessage("Poczekaj na decyzję w sprawie poprzedniej aplikacji dotyczącej konta");
            return true;
        }

        internal async Task<bool> GuardApplicationForCharacterStillInProgressAsync(ulong sourceDiscordUserId)
        {
            Account account = await Account.GetByDiscordUserIdAsync(sourceDiscordUserId);
            CharacterApplication accountApplication;
            using (var db = new Context())
            {
                accountApplication = await db.CharacterApplications.FirstOrDefaultAsync(prop => prop.AccountID == account.ID);
            }
            if (accountApplication is null) { return false; }

            await SendMessage("Poczekaj na decyzję w sprawie poprzedniej aplikacji dotyczącej postaci.");
            return true;
        }

        internal async Task<bool> GuardApplicationForChannelStillInProgressAsync(ulong sourceDiscordUserId)
        {
            Account account = await Account.GetByDiscordUserIdAsync(sourceDiscordUserId);
            ChannelApplication channelApplication;
            using (var db = new Context())
            {
                channelApplication = await db.ChannelApplications.FirstOrDefaultAsync(prop => prop.AccountID == account.ID);
            }
            if (channelApplication is null) { return false; }

            await SendMessage("Poczekaj na decyzję w sprawie poprzedniej aplikacji dotyczącej kanału");
            return true;
        }
    }
}