using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CryingSunBot.Modules.AnonTalk;
using CryingSunBot.Utilities.Logger;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CryingSunBot
{
    internal class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly CommandService _anonTalk;

        //private readonly IServiceProvider _services;

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 50,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 20000
            });
            _client.Log += Logger.Log;

            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false,
            });
            _commands.Log += Logger.Log;

            _anonTalk = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = false,
            });
            _anonTalk.Log += Logger.Log;

            //_services = ConfigureServices();
        }

        private static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private async Task MainAsync()
        {
            await InitCommands();

            await _client.LoginAsync(TokenType.Bot, "");
            await _client.StartAsync();

            _client.Ready += Client_Ready;

            await Task.Delay(Timeout.Infinite);
        }

        private Task Client_Ready()
        {
            CryingSunNet.Initialize();

            CryingSunNet.Guilds.Guild = _client.GetGuild(CryingSunNet.Guilds.GuildID);
            CryingSunNet.Guilds.LogGuild = _client.GetGuild(CryingSunNet.Guilds.LogGuildID);

            CryingSunNet.Client = _client;

            return Task.CompletedTask;
        }

        private static IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection()
                // Repeat this for all the service classes
                // and other dependencies that your commands might need.
                .AddSingleton(new SomeServiceClass());

            // When all your required services are in the collection, build the container.
            // Tip: There's an overload taking in a 'validateScopes' bool to make sure
            // you haven't made any mistakes in your dependency graph.
            return map.BuildServiceProvider();
        }

        private async Task InitCommands()
        {
            //await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            //await _anonTalk.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            await _anonTalk.AddModuleAsync<AnonTalkAdminModule>(null);
            await _anonTalk.AddModuleAsync<AnonTalkModeratorModule>(null);

            await _anonTalk.AddModuleAsync<AnonTalkRegistrationModule>(null);
            await _anonTalk.AddModuleAsync<AnonTalkLoginModule>(null);
            await _anonTalk.AddModuleAsync<AnonTalkProxyModule>(null);

            //Assembly Ass;
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += AnonTalkRegistrationModule.ReactionAdded;
            _client.UserJoined += AnonTalkRegistrationModule.UserJoined;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage msg = arg as SocketUserMessage;
            SocketCommandContext context = new SocketCommandContext(_client, msg);

            if (msg == null) return;
            if (msg.Author.Id == _client.CurrentUser.Id) { return; }
            if (msg.Author.IsBot) { return; }
            if (CryingSunNet.Guilds.Guild.GetUser(msg.Author.Id) == null) { return; }

            AnonTalk anonTalk = new AnonTalk(context);
            await anonTalk.ExecuteAsync(_anonTalk);
        }

        private class SomeServiceClass : Type
        {
            public override Assembly Assembly { get; }
            public override string AssemblyQualifiedName { get; }
            public override Type BaseType { get; }
            public override string FullName { get; }
            public override Guid GUID { get; }
            public override Module Module { get; }
            public override string Namespace { get; }
            public override Type UnderlyingSystemType { get; }
            public override string Name { get; }

            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override Type GetElementType()
            {
                throw new NotImplementedException();
            }

            public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetInterface(string name, bool ignoreCase)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetInterfaces()
            {
                throw new NotImplementedException();
            }

            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type GetNestedType(string name, BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                throw new NotImplementedException();
            }

            public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            protected override TypeAttributes GetAttributeFlagsImpl()
            {
                throw new NotImplementedException();
            }

            protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException();
            }

            protected override bool HasElementTypeImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsArrayImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsByRefImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsCOMObjectImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPointerImpl()
            {
                throw new NotImplementedException();
            }

            protected override bool IsPrimitiveImpl()
            {
                throw new NotImplementedException();
            }
        }
    }
}