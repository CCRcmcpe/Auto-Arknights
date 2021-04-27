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
            Console.WriteLine(Resources.StartupLogo);

            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllBytes(ConfigFilePath, Resources.DefaultConfig);
            }

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile(ConfigFilePath)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.Debug(LogEventLevel.Debug)
                .WriteTo.File($"Log/{DateTime.Now:yyyy-MM-dd hh.mm.ss}.log",
                    config.GetValue("Log:Level", LogEventLevel.Debug))
                .CreateLogger();

            var app = new App(config);

            app.Start();

            if (args.Length == 0)
            {
                Log.Information("未输入参数，进入交互模式");
            }

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