using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using CryingSunBot.Modules.AnonTalk.Preconditions;
using Discord.Commands;

namespace CryingSunBot.Modules.AnonTalk
{
    public class AnonTalkLoginModule : ModuleBase<SocketCommandContext>
    {
        [RequireContext(ContextType.DM)]
        [Command("!help", false, RunMode = RunMode.Sync)]
        public async Task GetHelp()
        {
            var channel = await Context.User.GetOrCreateDMChannelAsync(); if (channel is null) { return; }
            await channel.SendMessageAsync(CryingSunNet.Messages.HelpMessage);
        }

        [RequireContext(ContextType.DM)]
        [RequireDatabaseCharacter]
        [Command("!login", false, RunMode = RunMode.Sync)]
        public async Task LoginToCharacter(string name)
        {
            bool characterExist = await Character.ExistWithNameAsync(name);
            if (!characterExist)
            {
                await Context.Channel.SendMessageAsync($@"Nie znaleziono konta dla postaci {name}. Użyj !characters dla spisu wszystkich postaci. Pamiętaj o formie !login Imię_Nazwisko.");
                return;
            }

            Account account = await Account.GetByDiscordUserIdAsync(Context.User.Id);
            if (account is null) { return; }

            Character character = await Character.GetCharacterByNameAsync(name);
            if (character is null) { return; }

            AccountDiscord accountDiscord = new AccountDiscord(account);
            await accountDiscord.LogoutWithDeconstructionAsync();
            await accountDiscord.LoginWithConstructionAsync(character);

            await Context.Channel.SendMessageAsync($@"Zalgowano na postać {name}. Użyj !channels w celu zobaczenia publicznych kanałów. Użyj !connect <channel_name> (<password>) w celu połączenia.");
        }

        [RequireContext(ContextType.DM)]
        [RequireDatabaseCharacter]
        [Command("!characters")]
        public async Task GetAllOwnedCharactersAsync()
        {
            Account account = await Account.GetByDiscordUserIdAsync(Context.User.Id);
            if (account is null) { return; }

            var characters = await account.GetCharactersAsync();
            StringBuilder stringBuilder = new StringBuilder($"Characters: {Environment.NewLine}");
            characters.ForEach(prop => stringBuilder.Append($"{prop.Name} - {prop.Nickname}{Environment.NewLine}"));

            await Context.Channel.SendMessageAsync(stringBuilder.ToString());
        }
    }
}