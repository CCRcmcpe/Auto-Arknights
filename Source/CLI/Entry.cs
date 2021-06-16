using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.CLI.Properties;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace REVUnit.AutoArknights.CLI
{
    public class Entry
    {
        private const string ConfigFilePath = "Auto Arknights CLI.config.yml";

        public static async Task Main(string[] args)
        {
            if (!File.Exists(ConfigFilePath))
            {
                await File.WriteAllBytesAsync(ConfigFilePath, Resources.DefaultConfig);
            }

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile(ConfigFilePath)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.Debug()
                .WriteTo.File($"Log/{DateTime.Now:yyyy-MM-dd HH.mm.ss}.log",
                    config.GetValue("Log:Level", LogEventLevel.Debug))
                .CreateLogger();

            var app = new App(config);

            Console.Clear();
#if DEBUG
            await app.Run(args);
#else
            try
            {
                await app.Run(args);
            }
            catch (Exception e)
            {
                Log.Fatal(e, Resources.Entry_FatalException);
                Console.ReadKey(true);
            }
#endif
            Log.CloseAndFlush();
        }
    }
}