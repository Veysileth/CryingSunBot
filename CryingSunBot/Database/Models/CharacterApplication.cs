using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class CharacterApplication
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public string CharacterName { get; set; }
        public string CharacterNickname { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong ApplicationMessageId { get; set; }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.CharacterApplications.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            using (var db = new Context())
            {
                db.CharacterApplications.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public static async Task<CharacterApplication> GetByApplicationMessageId(ulong id)
        {
            CharacterApplication characterApplication;
            using (var db = new Context())
            {
                characterApplication = await db.CharacterApplications.FirstOrDefaultAsync(prop => prop.ApplicationMessageId == id);
            }
            return characterApplication;
        }
    }
}