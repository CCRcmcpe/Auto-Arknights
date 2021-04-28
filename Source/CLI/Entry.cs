using System;
using System.IO;
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

        public static void Main(string[] args)
        {
            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllBytes(ConfigFilePath, Resources.DefaultConfig);
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
            app.Run(args);
#else
            try
            {
                app.Run(args);
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