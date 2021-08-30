using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Modules.AnonTalk;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class Channel
    {
        public enum ChannelType
        {
            Public,
            Private,
            PM
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public ulong DiscordChannelID { get; set; }
        public ChannelType Type { get; set; }
        public string Password { get; set; }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.Channels.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            var channel = CryingSunNet.Guilds.LogGuild.GetTextChannel(DiscordChannelID);
            if (channel is not null) { string oldName = channel.Name; await channel.ModifyAsync(prop => prop.Name = oldName + "-DELETED"); }

            using (var db = new Context())
            {
                db.Channels.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Proxy>> GetConnectedProxiesList()
        {
            List<Proxy> list = new List<Proxy>();
            using (var db = new Context())
            {
                return list = await db.Proxies.FromSqlRaw(sql: $"SELECT * FROM Proxies WHERE TargetDiscordChannelId = {this.DiscordChannelID}").ToListAsync();
            }
        }

        public async Task<List<Character>> GetConnectedCharactersList()
        {
            List<Character> list = new List<Character>();
            using (var db = new Context())
            {
                List<Proxy> proxies = await GetConnectedProxiesList();
                foreach (Proxy proxy in proxies)
                {
                    Character character = await Character.GetCharacterByIDAsync(proxy.CharacterId);
                    list.Add(character);
                }
                return list;
            }
        }

        public async static Task<Channel> GetChannelByNameAsync(string name)
        {
            using (var db = new Context())
            {
                var channel = await db.Channels.FirstOrDefaultAsync(prop => prop.Name == name);
                return channel;
            }
        }

        public async static Task<Channel> GetChannelByDiscordId(ulong id)
        {
            using (var db = new Context())
            {
                var channel = await db.Channels.FirstOrDefaultAsync(prop => prop.DiscordChannelID == id);
                return channel;
            }
        }

        public async static Task<bool> ExistWithNameAsync(string name)
        {
            using (var db = new Context())
            {
                var channel = await db.Channels.FirstOrDefaultAsync(prop => prop.Name == name);
                return channel != null;
            }
        }

        public async static Task<List<Channel>> GetAllPublic()
        {
            List<Channel> channels = new List<Channel>();
            using (var db = new Context())
            {
                return await db.Channels.FromSqlRaw("SELECT * FROM Channels WHERE Type = 0").ToListAsync();
            }
        }
    }
}