using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.IO;
using Console = Colorful.Console;
using Droppy_v1.Handlers;

namespace Droppy_v1
{
    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;
        Declarations dec = new Declarations();
        logHandler logHandler = new logHandler(); 

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();
        public async Task StartAsync()
        {
            dec.CreateFilesAndDirectories();
            logHandler.Start();
            if (Configurator.GetFromConfig.token == "" || Configurator.GetFromConfig.token == null)
            {
                Console.WriteLine("Token missing or null.");
                Console.ReadLine();
                return;
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
            });
            Console.WriteAscii(dec.droppyVersion, System.Drawing.Color.FromArgb(0, 255, 0));
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot, Configurator.GetFromConfig.token);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = System.Drawing.Color.FromArgb(255, 0, 0);
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = System.Drawing.Color.FromArgb(255, 255, 0);
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = System.Drawing.Color.FromArgb(0, 255, 0);
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = System.Drawing.Color.FromArgb(190, 190, 190);
                    break;
            }
            string log = $"[{DateTime.Now}] [{msg.Severity}] {msg.Source}: {msg.Message}";
            Console.WriteLine(log);
            using (StreamWriter sw = new StreamWriter(Declarations.logPath, true))
            {
                sw.WriteLine(log);
            }
            Console.ForegroundColor = System.Drawing.Color.FromArgb(255, 255, 255);
            return Task.CompletedTask;
        }
    }
}
