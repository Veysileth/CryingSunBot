using System;
using System.Linq;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using Discord.Commands;

namespace CryingSunBot.Modules.AnonTalk.Preconditions
{
    internal class RequireDatabaseLoggedCharacter : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            Account account;
            Character character;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.DiscordUserId == context.User.Id);
            }
            if (account is null)
            {
                await context.Channel.SendMessageAsync($@"{context.User.Username} to konto discord nie posiada zarejestrowanego konta Vibe. Użyj !account <forum username> w celu utworzenia konta.");
                await context.Message.DeleteAsync();
                return await Task.FromResult(PreconditionResult.FromError("To konto discord nie posiada zarejestrowanego konta Vibe. Użyj !account <forum username> w celu utworzenia konta."));
            }
            using (var db = new Context())
            {
                character = await db.Characters.FirstOrDefaultAsync(prop => prop.AccountID == account.ID && prop.LoggedIn == true);
            }
            if (character is null)
            {
                await context.Channel.SendMessageAsync($@"{context.User.Username} to konto nie posiada zalogowanej postaci. Użyj !login <choosen_username> w celu zalogowania na postać.");
                await context.Message.DeleteAsync();
                return await Task.FromResult(PreconditionResult.FromError("To konto nie posiada zalogowanej postaci. Użyj !login <choosen_username> w celu zalogowania na postać."));
            }
            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}