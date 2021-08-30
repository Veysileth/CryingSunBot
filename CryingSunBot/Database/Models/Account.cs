using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class Account
    {
        public int ID { get; set; }
        public ulong DiscordUserId { get; set; }
        public string Username { get; set; }

        public async Task CreateCharacter(string name, string nickname)
        {
            Character character = new Character();
            character.AccountID = ID;
            character.Name = name;
            character.Nickname = nickname;
            character.LoggedIn = false;
            await character.SaveAsync();
        }

        public async Task LogoutAllCharactersAsync()
        {
            List<Character> characters = await GetCharactersAsync();
            characters.ForEach(async prop => await prop.Logout());
        }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.Accounts.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            using (var db = new Context())
            {
                db.Accounts.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Character> GetLoggedInCharacterAsync()
        {
            Character character;
            using (var db = new Context())
            {
                character = await db.Characters.FirstOrDefaultAsync(prop => (prop.AccountID == ID) && (prop.LoggedIn == true));
                return character;
            }
        }

        public async Task<List<Character>> GetCharactersAsync()
        {
            List<Character> list = new List<Character>();
            using (var db = new Context())
            {
                list = await db.Characters.FromSqlRaw(sql: $"SELECT * FROM Characters WHERE AccountID = {this.ID}").ToListAsync();
            }
            return list;
        }

        public async Task<List<Proxy>> GetProxiesAsync()
        {
            List<Proxy> list = new List<Proxy>();
            using (var db = new Context())
            {
                List<Character> a = await db.Characters.FromSqlRaw(sql: $"SELECT * FROM Proxies WHERE AccountID = {this.ID}").ToListAsync();
                a.ForEach(prop => Console.WriteLine(prop));
            }
            return list;
        }

        public async Task<CharacterApplication> GetCharacterApplication()
        {
            CharacterApplication characterApplication;
            using (var db = new Context())
            {
                characterApplication = await db.CharacterApplications.FirstOrDefaultAsync(prop => prop.AccountID == ID);
                return characterApplication;
            }
        }

        public async Task<ChannelApplication> GetChannelApplication()
        {
            ChannelApplication channelApplication;
            using (var db = new Context())
            {
                channelApplication = await db.ChannelApplications.FirstOrDefaultAsync(prop => prop.AccountID == ID);
                return channelApplication;
            }
        }

        public static async Task<Account> GetByIdAsync(int id)
        {
            Account account;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.ID == id);
                return account;
            }
        }

        public static async Task<Account> GetByDiscordUserIdAsync(ulong id)
        {
            Account account;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.DiscordUserId == id);
                return account;
            }
        }

        public static async Task<Account> GetByUsernameAsync(string username)
        {
            Account account;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.Username == username);
                return account;
            }
        }

        public static async Task<Account> GetWithCharacterAsync(Character character)
        {
            Account account;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.ID == character.AccountID);
                return account;
            }
        }
    }
}