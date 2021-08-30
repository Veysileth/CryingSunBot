using System;
using System.IO;
using CryingSunBot.Utilities;
using Discord.WebSocket;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using CryingSunBot.Database;

namespace CryingSunBot.Modules.AnonTalk
{
    public static class CryingSunNet
    {
        public static class Limits
        {
            private static int maxProxyConnections = 5;
            public static int MaxProxyConnections { get { return maxProxyConnections; } set { maxProxyConnections = value; } }
        }

        public static class Guilds
        {
            public static SocketGuild Guild;
            public static SocketGuild LogGuild;

            internal static readonly ulong GuildID = 0;
            internal static readonly ulong LogGuildID = 0;
        }

        public static class Roles
        {
            internal static readonly ulong Applications = 0;
        }

        public static class Channels
        {
            internal static readonly ulong accountApplicationsChannelId = 0;
            internal static readonly ulong characterApplicationsChannelId = 0;
            internal static readonly ulong channelApplicationsChannelId = 0;
        }

        public static class Nodes
        {
            public static readonly ulong CentralNodeID = 0;

            public static readonly ulong HiddenNodeID = 0;

            public static readonly ulong PrivateNodeID = 0;

            public static readonly ulong ProxyID = 0;
        }

        public static class Messages
        {
            public static readonly string HowToStartMessage = @"
Jak zacząć:
```
!account <Nazwa konta z forum> (bez spacji)
!character <Imię_Nazwisko> <Ksywa> (Imię_Nazwisko wraz z _, musi należeć do niezablokowanej postaci istniejącej na twoim koncie)
!login <Imię_Nazwisko>
!channels
!connect <nazwa kanału z listy>
```

Użyj !help w celu przywołania listy wszystkich dostępnych komend.
";

            public static readonly string HelpMessage = @"
[PW] -> Prywatna wiadomość do bota.
[PROXY] -> Wiadomość w specjalnie wygenerowanym kanale zaczynającym się na ""channel"" lub ""pm"".

Aplikacje
```
[PW]!account <Nazwa> - Aplikacja o stworzenie konta. Jako nazwę należy użyć nazwę konta z forum vrp.
[PW]!character <Imię_Nazwisko> <Ksywa> - Aplikacja o stworzenie postaci. Imię_Nazwisko wraz z _, musi należeć do niezablokowanej postaci istniejącej na twoim koncie.
[PW]!channel <Nazwa> [<hasło>] - Aplikacja o kanał. Hasło należy zostawić puste dla kanałów publicznych, widocznych na /channels.
```

Zarządzanie kontem
```
[PW]!characters - wyświetla wszystkie postaci.
[PW]!login <Imię_Nazwisko> - loguje na daną postać.
```

Łączenie
```
[PW]!channels - wyświetla wszystkie kanały publiczne.
[PW]!connect <Nazwa_Kanału> [<hasło>] - łączy z kanałem.
[PW]!pm <Ksywa> - otwiera kanał prywatnej wiadomości.
```

Komendy lokalne
```
[PROXY]!users - wyświetla podłączonych użytkowników
[PROXY]!disconnect - rozłącza z kanału
```
";
        }

        public static DiscordSocketClient Client;

        public static void Initialize()
        {
            using (var db = new Context())
            {
                db.Database.EnsureCreated();
            }
        }

        public static void Save()
        {
        }

        public static void Load()
        {
        }
    }
}