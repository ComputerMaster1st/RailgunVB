using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Extensions;
using TreeDiagram;

namespace Railgun.Commands
{
    [Alias("rst")]
	public class Rst : SystemBase
	{
		private readonly MasterConfig _config;

		public Rst(MasterConfig config) => _config = config;

		[Command]
		public Task RstAsync()
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync($"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.");
			if (!data.IsEnabled)
				return ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

			var msg = data.GetRst();

			if (string.IsNullOrEmpty(msg))
				msg = $"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.";

			return ReplyAsync(msg);
		}

		[Command("add")]
		public Task AddAsync([Remainder] string msg)
		{
			if (string.IsNullOrWhiteSpace(msg))
				return ReplyAsync("Your message was empty. Please add a message to add.");

			var data = Context.Database.FunRsts.GetOrCreateData(Context.Guild.Id);

			if (!data.IsEnabled)
				return ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

			data.AddRst(msg);
			return ReplyAsync($"Added To RST: {Format.Code(msg)}");
		}

		[Command("remove"), UserPerms(GuildPermission.ManageMessages)]
		public Task RemoveAsync(int index)
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync($"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.");
			if (!data.IsEnabled)
				return ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");
			if (index < 0 || index >= data.Rst.Count)
				return ReplyAsync("The Message Id provided is out of bounds. Please recheck via RST List.");

			data.Rst.RemoveAt(index);
			return ReplyAsync("Message Removed!");
		}

		[Command("list"), BotPerms(ChannelPermission.AttachFiles)]
		public Task ListAsync()
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync($"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.");
			if (!data.IsEnabled)
				return ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

			var output = new StringBuilder()
				.AppendLine("Randomly Selected Text List :").AppendLine();

			data.Rst.ForEach(msg => output.AppendFormat("[{0}] {1}", Format.Code(data.Rst.IndexOf(msg).ToString()), msg).AppendLine());

			if (output.Length < 1950)
				return ReplyAsync(output.ToString());
			return (Context.Channel as ITextChannel).SendStringAsFileAsync("RST.txt", output.ToString());
		}

		[Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
		public Task AllowDenyAsync()
		{
			var data = Context.Database.FunRsts.GetOrCreateData(Context.Guild.Id);
			data.IsEnabled = !data.IsEnabled;
			return ReplyAsync($"RST is now {(data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled"))}!");
		}

		[Command("reset"), UserPerms(GuildPermission.ManageMessages)]
		public Task ResetAsync()
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync("RST has no data to reset.");

			Context.Database.FunRsts.Remove(data);
			return ReplyAsync("RST has been reset.");
		}
	}
}