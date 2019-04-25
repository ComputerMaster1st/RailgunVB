using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Events;
using Railgun.Music.PlayerEventArgs;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Music.Events
{
    public class PlayingEvent : IPlayerEvent
    {
        private readonly MasterConfig _config;
        private readonly IDiscordClient _client;
        private readonly IServiceProvider _services;
        private PlayerContainer _container;

        public PlayingEvent(MasterConfig config, IDiscordClient client, IServiceProvider services)
        {
            _config = config;
            _client = client;
            _services = services;
        }

        public void Load(PlayerContainer container)
		{
            _container = container;
			_container.Player.Playing += (s, a) => Task.Factory.StartNew(() => ExecuteAsync(a));
		}

        private async Task ExecuteAsync(PlayingEventArgs args)
        {
			try {
				ServerMusic data;
				ITextChannel tc;
				await _container.Lock.WaitAsync();
				
				using (var scope = _services.CreateScope()) {
					data = scope.ServiceProvider.GetService<TreeDiagramContext>().ServerMusics.GetData(args.GuildId);
				}

				if (data.NowPlayingChannel != 0)
					tc = await (await _client.GetGuildAsync(args.GuildId)).GetTextChannelAsync(data.NowPlayingChannel);
				else tc = _container.TextChannel;

				if (!data.SilentNowPlaying) {
					var output = new StringBuilder()
						.AppendFormat("Now Playing: {0} {1} ID: {2}", Format.Bold(args.Song.Metadata.Name), SystemUtilities.GetSeparator, Format.Bold(args.Song.Id.ToString())).AppendLine()
						.AppendFormat("Time: {0} {1} Uploader: {2} {1} URL: {3}", Format.Bold(args.Song.Metadata.Length.ToString()), SystemUtilities.GetSeparator, Format.Bold(args.Song.Metadata.Uploader), Format.Bold($"<{args.Song.Metadata.Url}>"));

					await tc.SendMessageAsync(output.ToString());
				}

				await PlayerUtilities.CreateOrModifyMusicPlayerLogEntryAsync(_config, _client, _container);
				_container.Lock.Release();
			} catch {
				SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
				_container.Player.CancelStream();
				_container.Lock.Release();
			}
        }
    }
}