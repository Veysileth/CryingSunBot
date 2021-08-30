using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class ChannelApplication
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int CharacterID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong ApplicationMessageId { get; set; }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.ChannelApplications.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            using (var db = new Context())
            {
                db.ChannelApplications.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public static Task<ChannelApplication> GetByApplicationMessageId(ulong id)
        {
            using (var db = new Context())
            {
                return db.ChannelApplications.FirstOrDefaultAsync(prop => prop.ApplicationMessageId == id);
            }
        }
    }
}