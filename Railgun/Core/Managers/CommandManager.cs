using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Results;
using Railgun.Core.Configuration;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.User;

namespace Railgun.Core.Managers
{
    public class CommandManager
    {
        private readonly IServiceProvider _services;
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly CommandService<SystemContext> _commands;
        private readonly Log _log;
        private readonly Analytics _analytics;
        private readonly FilterManager _filterManager;

        public CommandManager(IServiceProvider services) {
            _services = services;

            _config = _services.GetService<MasterConfig>();
            _client = _services.GetService<DiscordShardedClient>();
            _commands = _services.GetService<CommandService<SystemContext>>();
            _log = _services.GetService<Log>();
            _analytics = _services.GetService<Analytics>();
            _filterManager = _services.GetService<FilterManager>();

            _client.MessageReceived += MessageReceivedAsync;
            _client.MessageUpdated += async (oldMsg, newMsg, channel) => await MessageReceivedAsync(newMsg);
        }

        private async Task ProcessMessageAsync(SocketMessage sMessage) {
            try {
                var msg = (IUserMessage)sMessage;
                var tc = (ITextChannel)msg.Channel;
                var guild = tc.Guild;
                var self = await guild.GetCurrentUserAsync();
                var perms = self.GetPermissions(tc);

                if (!self.GetPermissions(tc).SendMessages) return;

                var filterMsg = await _filterManager.ApplyFilterAsync(msg);

                if (filterMsg != null) {
                    await Task.Run(() => {
                        try {
                            msg.DeleteAsync();
                            
                            _analytics.DeletedMessages++;

                            Task.Delay(5000);
                            filterMsg.DeleteAsync();
                        } catch { }
                    });
                }

                
                
                var context = new SystemContext(_client, sMessage, _services);
                var result = await _commands.ExecuteAsync(context, _services);

                context.DatabaseDispose();

                if (result.IsSuccess) {
                    await _analytics.ExecutedCommandAsync(result as CommandResult);

                    using (var scope = _services.CreateScope()) {
                        var data = await scope.ServiceProvider.GetService<TreeDiagramContext>().ServerCommands.GetAsync(guild.Id);
                        
                        if (data.DeleteCmdAfterUse && perms.ManageMessages) await msg.DeleteAsync();
                    }
                    
                    return;
                }

                switch (result) {
                    case PreconditionResult r:
                        await tc.SendMessageAsync(string.Format("{0} {1}", Format.Bold("Command Error!"), r.Error));
                        break;
                    case CommandResult c:
                        await LogCommandErrorAsync(c);
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Command", "Unexpected Exception!", e));
            }
        }

        private Task MessageReceivedAsync(SocketMessage sMessage) {
            if (sMessage == null || !(sMessage is SocketUserMessage) || !(sMessage.Channel is SocketGuildChannel) || string.IsNullOrEmpty(sMessage.Content))
                return Task.CompletedTask;
            
            return Task.Factory.StartNew(async () => await ProcessMessageAsync(sMessage));
        }

        private async Task LogCommandErrorAsync(CommandResult result) {
            var context = result.Context;

            var output = new StringBuilder()
                .AppendFormat("<{0} <{1}>>", context.Guild.Name, context.Guild.Id).AppendLine()
                .AppendFormat("---- Command : {0}", result.CommandPath).AppendLine()
                .AppendFormat("---- Content : {0}", context.Message.Content).AppendLine()
                .AppendLine("---- Result  : FAILURE").AppendLine()
                .AppendLine(result.Exception.ToString());
            
            await _log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager, true);

            var tcOutput = new StringBuilder()
                .AppendFormat("{0} Something bad has happened inside of me! I've alerted my developer about the problem. I hope they'll get me all patched up soon!", Format.Bold("OH NO!")).AppendLine()
                .AppendLine()
                .AppendFormat("{0} {1}", Format.Bold("ERROR :"), result.Exception.Message);
            
            await context.Channel.SendMessageAsync(tcOutput.ToString());
        }
    }
}