using System;
using System.Linq;
using System.Threading.Tasks;
using CryingSunBot.Database;
using CryingSunBot.Database.Models;
using Discord.Commands;

namespace CryingSunBot.Modules.AnonTalk.Preconditions
{
    internal class RequireProxyChannelAsSource : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            Proxy proxy = await Proxy.GetProxyBySourceChannelId(context.Channel.Id);
            PrivateConnection priv = await PrivateConnection.GetConnectionBySourceChannelId(context.Channel.Id);
            if (proxy is null && priv is null)
            {
                await context.Channel.SendMessageAsync($@"{context.User.Username} ta komenda jest jedynie dostępna z kanałów proxy.");
                await context.Message.DeleteAsync();
                return await Task.FromResult(PreconditionResult.FromError($@"Komenda dostępna jedynie z kanałów proxy."));
            }
            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}