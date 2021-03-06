using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace LebakasBot
{
    public class CommandHandlingService
    {
        private readonly CommandService _CommandService;
        private readonly DiscordShardedClient _ShardedClient;
        private readonly IServiceProvider _Services;
        private readonly char _BotPrefix;

        public CommandHandlingService(IServiceProvider services)
        {
            _CommandService = services.GetRequiredService<CommandService>();
            _ShardedClient = services.GetRequiredService<DiscordShardedClient>();
            _Services = services;

            _CommandService.CommandExecuted += CommandExecutedAsync;
            _CommandService.Log += Logger.LogAsync;
            _ShardedClient.MessageReceived += MessageReceivedAsync;

            _BotPrefix = ConfigurationManager.AppSettings["BotPrefix"][0];
        }

        public async Task InitializeAsync()
        {
            await _CommandService.AddModuleAsync(typeof(AmdStockCheck.Module.AmdStockCheckModule), _Services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if(rawMessage is not SocketUserMessage message || message.Source != MessageSource.User)
            {
                return;
            }

            int argPos = 0;
            if(message.Channel is not IDMChannel)
            {
                if (!message.HasCharPrefix(_BotPrefix, ref argPos)
                    && !message.HasMentionPrefix(_ShardedClient.CurrentUser, ref argPos))
                {
                    return;
                }
            }

            ShardedCommandContext context = new ShardedCommandContext(_ShardedClient, message);
            await _CommandService.ExecuteAsync(context, argPos, _Services);
        }

        public static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if(!command.IsSpecified
                || result.IsSuccess)
            {
                return;
            }

            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}
