using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Containers;
using Railgun.Core.Logging;
using Railgun.Core.Music;
using Railgun.Core.Music.PlayerEventArgs;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Core.Managers
{
	public class PlayerManager
	{
		private readonly IServiceProvider _services;
		private readonly IDiscordClient _client;
		private readonly Log _log;
		private readonly CommandUtils _commandUtils;
		private readonly MusicService _musicService;

		public List<PlayerContainer> PlayerContainers = new List<PlayerContainer>();

		public PlayerManager(IServiceProvider services)
		{
			_services = services;

			_client = _services.GetService<IDiscordClient>();
			_log = _services.GetService<Log>();
			_commandUtils = _services.GetService<CommandUtils>();
			_musicService = _services.GetService<MusicService>();
		}

		public async Task CreatePlayerAsync(IGuildUser user, IVoiceChannel vc, ITextChannel tc, bool autoJoin = false, ISong preRequestedSong = null)
		{
			Playlist playlist;
			ServerMusic data;

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				data = db.ServerMusics.GetOrCreateData(tc.GuildId);

				playlist = await _commandUtils.GetPlaylistAsync(data);
			}

			if (playlist.Songs.Count < 1) {
				if (preRequestedSong != null && !playlist.Songs.Contains(preRequestedSong.Id))
					playlist.Songs.Add(preRequestedSong.Id);

				await tc.SendMessageAsync("As this server has no music yet, I've decided to gather 100 random songs from my repository. One momemt please...");

				var repository = (await _musicService.GetAllSongsAsync()).ToList();
				var rand = new Random();

				if (repository.Count < 100) foreach (var song in repository) playlist.Songs.Add(song.Id);
				else while (playlist.Songs.Count < 100) {
						var i = rand.Next(0, repository.Count());
						var song = repository.ElementAtOrDefault(i);

						if (song != null && !playlist.Songs.Contains(song.Id)) playlist.Songs.Add(song.Id);
					}

				await _musicService.Playlist.UpdateAsync(playlist);
			}

			var username = _commandUtils.GetUsernameOrMention(user);

			await tc.SendMessageAsync($"{(autoJoin ? "Music Auto-Join triggered by" : "Joining now")} {Format.Bold(username)}. Standby...");

			var player = new Player(_musicService, vc) {
				PlaylistAutoLoop = data.PlaylistAutoLoop
			};

			async void connected(object s, ConnectedPlayerEventArgs a) => await ConnectedAsync(a);
			player.Connected += connected;
			async void playing(object s, CurrentSongPlayerEventArgs a) => await PlayingAsync(a);
			player.Playing += playing;
			async void timeout(object s, TimeoutPlayerEventArgs a) => await TimeoutAsync(a);
			player.Timeout += timeout;
			async void finished(object s, FinishedPlayerEventArgs a) => await FinishedAsync(a);
			player.Finished += finished;

			if (preRequestedSong != null) {
				player.AddSongRequest(preRequestedSong);
				player.AutoSkipped = true;
			}

			player.StartPlayer(playlist.Id, autoJoin);

			PlayerContainers.Add(new PlayerContainer(tc, player));

			var autoString = $"{(autoJoin ? "Auto-" : "")}Connecting...";

			await _log.LogToBotLogAsync($"<{vc.Guild.Name} <{vc.GuildId}>> {autoString}", BotLogType.MusicPlayer);
			await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "Music", autoString));
		}

		public PlayerContainer GetPlayer(ulong playerId)
			=> PlayerContainers.FirstOrDefault(container => container.GuildId == playerId);

		public bool IsCreated(ulong playerId)
		{
			if (PlayerContainers.Any(container => container.GuildId == playerId)) return true;
			return false;
		}

		public void DisconnectPlayer(ulong playerId)
			=> PlayerContainers.First(container => container.GuildId == playerId).Player.CancelStream();

		private async Task StopPlayerAsync(ulong playerId, bool autoLeave = false)
		{
			var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == playerId);

			if (container == null) return;

			var player = container.Player;

			if (!autoLeave) player.CancelStream();

			while (player.PlayerTask.Status == TaskStatus.WaitingForActivation) await Task.Delay(500);

			player.PlayerTask.Dispose();
			PlayerContainers.Remove(PlayerContainers.First(cnt => cnt.GuildId == playerId));

			await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "Music", $"Player ID '{playerId}' Destroyed"));
		}

		private async Task ConnectedAsync(ConnectedPlayerEventArgs args)
		{
			var tc = PlayerContainers.First(container => container.GuildId == args.GuildId).TextChannel;
			await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({tc.GuildId})> Connected!", BotLogType.MusicPlayer);
		}

		private async Task PlayingAsync(CurrentSongPlayerEventArgs args)
		{
			try {
				ServerMusic data;
				ITextChannel tc;

				using (var scope = _services.CreateScope()) {
					data = scope.ServiceProvider.GetService<TreeDiagramContext>().ServerMusics.GetData(args.GuildId);
				}

				if (data.NowPlayingChannel != 0)
					tc = await (await _client.GetGuildAsync(args.GuildId)).GetTextChannelAsync(data.NowPlayingChannel);
				else tc = PlayerContainers.First(container => container.GuildId == args.GuildId).TextChannel;

				if (!data.SilentNowPlaying) {
					var output = new StringBuilder()
						.AppendFormat("Now Playing: {0} {1} ID: {2}", Format.Bold(args.Song.Metadata.Name), Response.GetSeparator(), Format.Bold(args.Song.Id.ToString())).AppendLine()
						.AppendFormat("Time: {0} {1} Uploader: {2} {1} URL: {3}", Format.Bold(args.Song.Metadata.Length.ToString()), Response.GetSeparator(), Format.Bold(args.Song.Metadata.Uploader), Format.Bold($"<{args.Song.Metadata.Url}>"));

					await tc.SendMessageAsync(output.ToString());
				}

				await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({tc.GuildId})> Now Playing {args.Song.Id}", BotLogType.MusicPlayer);
			} catch {
				await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));

				var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == args.GuildId);

				if (container != null) container.Player.CancelStream();
			}
		}

		private async Task TimeoutAsync(TimeoutPlayerEventArgs args)
		{
			var tc = PlayerContainers.First(container => container.GuildId == args.GuildId).TextChannel;

			try {
				await tc.SendMessageAsync("Connection to Discord Voice has timed out! Please try again.");
			} catch {
				await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
			} finally {
				var output = new StringBuilder()
					.AppendFormat("<{0} ({1})> Action Timeout!", tc.Guild.Name, args.GuildId).AppendLine()
					.AppendFormat("---- Exception : {0}", args.Exception.ToString());

				await _log.LogToBotLogAsync(output.ToString(), BotLogType.MusicPlayer);
			}
		}

		private async Task FinishedAsync(FinishedPlayerEventArgs args)
		{
			var container = PlayerContainers.FirstOrDefault(cnt => cnt.GuildId == args.GuildId);

			if (container == null) return;

			var tc = container.TextChannel;

			try {
				var output = new StringBuilder();

				if (args.Exception != null) {
					await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Error, "Music", $"{tc.GuildId} Exception!", args.Exception));

					var logOutput = new StringBuilder()
						.AppendFormat("<{0} ({1})> Music Player Error!", tc.Guild.Name, tc.GuildId).AppendLine()
						.AppendFormat("---- Error : {0}", args.Exception.ToString());

					await _log.LogToBotLogAsync(logOutput.ToString(), BotLogType.MusicPlayer);

					output.AppendLine("An error has occured while playing! The stream has been automatically reset. You may start playing music again at any time.");
				}

				var autoOutput = args.AutoDisconnected ? "Auto-" : "";

				output.AppendFormat("{0}Left Voice Channel", autoOutput);

				await tc.SendMessageAsync(output.ToString());
				await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({tc.GuildId})> {autoOutput}Disconnected", BotLogType.MusicPlayer);
			} catch {
				await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
				await _log.LogToBotLogAsync($"<{tc.Guild.Name} ({args.GuildId})> Crash-Disconnected", BotLogType.MusicPlayer);
			} finally {
				await StopPlayerAsync(args.GuildId, args.AutoDisconnected);
			}
		}
	}
}