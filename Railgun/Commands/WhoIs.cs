using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("whois")]
    public class WhoIs : SystemBase
    {
        private (double Percent, int Level, int PermissionCount) GetEsperLevel(IGuildUser user) 
        {
            var score = 0;
            var permList = user.GuildPermissions.ToList();
            
            if (Context.Guild.OwnerId == user.Id) 
                return (-1, 6, permList.Count);

            if (permList.Contains(GuildPermission.Administrator)) 
                return (-1, 5, permList.Count);

            foreach (var perm in permList) 
            {
                switch (perm) 
                {
                    case GuildPermission.ManageChannels:
                    case GuildPermission.ManageEmojis:
                    case GuildPermission.ManageGuild:
                    case GuildPermission.ManageRoles:
                    case GuildPermission.ManageWebhooks:
                    case GuildPermission.ViewAuditLog:
                    case GuildPermission.ManageNicknames:
                    case GuildPermission.ManageMessages:
                        score += 10;
                        continue;
                    case GuildPermission.BanMembers:
                    case GuildPermission.DeafenMembers:
                    case GuildPermission.KickMembers:
                    case GuildPermission.MoveMembers:
                    case GuildPermission.MuteMembers:
                    case GuildPermission.MentionEveryone:
                    case GuildPermission.SendTTSMessages:
                    case GuildPermission.PrioritySpeaker:
                        score += 5;
                        continue;
                    case GuildPermission.UseVAD:
                    case GuildPermission.UseExternalEmojis:
                    case GuildPermission.AttachFiles:
                    case GuildPermission.CreateInstantInvite:
                    case GuildPermission.AddReactions:
                    case GuildPermission.ChangeNickname:
                    case GuildPermission.EmbedLinks:
                    case GuildPermission.ReadMessageHistory:
                    case GuildPermission.Stream:
                        score += 2;
                        continue;
                    default:
                        score++;
                        continue;
                }
            }

            var percent = Math.Round(score / 142.00 * 100.00);
            int level;

            if (percent < 10.00) level = 0;
            else if (percent < 20.00) level = 1;
            else if (percent < 40.00) level = 2;
            else if (percent < 60.00) level = 3;
            else level = 4;

            return (percent, level, permList.Count);
        }

        [Command]
        public async Task ExecuteAsync(IUser user) 
        {
            var username = $"{user.Username}#{user.DiscriminatorValue}";
            var isHuman = user.IsBot | user.IsWebhook ? "Bot" : "User";
            var guilds = await Context.Client.GetGuildsAsync();
            var locatedOn = 0;

            var embedBuilder = new EmbedBuilder() 
            {
                Title = $"Who is {Format.Bold(username)}?",
                Color = Color.Blue,
                ThumbnailUrl = user.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder() { Text = $"User ID : {user.Id}" }
            };

            foreach (var guild in guilds) 
                if ((await guild.GetUserAsync(user.Id)) != null) 
                    locatedOn++;

            embedBuilder.AddField("User Type:", isHuman, true)
                .AddField("Registered:", user.CreatedAt, true)
                .AddField("Located On:", $"{locatedOn} servers", true);
            
            var gUser = await Context.Guild.GetUserAsync(user.Id);

            if (gUser != null) 
            {
                var roles = new StringBuilder();
                var (Percent, Level, PermissionCount) = GetEsperLevel(gUser);

                foreach (var roleId in gUser.RoleIds) roles.AppendFormat("{0} ", Context.Guild.GetRole(roleId).Mention);

                embedBuilder.AddField("Server Nickname:", gUser.Nickname ?? Format.Bold("N/A"), true)
                    .AddField("Joined Server At:", gUser.JoinedAt.ToString() ?? Format.Bold("Recently"), true)
                    .AddField("Current Server Roles:", roles.ToString() ?? Format.Bold("N/A"))
                    .AddField("ESPer Level:", $"{Level} ({Percent}% - {PermissionCount} Guild Permissions)");
            }

            await ReplyAsync(embed:embedBuilder.Build());
        }

        [Command]
        public async Task ExecuteAsync(ulong userId) 
        {
            var user = await Context.Client.GetUserAsync(userId);

            if (user == null)
            {
                await ReplyAsync("Can not find user.");
                return;
            }

            await ExecuteAsync(user);
        }

        [Command]
        public Task ExecuteAsync() => ExecuteAsync(Context.Author);
    }
}