using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Railgun.Core.Extensions;
using Railgun.Core.Managers;
using TreeDiagram;
using TreeDiagram.Models.Filter;

namespace Railgun.Core.Filters
{
	public class AntiUrl : IMessageFilter
	{
		private readonly Regex _regex = new Regex("(http(s)?)://(www.)?");

		private bool CheckContentForUrl(FilterUrl data, string content)
		{
			foreach (var url in data.BannedUrls) {
				if (data.DenyMode && !content.Contains(url) && _regex.IsMatch(content)) return true;
				if (!data.DenyMode && content.Contains(url)) return true;
			}

			return false;
		}

		public async Task<IUserMessage> FilterAsync(ITextChannel tc, IUserMessage message, TreeDiagramContext context)
		{
			var data = context.FilterUrls.GetData(tc.GuildId);

			if (data == null || !data.IsEnabled ||
				(!data.IncludeBots && (message.Author.IsBot | message.Author.IsWebhook)) ||
				data.IgnoredChannels.Any(f => f.ChannelId == tc.Id)) return null;

			var self = await tc.Guild.GetCurrentUserAsync();
			var user = message.Author;

			if (message.Author.Id == self.Id) return null;
			if (!self.GetPermissions(tc).ManageMessages) {
				await tc.TrySendMessageAsync($"{Format.Bold("Anti-Url :")} Triggered but missing {Format.Bold("Manage Messages")} permission!");
				return null;
			}

			var content = message.Content.ToLower();
			var output = new StringBuilder()
				.AppendFormat("{0} Deleted {1}'s Message! {2}", Format.Bold("Anti-Url :"), user.Mention, Format.Bold("Reason :"));

			if (_regex.IsMatch(content) && CheckContentForUrl(data, content)) {
				output.AppendFormat("Unlisted Url Block");
				return await tc.TrySendMessageAsync(output.ToString());
			}
			if (CheckContentForUrl(data, content)) {
				output.AppendFormat("Listed Url Block");
				return await tc.TrySendMessageAsync(output.ToString());
			}

			return null;
		}
	}
}