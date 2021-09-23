using BackgroundMessageDispatcher;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LebakasBot
{
    class LebakasBot
    {
        public static void Main(string[] _)
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
            ServiceProvider services = new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<MessageDispatcher>()
                .AddSingleton(AmdStockCheck.Util.Web.CreateClient())
                .AddSingleton<AmdStockCheck.Service.AmdStockCheckService>()
                .BuildServiceProvider();

            return services;
        }

        private Task ReadyAsync(DiscordSocketClient shard)
        {
            _ = Logger.LogAsync(new LogMessage(LogSeverity.Info, $"Shard {shard.ShardId}", $"Ready!"));
            return Task.CompletedTask;
        }
    }
}
