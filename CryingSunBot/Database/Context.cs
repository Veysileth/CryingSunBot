using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryingSunBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CryingSunBot.Database
{
    public partial class Context : DbContext
    {
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountApplication> AccountApplications { get; set; }
        public virtual DbSet<Channel> Channels { get; set; }
        public virtual DbSet<ChannelApplication> ChannelApplications { get; set; }
        public virtual DbSet<Character> Characters { get; set; }
        public virtual DbSet<CharacterApplication> CharacterApplications { get; set; }
        public virtual DbSet<Proxy> Proxies { get; set; }
        public virtual DbSet<PrivateConnection> PrivateConnections { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ConnectionString cs = new ConnectionString();
            using (StreamReader sr = new StreamReader("Database/ConnectionString.json"))
            {
                string stream = sr.ReadToEnd();
                cs = JsonConvert.DeserializeObject<ConnectionString>(stream);
            }

            optionsBuilder.UseMySql(cs.ToString(), options => options.EnableRetryOnFailure());
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasKey(c => c.ID);

            modelBuilder.Entity<AccountApplication>().HasKey(c => c.ID);

            modelBuilder.Entity<Channel>().HasKey(c => c.ID);

            modelBuilder.Entity<ChannelApplication>().HasKey(c => c.ID);

            modelBuilder.Entity<Character>().HasKey(c => c.ID);

            modelBuilder.Entity<CharacterApplication>().HasKey(c => c.ID);

            modelBuilder.Entity<Proxy>().HasKey(c => c.ID);

            modelBuilder.Entity<PrivateConnection>().HasKey(c => c.ID);
        }
    }

    [Serializable]
    public class ConnectionString
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Sslmode { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Server != null) { sb.Append($"server={Server};"); }
            if (Database != null) { sb.Append($"database={Database};"); }
            if (User != null) { sb.Append($"user={User};"); }
            if (Password != null) { sb.Append($"password={Password};"); }
            if (Port != null) { sb.Append($"port={Port};"); }
            if (Sslmode != null) { sb.Append($"sslmode={Sslmode};"); }
            sb.Append($"Max Pool Size=50;");
            return $"{sb.ToString()}";
        }
    }
}