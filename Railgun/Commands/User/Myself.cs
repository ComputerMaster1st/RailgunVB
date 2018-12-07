using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using TreeDiagram;

namespace Railgun.Commands.User
{
    [Alias("myself", "self")]
    public class Myself : SystemBase
    {
        private readonly TreeDiagramContext _db;

        public Myself(TreeDiagramContext db) => _db = db;

        [Command("mention")]
        public async Task MentionsAsync() {
            var data = await _db.UserMentions.GetOrCreateAsync(Context.Author.Id);

            if (data.DisableMentions) {
                _db.UserMentions.Remove(data);

                await ReplyAsync($"Personal mentions are now {Format.Bold("Enabled")}.");
                return;
            }

            data.DisableMentions = !data.DisableMentions;

            await ReplyAsync($"Personal mentions are now {Format.Bold("Disabled")}.");
        }

        [Command("prefix")]
        public async Task PrefixAsync([Remainder] string input = null) {
            var data = await _db.UserCommands.GetAsync(Context.Author.Id);

            if (string.IsNullOrWhiteSpace(input) && data == null) {
                await ReplyAsync("No prefix has been specified. Please specify a prefix.");
                return;
            } else if (string.IsNullOrWhiteSpace(input) && data != null) {
                _db.UserCommands.Remove(data);

                await ReplyAsync("Personal prefix has been removed.");
                return;
            }

            data = await _db.UserCommands.GetOrCreateAsync(Context.Author.Id);
            data.Prefix = input;

            await ReplyAsync($"Personal prefix has been set! {Format.Code(input + " <command>")}!");
        }

        [Command("show")]
        public async Task ShowAsync() {
            var prefix = await _db.UserCommands.GetAsync(Context.Author.Id);
            var mention = await _db.UserMentions.GetOrCreateAsync(Context.Author.Id);
            var output = new StringBuilder()
                .AppendLine("Railgun User Configuration:").AppendLine()
                .AppendFormat("       Username : {0}#{1}", Context.Author.Username, Context.Author.DiscriminatorValue).AppendLine()
                .AppendFormat("        User ID : {0}", Context.Author.Id).AppendLine().AppendLine()
                .AppendFormat("  Allow Mention : {0}", mention != null ? "No" : "Yes").AppendLine()
                .AppendFormat("Personal Prefix : {0}", prefix != null ? prefix.Prefix : "Not Set").AppendLine();
            
            await ReplyAsync(Format.Code(output.ToString()));
        }
    }
}