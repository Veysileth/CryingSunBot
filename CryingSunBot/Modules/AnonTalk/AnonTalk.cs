using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database.Models;
using Discord.Commands;
using Discord.WebSocket;

namespace CryingSunBot.Modules.AnonTalk
{
    public class AnonTalk
    {
        private readonly SocketCommandContext context;
        private readonly AnonTalkHelper helper;
        private IResult result;

        public AnonTalk(SocketCommandContext socketCommandContext)
        {
            this.context = socketCommandContext;
            helper = new AnonTalkHelper(context);
        }

        public async Task ExecuteAsync(CommandService command)
        {
            ISocketMessageChannel channel = context.Message.Channel;
            SocketUser author = context.Message.Author;
            string message = context.Message.Content;

            if (context.Message.ToString().StartsWith("!"))
            {
                result = await command.ExecuteAsync(context, 0, null);
                return;
            }

            Account account = await Account.GetByDiscordUserIdAsync(author.Id);
            if (account is null) { return; }
            Character character = await account.GetLoggedInCharacterAsync();
            if (character is null) { return; }

            if (channel.Name.StartsWith("channel"))
            {
                Proxy proxy = await Proxy.GetProxyBySourceChannelId(channel.Id);
                if (proxy is null) { return; }

                await proxy.SendMessageToListeners($"**{character.Nickname}**: {CryingSunHelper.MessageModify(message)}");
                await proxy.SendMessageToTarget($"[{context.User.Id} - {context.User.Username}][{account.ID} - {account.Username}][{character.ID} - {character.Nickname}]: {CryingSunHelper.MessageModify(message)}");
                await proxy.SendMessageToSource($"**{character.Nickname}**: {CryingSunHelper.MessageModify(message)}");
                await context.Message.DeleteAsync();
            }
            else if (channel.Name.StartsWith("pm"))
            {
                PrivateConnection privateConnection = await PrivateConnection.GetConnectionBySourceChannelId(context.Channel.Id);
                if (privateConnection is null) { return; }
                if (privateConnection.Character1ID == character.ID)
                {
                    if (privateConnection.Character2SourceDiscordChannelId == 0) { await context.Channel.SendMessageAsync("Rozmówca nie jest połączony."); } else { await privateConnection.SendMessageToCharacter2($"**{character.Nickname}**: {CryingSunHelper.MessageModify(message)}"); await privateConnection.SendMessageToTarget($"[{context.User.Id} - {context.User.Username}][{account.ID} - {account.Username}][{character.ID} - {character.Nickname}]: {CryingSunHelper.MessageModify(message)}"); }
                    await privateConnection.SendMessageToCharacter1($"**{character.Nickname}**: {CryingSunHelper.MessageModify(message)}");
                    await context.Message.DeleteAsync();
                }
                else if (privateConnection.Character2ID == character.ID)
                {
                    if (privateConnection.Character1SourceDiscordChannelId == 0) { await context.Channel.SendMessageAsync("Rozmówca nie jest połączony."); } else { await privateConnection.SendMessageToCharacter1($"**{character.Nickname}**: {CryingSunHelper.MessageModify(message)}"); await privateConnection.SendMessageToTarget($"[{context.User.Id} - {context.User.Username}][{account.ID} - {account.Username}][{character.ID} - {character.Nickname}]: {CryingSunHelper.MessageModify(message)}"); }
                    await privateConnection.SendMessageToCharacter2($"**{character.Nickname}**: {CryingSunHelper.MessageModify(message)}");
                    await context.Message.DeleteAsync();
                }
            }
        }
    }
}