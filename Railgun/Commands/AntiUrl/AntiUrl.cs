using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    [Alias("antiurl"), UserPerms(GuildPermission.ManageMessages), BotPerms(GuildPermission.ManageMessages)]
	public partial class AntiUrl : SystemBase
	{
		private static string ProcessUrl(string url)
		{
			var cleanUrl = url;
			var parts = new string[] { "http://", "https://", "www." };

			foreach (var part in parts)
				if (cleanUrl.Contains(part)) cleanUrl = cleanUrl.Replace(part, "");

			return cleanUrl.Split('/', 2)[0];
		}

		[Command]
		public Task ExecuteAsync()
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Filters.Urls;

			data.IsEnabled = !data.IsEnabled;

			return ReplyAsync($"Anti-Url is now {Format.Bold(data.IsEnabled ? "Enabled" : "Disabled")}.");
		}
	}
}