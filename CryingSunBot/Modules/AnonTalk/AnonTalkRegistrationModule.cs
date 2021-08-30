using System.Linq;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using CryingSunBot.Modules.AnonTalk.Preconditions;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace CryingSunBot.Modules.AnonTalk
{
    //[Group("!Anon")]
    public class AnonTalkRegistrationModule : ModuleBase<SocketCommandContext>
    {
        [RequireContext(ContextType.DM)]
        [Command("!account", false, RunMode = RunMode.Sync)]
        public async Task RegisterAccountAsync(string forumUsername)
        {
            AnonTalkHelper helper = new AnonTalkHelper(Context);
            SocketUser sourceDiscordUser = Context.User;

            if (await helper.GuardAccountAlreadyExistsForThisForumUsernameAsync(forumUsername)) { return; }
            if (await helper.GuardApplicationForAccountStillInProgressAsync(sourceDiscordUser.Id)) { return; }

            var applicationMessage = await CryingSunNet.Guilds.Guild.GetTextChannel(CryingSunNet.Channels.accountApplicationsChannelId).SendMessageAsync($@"Application from {sourceDiscordUser.Username}[{sourceDiscordUser.Id}]. New account: {forumUsername}");

            AccountApplication accountApplication = new AccountApplication(Context.User.Id, forumUsername, Context.Channel.Id, Context.Message.Id, applicationMessage.Id);
            await accountApplication.SaveAsync();
            await Context.Channel.SendMessageAsync("Aplikacja w trakcie rozpatrywania.");
        }

        [RequireDatabaseAccount]
        [RequireContext(ContextType.DM)]
        [Command("!character", false, RunMode = RunMode.Sync)]
        public async Task RegisterCharacterAsync(string characterName, string anonNickname)
        {
            AnonTalkHelper helper = new AnonTalkHelper(Context);
            SocketUser sourceDiscordUser = Context.User;

            if (await helper.GuardApplicationForCharacterStillInProgressAsync(sourceDiscordUser.Id)) { return; }
            if (await helper.GuardCharacterNameAlreadyHasAccountAsync(sourceDiscordUser.Id, characterName)) { return; }
            if (await helper.GuardCharacterNicknameAlreadyHasAccountAsync(sourceDiscordUser.Id, anonNickname)) { return; }

            var account = await Account.GetByDiscordUserIdAsync(sourceDiscordUser.Id);
            if (account is null) { return; }

            var applicationMessage = await CryingSunNet.Guilds.Guild.GetTextChannel(CryingSunNet.Channels.characterApplicationsChannelId).SendMessageAsync($@"Application from {account.Username}[{sourceDiscordUser.Id}]. New character: {characterName} as {anonNickname}");

            CharacterApplication characterApplication = new CharacterApplication()
            { AccountID = account.ID, CharacterName = characterName, CharacterNickname = anonNickname, ChannelId = Context.Channel.Id, MessageId = Context.Message.Id, ApplicationMessageId = applicationMessage.Id};

            await characterApplication.SaveAsync();
            await Context.Channel.SendMessageAsync("Aplikacja w trakcie rozpatrywania.");
        }

        [RequireDatabaseLoggedCharacter]
        [RequireContext(ContextType.DM)]
        [Command("!channel", false, RunMode = RunMode.Sync)]
        public async Task RegisterChannelAsync(string channelName, string password = "")
        {
            bool result = await Channel.ExistWithNameAsync(channelName);
            if (result)
            {
                await Context.Channel.SendMessageAsync("Kanał o takiej nazwie już istnieje.");
                return;
            }

            AnonTalkHelper helper = new AnonTalkHelper(Context);
            SocketUser sourceDiscordUser = Context.User;

            if (await helper.GuardApplicationForChannelStillInProgressAsync(sourceDiscordUser.Id)) { return; }

            var account = await Account.GetByDiscordUserIdAsync(sourceDiscordUser.Id);
            if (account is null) { return; }

            var character = await account.GetLoggedInCharacterAsync();
            if (character is null) { return; }

            string message;
            if (password != "") { message = $@"Application from {sourceDiscordUser.Username}[{sourceDiscordUser.Id}]. New password-protected channel: {channelName}"; }
            else { message = $@"Application from {sourceDiscordUser.Username}[{sourceDiscordUser.Id}]. New channel: {channelName}"; }

            RestUserMessage applicationMessage = await CryingSunNet.Guilds.Guild.GetTextChannel(CryingSunNet.Channels.channelApplicationsChannelId).SendMessageAsync(message);

            ChannelApplication channelApplication = new ChannelApplication()
            { AccountID = account.ID, CharacterID = character.ID, Name = channelName, Password = password, ChannelId = Context.Channel.Id, MessageId = Context.Message.Id, ApplicationMessageId = applicationMessage.Id };
            await channelApplication.SaveAsync();
            await Context.Channel.SendMessageAsync("Aplikacja w trakcie rozpatrywania.");
        }

        public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (CryingSunNet.Guilds.Guild.GetUser(arg3.UserId).IsBot) { return; }
            if (CryingSunNet.Guilds.Guild.GetUser(arg3.UserId).Roles.FirstOrDefault(prop => prop.Id == CryingSunNet.Roles.Applications) == null) { return; }

            if (arg2.Id == CryingSunNet.Channels.accountApplicationsChannelId)
            {
                AccountApplication accountApplication = await AccountApplication.GetByApplicationMessageId(arg3.MessageId);
                if (accountApplication == null) { return; }

                var sourceDiscordUser = CryingSunNet.Guilds.Guild.GetUser(accountApplication.DiscordUserId);
                var sourceDMChannel = await CryingSunNet.Client.GetDMChannelAsync(accountApplication.ChannelId);

                if (arg3.Emote.Name == Usables.Emojis.RedCircle.Name)
                {
                    await sourceDMChannel.SendMessageAsync($@"Podanie o konto dla {accountApplication.Username} nie zostało zaakceptowane.");

                    await accountApplication.DeleteAsync();
                    return;
                }
                else if (arg3.Emote.Name == Usables.Emojis.GreenCircle.Name)
                {
                    await sourceDMChannel.SendMessageAsync($@"Podanie o konto dla {accountApplication.Username} zostało zaakceptowane i połączone z twoim kontem discord. Użyj !character <character_name> <nickname> w celu utworzenia konta dla swojej postaci. Max 1 pseudonim na postać.");

                    Account account = new Account() { DiscordUserId = accountApplication.DiscordUserId, Username = accountApplication.Username };
                    await account.SaveAsync();
                    await accountApplication.DeleteAsync();
                    return;
                }
            }
            else if (arg2.Id == CryingSunNet.Channels.characterApplicationsChannelId)
            {
                CharacterApplication application = await CharacterApplication.GetByApplicationMessageId(arg3.MessageId);
                if (application is null) { return; }

                Account account = await Account.GetByIdAsync(application.AccountID);
                if (account is null) { return; }

                var sourceDiscordUser = CryingSunNet.Guilds.Guild.GetUser(account.DiscordUserId);
                if (sourceDiscordUser is null) { await arg3.Channel.SendMessageAsync($"Użytkownik {account.DiscordUserId} - {account.Username} nie znajduje się na serwerze."); await application.DeleteAsync(); return; }

                var sourceDMChannel = await sourceDiscordUser.GetOrCreateDMChannelAsync();

                if (arg3.Emote.Name == Usables.Emojis.RedCircle.Name)
                {
                    await sourceDMChannel.SendMessageAsync($@"Podanie o postać dla {application.CharacterName} nie zostało zaakceptowane.");

                    await application.DeleteAsync();
                    return;
                }
                else if (arg3.Emote.Name == Usables.Emojis.GreenCircle.Name)
                {
                    await sourceDMChannel.SendMessageAsync($@"Podanie o konto dla {application.CharacterName} zostało zaakceptowane i połączone z kontem {account.Username}. Użyj !login <character_name> w celu aktywowania tej postaci.");

                    await account.CreateCharacter(application.CharacterName, application.CharacterNickname);
                    await application.DeleteAsync();
                    return;
                }
            }
            else if (arg2.Id == CryingSunNet.Channels.channelApplicationsChannelId)
            {
                var application = await ChannelApplication.GetByApplicationMessageId(arg3.MessageId);
                if (application is null) { return; }

                Account account = await Account.GetByIdAsync(application.AccountID);
                if (account is null) { return; }

                var sourceDiscordUser = CryingSunNet.Guilds.Guild.GetUser(account.DiscordUserId);
                if (sourceDiscordUser is null) { await arg3.Channel.SendMessageAsync($"Użytkownik {account.DiscordUserId} - {account.Username} nie znajduje się na serwerze."); await application.DeleteAsync(); return; }

                Character character = await account.GetLoggedInCharacterAsync();
                if (character is null) { return; }

                var sourceDMChannel = await sourceDiscordUser.GetOrCreateDMChannelAsync();
                var sourceDMMessage = await sourceDMChannel.GetMessageAsync(application.MessageId);

                if (arg3.Emote.Name == Usables.Emojis.RedCircle.Name)
                {
                    await sourceDMChannel.SendMessageAsync($@"Podanie o kanał {application.Name} nie zostało zaakceptowane.");

                    await application.DeleteAsync();
                    return;
                }
                else if (arg3.Emote.Name == Usables.Emojis.GreenCircle.Name)
                {
                    await sourceDMChannel.SendMessageAsync($@"Podanie o kanał {application.Name} zostało zaakceptowane. Użyj !connect {application.Name} {application.Password}");

                    RestTextChannel discordChannel = await CryingSunNet.Guilds.LogGuild.CreateTextChannelAsync(application.Name);

                    Channel.ChannelType type = Channel.ChannelType.Public;
                    if (application.Password != "") { type = Channel.ChannelType.Private; }

                    Channel channel = new Channel()
                    {DiscordChannelID = discordChannel.Id, Name = application.Name, Password = application.Password, Type = type };
                    await channel.SaveAsync();

                    await application.DeleteAsync();
                    return;
                }
            }
        }

        public static async Task UserJoined(SocketGuildUser arg)
        {
            var channel = await arg.GetOrCreateDMChannelAsync(); if (channel is null) { return; }
            await channel.SendMessageAsync(CryingSunNet.Messages.HelpMessage);
            await channel.SendMessageAsync(CryingSunNet.Messages.HowToStartMessage);
        }
    }
}