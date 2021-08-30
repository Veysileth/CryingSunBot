using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class AccountApplication
    {
        public int ID { get; set; }
        public ulong DiscordUserId { get; set; }
        public string Username { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong ApplicationMessageId { get; set; }

        public AccountApplication(ulong DiscordUserId, string Username, ulong ChannelId, ulong MessageId, ulong ApplicationMessageId)
        {
            this.DiscordUserId = DiscordUserId;
            this.Username = Username;
            this.ChannelId = ChannelId;
            this.MessageId = MessageId;
            this.ApplicationMessageId = ApplicationMessageId;
        }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.AccountApplications.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            using (var db = new Context())
            {
                db.AccountApplications.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public static async Task<AccountApplication> GetByApplicationMessageId(ulong id)
        {
            AccountApplication accountApplication;
            using (var db = new Context())
            {
                accountApplication = await db.AccountApplications.FirstOrDefaultAsync(prop => prop.ApplicationMessageId == id);
                return accountApplication;
            }
        }

        //Context.User.Id, Context.User.Username, forumUsername, Context.Channel.Id, Context.Message.Id
    }
}