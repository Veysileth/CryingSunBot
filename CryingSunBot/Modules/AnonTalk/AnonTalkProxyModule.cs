using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database.Models;
using CryingSunBot.Modules.AnonTalk.Preconditions;
using Discord;
using Discord.Commands;
using Discord.Rest;

namespace CryingSunBot.Modules.AnonTalk
{
    public class AnonTalkProxyModule : ModuleBase<SocketCommandContext>
    {
        [RequireDatabaseLoggedCharacter]
        [RequireContext(ContextType.DM)]
        [Command("!channels")]
        public async Task GetAvailableChannelsAsync()
        {
            List<Channel> channels = await Channel.GetAllPublic();
            StringBuilder stringBuilder = new StringBuilder($"Channels: {Environment.NewLine}");
            foreach (Channel channel in channels)
            {
                stringBuilder.Append($"{channel.Name}" + Environment.NewLine);
            }
            await Context.Channel.SendMessageAsync(stringBuilder.ToString());
        }

        [RequireDatabaseLoggedCharacter]
        [RequireProxyChannelAsSource]
        [RequireContext(ContextType.Guild)]
        [Command("!users")]
        public async Task PrintUsers()
        {
            var sourceDiscordUser = Context.User;
            var sourceDiscordChannel = Context.Channel;

            Account account = await Account.GetByDiscordUserIdAsync(sourceDiscordUser.Id);
            if (account is null) { return; }

            Character character = await account.GetLoggedInCharacterAsync();
            if (character is null) { return; }

            Proxy proxy = await Proxy.GetProxyBySourceChannelId(sourceDiscordChannel.Id);
            if (proxy is null) { return; }

            Channel channel = await proxy.GetChannel();
            if (channel is null) { return; }

            List<Proxy> proxies = await channel.GetConnectedProxiesList();
            if (proxies is null) { return; }

            StringBuilder stringBuilder = new StringBuilder($@"{channel.Name} users:" + Environment.NewLine);
            foreach (Proxy prx in proxies)
            {
                Character connectedCharacter = await Character.GetCharacterByIDAsync(prx.CharacterId);
                stringBuilder.Append($"{connectedCharacter.Nickname}" + Environment.NewLine);
            }

            await Context.Channel.SendMessageAsync(stringBuilder.ToString());
            await Context.Message.DeleteAsync();
        }

        [RequireDatabaseLoggedCharacter]
        [RequireProxyConnectionLimit]
        [RequireContext(ContextType.DM)]
        [Command("!connect")]
        public async Task ConnectToCentralNodeAsync(string channelName, string password = "")
        {
            bool exists = await Channel.ExistWithNameAsync(channelName);
            if (!exists) { await Context.Channel.SendMessageAsync($"Kanał o takiej nazwie nie istnieje."); return; }

            Account account = await Account.GetByDiscordUserIdAsync(Context.User.Id);
            if (account is null) { return; }

            Character character = await account.GetLoggedInCharacterAsync();
            if (character is null) { return; }

            Channel channel = await Channel.GetChannelByNameAsync(channelName);
            if (channel is null) { return; }

            bool connected = await Proxy.IsConnectionEstabilished(character, channel.DiscordChannelID);
            if (connected) { await Context.Channel.SendMessageAsync($"Jesteś już połączony z tym kanałem."); return; }

            if (channel.Password != "" && channel.Password != password) { await Context.Channel.SendMessageAsync($"Nieprawidłowe hasło."); return; }

            await Proxy.NewProxyAsync(account, character, channel);

            await Context.Channel.SendMessageAsync($"Połączono z {channel.Name}. Sprawdź kanał channel-{channelName} na discordzie CryinSunIRC.");
        }

        [RequireDatabaseLoggedCharacter]
        [RequireContext(ContextType.DM)]
        [Command("!pm")]
        public async Task ConnectToPrivateNodeAsync(string userName)
        {
            Character character = await Character.GetCharacterByNicknameAsync(userName);

            Account senderAccount = await Account.GetByDiscordUserIdAsync(Context.User.Id);
            Character senderCharacter = await senderAccount.GetLoggedInCharacterAsync();

            if (character.ID == senderCharacter.ID)
            {
                await Context.Channel.SendMessageAsync($"Nie możesz wysłać wiadomości do samego siebie.");
                return;
            }
            if (character is null)
            {
                await Context.Channel.SendMessageAsync($"Postać o nicku {userName} nie istnieje.");
                return;
            }

            Character receiverCharacter = await Character.GetCharacterByNicknameAsync(userName); if (receiverCharacter is null) { return; }
            Account receiverAccount = await receiverCharacter.GetAccount(); if (receiverAccount is null) { return; }

            var users = await (CryingSunNet.Guilds.Guild as IGuild).GetUsersAsync();
            var senderDiscordUser = users.FirstOrDefault(prop => prop.Id == senderAccount.DiscordUserId); if (senderDiscordUser is null) { return; }
            var receiverDiscordUser = users.FirstOrDefault(prop => prop.Id == receiverAccount.DiscordUserId); if (receiverDiscordUser is null) { return; }

            PrivateConnection privateConnection = await PrivateConnection.GetConnectionBetweenCharacters(senderCharacter, receiverCharacter);
            if (privateConnection is not null)
            {
                if (privateConnection.Character1ID == senderCharacter.ID)
                {
                    if (privateConnection.Character1InvitationSent == true)
                    {
                        await Context.Channel.SendMessageAsync("Zaproszenie zostało wysłane. Oczekuj na odpowiedź.");
                        return;
                    }
                    if (privateConnection.Character2InvitationSent == true || privateConnection.Character1SourceDiscordChannelId == 0)
                    {
                        var newchannel = await CryingSunNet.Guilds.Guild.CreateTextChannelAsync($"PM-{receiverCharacter.Nickname}");
                        await CryingSunHelper.DenyAllPermissionsToEveryoneRole(newchannel);
                        await CryingSunHelper.SetMessageInterval(newchannel);
                        await CryingSunHelper.GrantPresetPermissionsToUser(newchannel, senderDiscordUser);

                        privateConnection.Character1SourceDiscordChannelId = newchannel.Id;
                        privateConnection.Character2InvitationSent = false;
                        await privateConnection.UpdateAsync(privateConnection);
                    }

                    if (privateConnection.Character2SourceDiscordChannelId == 0)
                    {
                        await CryingSunNet.Guilds.Guild.GetUser(receiverDiscordUser.Id)?.SendMessageAsync($"{senderCharacter.Nickname} wysyła zaproszenie do prywatnej rozmowy. !pm {senderCharacter.Nickname} w celu zatwierdzenia.");
                        privateConnection.Character1InvitationSent = true;
                        await privateConnection.UpdateAsync(privateConnection);
                    }
                }

                if (privateConnection.Character2ID == senderCharacter.ID)
                {
                    if (privateConnection.Character2InvitationSent == true)
                    {
                        await Context.Channel.SendMessageAsync("Zaproszenie zostało wysłane. Oczekuj na odpowiedź.");
                        return;
                    }
                    if (privateConnection.Character1InvitationSent == true || privateConnection.Character2SourceDiscordChannelId == 0)
                    {
                        var newchannel = await CryingSunNet.Guilds.Guild.CreateTextChannelAsync($"PM-{receiverCharacter.Nickname}");
                        await CryingSunHelper.DenyAllPermissionsToEveryoneRole(newchannel);
                        await CryingSunHelper.SetMessageInterval(newchannel);
                        await CryingSunHelper.GrantPresetPermissionsToUser(newchannel, senderDiscordUser);

                        privateConnection.Character2SourceDiscordChannelId = newchannel.Id;
                        privateConnection.Character1InvitationSent = false;
                        await privateConnection.UpdateAsync(privateConnection);
                    }
                    if (privateConnection.Character1SourceDiscordChannelId == 0)
                    {
                        await CryingSunNet.Guilds.Guild.GetUser(receiverDiscordUser.Id)?.SendMessageAsync($"{senderCharacter.Nickname} wysyła zaproszenie do prywatnej rozmowy. !pm {senderCharacter.Nickname} w celu zatwierdzenia.");
                        privateConnection.Character2InvitationSent = true;
                        await privateConnection.UpdateAsync(privateConnection);
                    }
                }

                return;
            }

            var senderDiscordChannel = await CryingSunNet.Guilds.Guild.CreateTextChannelAsync($"PM-{receiverCharacter.Nickname}");
            await CryingSunHelper.DenyAllPermissionsToEveryoneRole(senderDiscordChannel);
            await CryingSunHelper.GrantPresetPermissionsToUser(senderDiscordChannel, senderDiscordUser);

            await senderDiscordChannel.SendMessageAsync("Zaproszenie zostało wysłane. Oczekuj na odpowiedź.");

            var logDiscordChannel = await CryingSunNet.Guilds.LogGuild.CreateTextChannelAsync($"PM-{senderCharacter.Nickname}-{receiverCharacter.Nickname}");
            privateConnection = new PrivateConnection() { Account1ID = senderAccount.ID, Account2ID = receiverAccount.ID, Character1ID = senderCharacter.ID, Character2ID = receiverCharacter.ID, Character1InvitationSent = true, Character2InvitationSent = false, Character1SourceDiscordChannelId = senderDiscordChannel.Id, Character2SourceDiscordChannelId = 0, TargetDiscordChannelId = logDiscordChannel.Id };
            await privateConnection.SaveAsync();

            await receiverDiscordUser.GetOrCreateDMChannelAsync().Result.SendMessageAsync($"{senderCharacter.Nickname} wysyła zaproszenie do prywatnej rozmowy. !pm {senderCharacter.Nickname} w celu zatwierdzenia.");
        }

        [RequireProxyChannelAsSource]
        [RequireDatabaseLoggedCharacter]
        [RequireContext(ContextType.Guild)]
        [Command("!disconnect")]
        public async Task DisconnectFromProxyChannelAsync()
        {
            var sourceDiscordUser = Context.User;
            var sourceDiscordChannel = Context.Channel;

            Account account = await Account.GetByDiscordUserIdAsync(sourceDiscordUser.Id);
            if (account is null) { return; }

            Character character = await account.GetLoggedInCharacterAsync();
            if (character is null) { return; }

            Proxy proxy = await Proxy.GetProxyBySourceChannelId(sourceDiscordChannel.Id);
            if (proxy is not null)
            {
                await proxy.DeleteAsync();
            }

            PrivateConnection privateConnection = await PrivateConnection.GetConnectionBySourceChannelId(Context.Channel.Id);
            if (privateConnection is not null)
            {
                if (privateConnection.Character1ID == character.ID)
                {
                    if (privateConnection.Character2SourceDiscordChannelId != 0)
                    {
                        await privateConnection.SendMessageToCharacter2($"*{character.Nickname} has disconnected*");
                    }
                    await CryingSunNet.Guilds.Guild.GetTextChannel(privateConnection.Character1SourceDiscordChannelId).DeleteAsync();
                    privateConnection.Character1SourceDiscordChannelId = 0;
                    await privateConnection.UpdateAsync(privateConnection);
                }
                else if (privateConnection.Character2ID == character.ID)
                {
                    if (privateConnection.Character1SourceDiscordChannelId != 0)
                    {
                        await privateConnection.SendMessageToCharacter1($"*{character.Nickname} has disconnected*");
                    }
                    await CryingSunNet.Guilds.Guild.GetTextChannel(privateConnection.Character2SourceDiscordChannelId).DeleteAsync();
                    privateConnection.Character2SourceDiscordChannelId = 0;
                    await privateConnection.UpdateAsync(privateConnection);
                }
            }
        }
    }
}