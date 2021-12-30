using BackgroundMessageDispatcher;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
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
            LogSeverity logLevel = (LogSeverity)Enum.Parse(typeof(LogSeverity), ConfigurationManager.AppSettings["LogSeverity"]);

            TokenManager tokenManager = new TokenManager();
            DiscordSocketConfig socketConfig = new DiscordSocketConfig()
            {
                MessageCacheSize = int.Parse(ConfigurationManager.AppSettings["MessageCacheSize"]),
                TotalShards = int.Parse(ConfigurationManager.AppSettings["TotalShards"]),
                LogLevel = logLevel
            };
            DiscordRestConfig restConfig = new DiscordRestConfig()
            {
                LogLevel = logLevel
            };
            Logger.LogLevel = logLevel;

            using (ServiceProvider services = ConfigureServices(socketConfig, restConfig))
            {
                DiscordShardedClient shardClient = services.GetRequiredService<DiscordShardedClient>();
                DiscordRestClient restClient = services.GetRequiredService<DiscordRestClient>();

                shardClient.ShardReady += ReadyAsync;
                shardClient.Log += Logger.LogAsync;

                restClient.Log += Logger.LogAsync;

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await shardClient.LoginAsync(TokenType.Bot, tokenManager.Token);
                await shardClient.StartAsync();

                await restClient.LoginAsync(TokenType.Bot, tokenManager.Token);

                await Task.Delay(Timeout.Infinite);
            }
        }

        private static ServiceProvider ConfigureServices(DiscordSocketConfig socketConfig, DiscordRestConfig restConfig)
        {
            ServiceProvider services = new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(socketConfig))
                .AddSingleton(new DiscordRestClient(restConfig))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<MessageDispatcher>()
                .AddSingleton(AmdStockCheck.Util.Web.CreateClient())
                .AddDbContext<AmdStockCheck.DataAccess.AmdStockCheckContext>()
                .AddSingleton<AmdStockCheck.DataAccess.AmdDatabaseService>()
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
