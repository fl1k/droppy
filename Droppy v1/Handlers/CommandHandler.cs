using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using Discord;

namespace Droppy_v1.Handlers
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;
        static logHandler logHandler = new logHandler();

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            var cmdConfig = new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async
            };
            _service = new CommandService(cmdConfig);
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
            _client.Ready += StartedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.UserBanned += UserBannedAsync;
            _client.UserJoined += UserJoinedAsync;

        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var Context = new SocketCommandContext(_client, msg);

            if (msg.Channel.Id == 590997055758008321) // checks if channel is -terms-of-use-and-policy
            {
                if (msg.Content.ToLower().Contains("agree")) // if message has 'agree' in it, (will let ?agree !agree .agree anything people come up with)
                {
                    await (Context.User as IGuildUser).AddRoleAsync(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.memberRole));
                    logHandler.WriteLine($"[{DateTime.Now}] [Info] {Context.User}({Context.User.Id}) has been given the member role");
                }
                else
                {
                    logHandler.WriteLine($"[{DateTime.Now}] [Warning] {Context.User}({Context.User.Id}) is typing in #rules-and-tos!: {msg}");
                }
                await Context.Message.DeleteAsync(); // deletes msg - no need for 6 hour slowmode for people who misspell agree
            }

            if (msg.Channel.Id == 480046709519417344)
            {
                await (Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetChannel(480048125256663050) as IMessageChannel).SendMessageAsync($"**From:** {Context.User}\n**Msg:** {msg}");
                await Context.Message.DeleteAsync();
            }

            if (msg == null || msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot || !(msg.Channel is ISocketPrivateChannel))
                return;

            int argPos = 0;
            if (msg.HasStringPrefix(Configurator.GetFromConfig.cmdPrefix, ref argPos))
            {
                if (Context.Client.GetGuild(Configurator.GetFromConfig.guildID).GetUser(Context.User.Id).Roles.Contains(Context.Client.GetGuild(Configurator.GetFromConfig.guildID).Roles.FirstOrDefault(x => x.Name == Configurator.GetFromConfig.memberRole)))
                {
                    logHandler.WriteLine($"[{DateTime.Now}] [Command] {Context.User}({Context.User.Id}): {msg}");
                    var result = await _service.ExecuteAsync(Context, argPos);

                    if (!result.IsSuccess)
                        await Context.User.SendMessageAsync(result.ErrorReason);

                }
                else
                {
                    await Context.User.SendMessageAsync("You do not `;agree` with our TOS, please go to #rules-and-info and read the everything and then `;agree`");
                }
            }
        }

        private async Task StartedAsync()
        {
            await _client.SetGameAsync("drops | !help", null, ActivityType.Watching);
            await _client.SetStatusAsync(UserStatus.Online);
        }

        private Task UserLeftAsync(SocketGuildUser user)
        {
            UserDataHandler.RemoveFromQueue(user.Id);
            return Task.CompletedTask;
        }

        private Task UserBannedAsync(SocketUser user, SocketGuild guild)
        {
            UserDataHandler.RemoveFromQueue(user.Id);
            return Task.CompletedTask;
        }

        private Task UserJoinedAsync(SocketUser user)
        {
            user.SendMessageAsync($"Hi. Welcome to {_client.GetGuild(Configurator.GetFromConfig.guildID).Name}. Please read #rules-and-info and then send ;agree to access the rest of the server.\nUse !help to access my commands.\nPlease make sure to tell us how you found about us with !feedback <message>");
            logHandler.WriteLine($"[{DateTime.Now}] [Info] {user} has been sent the welcome message.");
            return Task.CompletedTask;
        }
    }
}
