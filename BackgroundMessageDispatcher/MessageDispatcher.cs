using Discord;
using Discord.WebSocket;
using GenericUtil.Extensions;
using System;
using System.Threading.Tasks;

namespace BackgroundMessageDispatcher
{
    public class MessageDispatcher
    {
        private readonly DiscordShardedClient _ShardClient;
        private readonly string _Source = "MsgDispatcher";
        public MessageDispatcher(IServiceProvider services)
        {
            _ShardClient = services.GetService<DiscordShardedClient>();
        }

        public async Task<bool> SendMessageInGuildAsync(string message, ulong guildId, ulong channelId)
        {
            if (_ShardClient.GetGuild(guildId).GetTextChannel(channelId) is not IMessageChannel channel)
            {
                _ = Logger.LogAsync(new LogMessage(LogSeverity.Warning, _Source, $"Failed to retrieve message channel '{channelId}' in guild '{guildId}"));
                return false;
            }
            await channel.SendMessageAsync(message);
            return true;
        }

        public async Task<bool> SendPrivateMessageAsync(string message, ulong userId)
        {
            try
            {
                await _ShardClient.GetUser(userId).SendMessageAsync(message);
                return true;
            }
            catch(Exception e)
            {
                _ = Logger.LogAsync(new LogMessage(LogSeverity.Error, _Source, $"Failed to send private message to user '{userId}'. Details: {e}"));
                return false;
            }
        }
    }
}
