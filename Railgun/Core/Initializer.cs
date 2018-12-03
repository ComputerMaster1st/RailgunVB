using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Configuration;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Core
{
    public class Initializer
    {
        private readonly MasterConfig _masterConfig;
        private readonly DiscordShardedClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private Log _log;
        private ServerCount _serverCount;

        public Initializer(MasterConfig config, DiscordShardedClient client) {
            _masterConfig = config;
            _client = client;
            _commands = new CommandService(new CommandServiceConfig() {
                DefaultRunMode = RunMode.Async
            });
            _log = new Log(_masterConfig, _client);
            _serverCount = new ServerCount(_masterConfig, _client);

            var postgreConfig = _masterConfig.PostgreSqlConfig;
            var mongoConfig = _masterConfig.MongoDbConfig;

            _services = new ServiceCollection()
                .AddSingleton(_masterConfig)
                .AddSingleton(_client)
                .AddSingleton<IDiscordClient>(_client)
                .AddSingleton(_commands)
                .AddSingleton(_log)
                .AddSingleton(_serverCount)
                .AddDbContext<TreeDiagramContext>(options => {
                    options.UseNpgsql(
                        string.Format("Server={0};Port=5432;Database={1};UserId={2};Password={3};",
                            postgreConfig.Hostname,
                            postgreConfig.Database,
                            postgreConfig.Username,
                            postgreConfig.Password
                        )
                    ).EnableSensitiveDataLogging().UseLazyLoadingProxies();
                }, ServiceLifetime.Transient)
                .AddSingleton(new MusicService(new MusicServiceConfig() {
                    Hostname = mongoConfig.Hostname,
                    Username = mongoConfig.Username,
                    Password = mongoConfig.Password,
                    EnableResync = true
                }))
                .AddSingleton<Analytics>()
                .BuildServiceProvider();
//                 .AddSingleton(Of Analytics) _
//                 .AddTransient(Of CommandUtils) _
//                 .AddSingleton(Of Events) _
//                 .AddTransient(Of RandomCat) _
//                 .AddSingleton(Of CommandManager) _
//                 .AddSingleton(Of FilterManager) _
//                 .AddSingleton(Of MusicManager) _
//                 .AddSingleton(Of PlayerManager) _
//                 .AddSingleton(Of TimerManager) _
//                 .AddSingleton(Of AntiCaps) _
//                 .AddSingleton(Of AntiUrl) _
//                 .AddTransient(Of YoutubeSearch) _
//                 .BuildServiceProvider()
            
            TaskScheduler.UnobservedTaskException += async (sender, e) => await UnobservedTaskAsync(e);
        }

        public async Task InitializeCommandsAsync() {
            _services.GetService<Analytics>();
//             _services.GetService(Of Analytics)()
//             _services.GetService(Of Events)()
//             _services.GetService(Of CommandManager)()
//             _services.GetService(Of AntiCaps)()
//             _services.GetService(Of AntiUrl)()

            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", "TreeDiagram Ready!"));
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", 
                $"{_commands.Commands.Count()} Commands Loaded!"));
        }

        private async Task UnobservedTaskAsync(UnobservedTaskExceptionEventArgs e) {
            e.SetObserved();

            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Error, "System", "An unobserved task threw an exception!", e.Exception));

            var output = new StringBuilder()
                .AppendLine("An unobserved task threw an exception!")
                .AppendLine(e.Exception.ToString());
            
            await _log.LogToBotLogAsync(output.ToString(), BotLogType.TaskScheduler);
        }
    }
}