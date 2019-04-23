using System;
using System.Threading.Tasks;
using Discord;
using TreeDiagram;
using TreeDiagram.Interfaces;

namespace Railgun.Core.Filters
{
    public class AntiCaps : AntiFilterBase, IMessageFilter
	{
		public async Task<IUserMessage> FilterAsync(ITextChannel tc, IUserMessage message, TreeDiagramContext context)
		{
			var data = context.FilterCapses.GetData(tc.GuildId);

			if (!CheckConditions(data as ITreeFilter, message)) return null;
			if (message.Content.Length < data.Length) return null;

			var self = await tc.Guild.GetCurrentUserAsync();

			if (message.Author.Id == self.Id) return null;
			else if (!self.GetPermissions(tc).ManageMessages) {
				await tc.SendMessageAsync($"{Format.Bold("Anti-Caps : ")} Triggered but missing {Format.Bold("Manage Messages")} permission!");

				return null;
			}

			var user = await tc.Guild.GetUserAsync(message.Author.Id);
			double charCount = 0;
			double capsCount = 0;

			foreach (var c in message.Content) {
				if (char.IsLetter(c)) {
					charCount++;

					if (char.IsUpper(c)) capsCount++;
				}
			}

			if (capsCount < 1 || charCount < data.Length) return null;

			var percent = (capsCount / charCount) * 100.00;

			if (percent < data.Percentage) return null;

			return await tc.SendMessageAsync(string.Format("{0} Deleted {1}'s Message! ({2} Caps)",
				Format.Bold("Anti-Caps :"),
				user.Mention,
				Format.Bold(Math.Round(percent) + "%")));
		}
	}
}