﻿using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Inactivity
{
    [Alias("inactive-monitor", "inactive", "imon"), UserPerms(GuildPermission.ManageGuild), BotPerms(GuildPermission.ManageRoles)]
    public partial class Inactivity : SystemBase
    {
        [Command("enable")]
        public Task EnableAsync()
        {
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;
            data.IsEnabled = !data.IsEnabled;
            return ReplyAsync($"Inactivity Monitor has now been turned {Format.Bold(data.IsEnabled ? "On" : "Off")}.");
        }

        [Command("role")]
        public async Task SetRoleAsync(IRole role)
        {
            var self = await Context.Guild.GetCurrentUserAsync();
            var selfHighestRole = self.RoleIds.Select(roleId => Context.Guild.GetRole(roleId))
                .Select(tempRole => tempRole.Position).Concat(new[] {0}).Max();

            if (selfHighestRole < role.Position) await ReplyAsync("Please make sure the inactivity role is in a lower role position than me.");

            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;
            data.InactiveRoleId = role.Id;
            await ReplyAsync($"Inactive Role has been set! ({Format.Bold(role.Name)})");
        }
        
        [Command("role")]
        public Task SetRoleAsync([Remainder] string name)
        {
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == name);
            return role == null ? ReplyAsync($"Unable to find role: {Format.Bold(name)}") : SetRoleAsync(role);
        }

        [Command("initialize")]
        public async Task InitializeAsync()
        {
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;

            if (!data.IsEnabled || data.InactiveRoleId == 0 || data.InactiveDaysThreshold == 0)
            {
                await ReplyAsync("Unable to initialize! Please make sure the Inactivity Monitor is enabled, inactive role is set & inactivity threshold is set.");
                return;
            }

            var output = new StringBuilder()
                .AppendFormat("{0} {1} Initializing Inactivity Monitor...", DateTime.Now.ToString("HH:mm:ss"), SystemUtilities.GetSeparator).AppendLine();
            var response = await ReplyAsync(Format.Code(output.ToString()));
            var users = await Context.Guild.GetUsersAsync();
            var monitoringUsers = 0;
            var alreadyMonitoring = 0;

            output.AppendFormat("{0} {1} {2} Users Found! Preparing Monitor...", DateTime.Now.ToString("HH:mm:ss"), 
                SystemUtilities.GetSeparator, users.Count);
            await response.ModifyAsync((x) => x.Content = Format.Code(output.ToString()));

            foreach (var user in users)
            {
                if (user.IsBot || user.IsWebhook) continue;
                if (Context.Guild.OwnerId == user.Id) continue;
                if (data.Users.Any((u) => u.UserId == user.Id))
                {
                    alreadyMonitoring++;
                    continue;
                }
                if (data.UserWhitelist.Any((u) => u == user.Id)) continue;

                var whitelisted = false;

                if (data.RoleWhitelist.Count > 0)
                {
                    foreach (var role in data.RoleWhitelist)
                    {
                        if (!user.RoleIds.Contains(role)) continue;
                        
                        whitelisted = true;
                        break;
                    }
                }

                if (whitelisted) continue;
                
                data.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
                monitoringUsers++;
            }

            output.AppendFormat("{0} {1} Initialization completed!", DateTime.Now.ToString("HH:mm:ss"), 
                    SystemUtilities.GetSeparator).AppendLine()
                .AppendLine()
                .AppendFormat("Monitoring Users   : {0}", monitoringUsers).AppendLine()
                .AppendFormat("Whitelisted Users  : {0}", users.Count - monitoringUsers).AppendLine()
                .AppendFormat("Already Monitoring : {0}", alreadyMonitoring);
            await response.ModifyAsync((x) => x.Content = Format.Code(output.ToString()));
        }
    }
}
