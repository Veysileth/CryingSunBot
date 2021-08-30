using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryingSunBot.Database.Models
{
    public class Log
    {
        public ulong DiscordChannelID { get; set; }
        public Types Type { get; set; }

        public string Message { get; set; }

        public enum Types
        {
            Channel,
            Proxy,
            PrivateConnection
        }
    }
}