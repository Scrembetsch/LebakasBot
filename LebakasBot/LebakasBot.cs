using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Util;

namespace LebakasBot
{
    class LebakasBot
    {
        public static void Main(string[] args)
        => new LebakasBot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            TokenManager tokenManager = new TokenManager();
            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                MessageCacheSize = int.Parse(ConfigurationManager.AppSettings["MessageCacheSize"]),
                TotalShards = int.Parse(ConfigurationManager.AppSettings["TotalShards"]),
#if DEBUG
                LogLevel = LogSeverity.Info
#else
                LogLevel = LogSeverity.Error
#endif
            };

            using (ServiceProvider services = ConfigureServices(config))
            {
                DiscordShardedClient shardClient = services.GetRequiredService<DiscordShardedClient>();

                shardClient.ShardReady += ReadyAsync;
                shardClient.Log += Logger.LogAsync;

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await shardClient.LoginAsync(TokenType.Bot, tokenManager.Token);
                await shardClient.StartAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }

        private Task ReadyAsync(DiscordSocketClient shard)
        {
            ConsoleWrapper.WriteLine($"Shard Number {shard.ShardId} is ready!");
            return Task.CompletedTask;
        }
    }
}
