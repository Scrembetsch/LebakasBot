using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Util;

namespace LebakasBot
{
    class LebakasBot
    {
        public static void Main(string[] args)
        => new LebakasBot().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _Client;
        private Task Log(LogMessage msg)
        {
            ConsoleWrapper.WriteLine(msg.ToString(), msg.Severity switch
            {
                LogSeverity.Debug => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.Gray,
                LogSeverity.Info => ConsoleColor.Green,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.Cyan
            });
            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            _Client = new DiscordSocketClient();

            _Client.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = "token";

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _Client.LoginAsync(TokenType.Bot, token);
            await _Client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
