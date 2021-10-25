using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GenericUtil.Extensions;
using System;
using System.Threading.Tasks;

namespace BackgroundMessageDispatcher
{
    public class MessageDispatcher
    {
        private readonly DiscordShardedClient _ShardClient;
        private readonly DiscordRestClient _RestClient;
        private readonly string _Source = "MsgDispatcher";
        public MessageDispatcher(IServiceProvider services)
        {
            _ShardClient = services.GetService<DiscordShardedClient>();
            _RestClient = services.GetService<DiscordRestClient>();
        }

        public async Task<bool> SendMessageInGuildAsync(string message, ulong guildId, ulong channelId)
        {
            IGuild guild = _ShardClient.GetGuild(guildId);
            if(guild == null)
            {
                guild = await _RestClient.GetGuildAsync(guildId);
            }
            if(guild != null)
            {
                ITextChannel channel = await guild.GetTextChannelAsync(channelId);
                if (channel is IMessageChannel)
                {
                    await channel.SendMessageAsync(message);
                    return true;
                }
            }
            _ = Logger.LogAsync(new LogMessage(LogSeverity.Warning, _Source, $"Failed to retrieve message channel '{channelId}' in guild '{guildId}"));
            return false;
        }

        public async Task<bool> SendPrivateMessageAsync(string message, ulong userId)
        {
            try
            {
                IUser user = _ShardClient.GetUser(userId);
                if(user == null)
                {
                    user = await _RestClient.GetUserAsync(userId);
                }
                if(user != null)
                {
                    await user.SendMessageAsync(message);
                    return true;
                }
                _ = Logger.LogAsync(new LogMessage(LogSeverity.Error, _Source, $"Failed to send private message to user '{userId}'. Details: User not found!"));
                return false;
            }
            catch(Exception e)
            {
                _ = Logger.LogAsync(new LogMessage(LogSeverity.Error, _Source, $"Failed to send private message to user '{userId}'. Details: {e}"));
                return false;
            }
        }
    }
}
