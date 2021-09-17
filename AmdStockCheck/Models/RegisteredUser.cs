using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmdStockCheck.Models
{
    public class RegisteredUser
    {
        public ulong GuildId;
        public ulong ChannelId;
        public ulong UserId;

        public bool Equals(RegisteredUser user)
        {
            return GuildId == user.GuildId
                && ChannelId == user.ChannelId
                && UserId == user.UserId;
        }

        public bool Equals(ulong guildId, ulong channelId, ulong userId)
        {
            return GuildId == guildId
                && ChannelId == channelId
                && UserId == userId;
        }
    }
}
