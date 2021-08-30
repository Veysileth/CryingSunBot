using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace CryingSunBot.Modules.AnonTalk
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(Discord.ChannelPermission.ManageChannels)]
    [Group("!server")]
    internal class AnonTalkModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Group("channel")]
        private class AnonTalkNewModule : ModuleBase<SocketCommandContext>
        {
            [Command("create")]
            public void CreateChannel(string channelName = null, bool isPublic = true, bool requirePassword = false, string password = null)
            {
                //AnonTalkHelper helper = new AnonTalkHelper(Context);

                //if (channelName == null) { helper.SendMessage("!server new channel string channelName = null, bool isPublic = true, bool requirePassword = false, string password = null"); return; }

                //if (helper.GuardTheresAlreadyChannelWithThisName(channelName)) { return; }

                //helper.CreateNewChannel(channelName, isPublic, requirePassword, password);
            }

            [Command("delete")]
            public void DeleteChannel()
            {
                //var sourceDiscordChannelId = Context.Channel.Id;
                //AnonTalkHelper helper = new AnonTalkHelper(Context);

                //if (!helper.TryGetDatabaseChannel(sourceDiscordChannelId, out Database.Channel sourceDatabaseChannel)) { helper.SendMessage("You're not in a channel with database entry"); return; }
                //await helper.DeleteChannelByContext();
            }

            [Command("update")]
            public void UpdateChannel()
            {
                //var sourceDiscordChannel = Context.Channel;
                //var sourceSocketChannel = Context.Channel as SocketTextChannel;
                //var sourceDiscordChannelId = Context.Channel.Id;

                //AnonTalkHelper helper = new AnonTalkHelper(Context);

                //if (!helper.TryGetDatabaseChannel(sourceDiscordChannelId, out Database.Channel sourceDatabaseChannel)) { sourceSocketChannel.ModifyAsync(prop => prop.Topic = "You're not in a channel with database entry"); return; }

                //await sourceSocketChannel.ModifyAsync(prop => prop.Topic = $@"[ID:{sourceDatabaseChannel.Id}][NAME:{sourceDatabaseChannel.Name.ToUpper()}][PUBLIC:{sourceDatabaseChannel.IsPublic.ToString().ToUpper()}][REQUIREPASS:{sourceDatabaseChannel.RequirePassword.ToString().ToUpper()}]");
            }
        }
    }
}