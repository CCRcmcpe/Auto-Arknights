using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.CLI.Properties;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        private static IConfiguration? _config;
        private const string ConfigFilePath = "Auto Arknights CLI.config.yml";

        [MemberNotNull(nameof(_config))]
        public static void InitConfig()
        {
            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllBytes(ConfigFilePath, Resources.DefaultConfig);
            }

            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile(ConfigFilePath)
                .Build();
        }

        public static void InitLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
#if DEBUG
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
#else
                .WriteTo.Console(theme: AnsiConsoleTheme.Code,
                    restrictedToMinimumLevel: _config.GetValue("Log:Level", LogEventLevel.Information))
#endif
                .WriteTo.Debug()
                .WriteTo.File($"Log/{DateTime.Now:yyyy-MM-dd HH.mm.ss}.log")
                .CreateLogger();
        }

        public static async Task Main(string[] args)
        {
            InitConfig();
            InitLogger();

            var cli = new Cli(_config);
#if DEBUG
            await cli.Run(args);
#else
            try
            {
                await cli.Run(args);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "出现致命错误");
                Console.ReadKey(true);
            }
#endif
            Log.CloseAndFlush();
        }
    }
}