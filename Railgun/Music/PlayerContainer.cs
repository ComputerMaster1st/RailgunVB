using Discord;
using Railgun.Music;
using System;

namespace Railgun.Core.Containers
{
    public class PlayerContainer
    {
        private PlayerEventLoader _loader;

        public ulong GuildId { get; }
        public ITextChannel TextChannel { get; }
        public Player Player { get; private set; }
        public IUserMessage LogEntry { get; set; }
        public DateTime CreatedAt { get; } = DateTime.Now;

        public PlayerContainer(ITextChannel tc) {
            GuildId = tc.GuildId;
            TextChannel = tc;
        }

        public void AddEventLoader(PlayerEventLoader loader) => _loader = loader;

        public void AddPlayer(Player player) => Player = player;
    }
}