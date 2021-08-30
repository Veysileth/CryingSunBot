using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Modules.AnonTalk;
using Microsoft.EntityFrameworkCore;

namespace CryingSunBot.Database.Models
{
    public class Character
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public bool LoggedIn { get; set; }

        public async Task SaveAsync()
        {
            using (var db = new Context())
            {
                await db.Characters.AddAsync(this);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync()
        {
            using (var db = new Context())
            {
                db.Characters.Remove(this);
                await db.SaveChangesAsync();
            }
        }

        internal async Task<List<Proxy>> GetProxiesAsync()
        {
            List<Proxy> list = new List<Proxy>();
            using (var db = new Context())
            {
                list = await db.Proxies.FromSqlRaw(sql: $"SELECT * FROM Proxies WHERE CharacterId = {ID};").ToListAsync();
                return list;
            }
        }

        internal async Task<List<PrivateConnection>> GetPrivateConnectionsAsync()
        {
            List<PrivateConnection> list = new List<PrivateConnection>();
            using (var db = new Context())
            {
                list = await db.PrivateConnections.FromSqlRaw(sql: $"SELECT * FROM PrivateConnections WHERE Character1ID = {ID};").ToListAsync();
                if (list is not null) { return list; }

                list = await db.PrivateConnections.FromSqlRaw(sql: $"SELECT * FROM PrivateConnections WHERE Character2ID = {ID};").ToListAsync();
                if (list is not null) { return list; }
                return null;
            }
        }

        public async Task<Account> GetAccount()
        {
            Account account;
            using (var db = new Context())
            {
                account = await db.Accounts.FirstOrDefaultAsync(prop => prop.ID == AccountID);
                return account;
            }
        }

        public async Task Login()
        {
            using (var db = new Context())
            {
                var character = await db.Characters.FirstOrDefaultAsync(prop => prop.ID == this.ID);
                character.LoggedIn = true;
                await db.SaveChangesAsync();
            }
        }

        public async Task Logout()
        {
            using (var db = new Context())
            {
                var character = await db.Characters.FirstOrDefaultAsync(prop => prop.ID == this.ID);
                character.LoggedIn = false;
                await db.SaveChangesAsync();
            }
        }

        public async static Task<Character> GetCharacterByNameAsync(string name)
        {
            using (var db = new Context())
            {
                return await db.Characters.FirstOrDefaultAsync(prop => prop.Name == name);
            }
        }

        public async static Task<Character> GetCharacterByNicknameAsync(string name)
        {
            using (var db = new Context())
            {
                return await db.Characters.FirstOrDefaultAsync(prop => prop.Nickname == name);
            }
        }

        public async static Task<Character> GetCharacterByIDAsync(int id)
        {
            using (var db = new Context())
            {
                return await db.Characters.FirstOrDefaultAsync(prop => prop.ID == id);
            }
        }

        public async static Task<bool> ExistWithNameAsync(string name)
        {
            using (var db = new Context())
            {
                var character = await db.Characters.FirstOrDefaultAsync(prop => prop.Name == name);
                return character != null;
            }
        }

        public async static Task<bool> ExistWithNickname(string nickname)
        {
            using (var db = new Context())
            {
                var character = await db.Characters.FirstOrDefaultAsync(prop => prop.Nickname == nickname);
                return character != null;
            }
        }
    }
}