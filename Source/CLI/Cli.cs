using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.CLI.Properties;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extensions;
using Serilog;
using Console = System.Console;
using Process = System.Diagnostics.Process;

namespace REVUnit.AutoArknights.CLI
{
    public class Cli
    {
        private readonly IConfiguration _config;
        private readonly RootCommand _rootCommand;

        private Game? _game;

        public Cli(IConfiguration config)
        {
            _config = config;

            _rootCommand = new RootCommand("Interactive mode")
            {
                new Option<bool>("--no-logo"),

                new Command("start")
                    {Handler = CommandHandler.Create(StartGame)},
                new Command("stop")
                    {Handler = CommandHandler.Create(StopGame)},
                new Command("combat")
                {
                    new Argument<int>("--times").Also(x => x.AddAlias("-t")),
                    new Option("--level").Also(x => x.AddAlias("-l")),
                    new Option<bool>("--wait").Also(x => x.AddAlias("-w")),
                    new Option<bool>("--potions").Also(x => x.AddAlias("-p")),
                    new Option<int>("--originites").Also(x => x.AddAlias("-o"))
                }.Also(c => c.Handler = CommandHandler.Create<int, string?, bool, bool, int>(Combat)),
                new Command("claim")
                {
                    new Command("task")
                        {Handler = CommandHandler.Create(() => _game!.ClaimTasks())},
                    new Command("cp")
                        {Handler = CommandHandler.Create(() => _game!.Infrastructure.ClaimCreditPoints())},
                    new Command("infra")
                        {Handler = CommandHandler.Create(() => _game!.Infrastructure.ClaimProducts())}
                }
            }.Also(c => c.Handler = CommandHandler.Create<bool>(Interactive));
        }

        public async Task Run(string[] args)
        {
            await _rootCommand.InvokeAsync(args);
        }

        private async Task AttachGame()
        {
            var adbDevice = new AdbDevice(_config["Adb:ExecutablePath"]);
            await adbDevice.Connect(_config["Adb:DeviceSerial"]);

            _game = await Game.FromDevice(adbDevice);
        }

        private async Task Combat(int times, string? levelName, bool wait, bool usePotions, int useOriginitesCount)
        {
            await _game!.Combat.Run(levelName == null ? null : Level.FromName(levelName),
                new LevelCombatSettings(times, wait, usePotions, useOriginitesCount));
        }

        private async Task Interactive(bool noLogo)
        {
            if (!noLogo)
            {
                Console.WriteLine(Resources.Logo);
                Console.WriteLine();
                await AttachGame();
                Console.Clear();
            }
            else
            {
                await AttachGame();
            }

            while (true)
            {
                // ReSharper disable once LocalizableElement
                Console.Write("> ");
                string? args = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(args))
                {
                    Log.Warning("未输入指令");
                    continue;
                }

                await _rootCommand.InvokeAsync(args);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static async Task RunCommandLine(string commandLine)
        {
            Process? process =
                Process.Start(new ProcessStartInfo("cmd.exe", "/c " + commandLine) {CreateNoWindow = true});
            if (process == null) throw new Exception("未能启动 cmd");

            var cts = new CancellationTokenSource(5000);
            Task waitForExit = process.WaitForExitAsync(cts.Token);
            await waitForExit;
            if (waitForExit.IsCanceled)
            {
                throw new Exception("命令行运行超时");
            }
        }

        private Task StartGame()
        {
            //TODO Throw if config value empty
            return RunCommandLine(_config["Remote:StartCommandLine"]);
        }

        private Task StopGame()
        {
            return RunCommandLine(_config["Remote:CloseCommandLine"]);
        }
    }
}