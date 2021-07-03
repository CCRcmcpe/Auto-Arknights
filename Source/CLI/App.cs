using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
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
    public class App
    {
        private readonly IConfiguration _config;
        private readonly RootCommand _rootCommand;

        private Game? _game;

        public App(IConfiguration config)
        {
            _config = config;

            _rootCommand = new RootCommand("Interactive mode")
            {
                new Option<bool>("--no-logo"),

                new Command("start").Also(c =>
                    c.Handler = CommandHandler.Create(() => RunCommandLine(_config["Remote:StartCommandLine"]))),
                new Command("close-remote").Also(c => c.Handler = CommandHandler.Create(() =>
                    RunCommandLine(_config["Remote:CloseCommandLine"] ?? throw new Exception()))),
                new Command("shutdown-local").Also(c =>
                    c.Handler = CommandHandler.Create(() => RunCommandLine("shutdown /p"))),
                new Command("reboot-local").Also(c =>
                    c.Handler = CommandHandler.Create(() => RunCommandLine("shutdown /r /t 0"))),
                new Command("combat")
                {
                    new Option("--level"),
                    new Argument<int>("--times"),
                    new Option<bool>("--wait"),
                    new Option<bool>("--use-potions"),
                    new Option<bool>("--use-originites"),
                    new Option<int>("--max-originites-usage")
                }.Also(c => c.Handler = CommandHandler.Create<string, int, bool, bool, bool, int>(Combat)),
                new Command("claim")
                {
                    new Command("task")
                    {
                        Handler = CommandHandler.Create(() => _game!.ClaimTasks())
                    },
                    new Command("cp")
                    {
                        Handler = CommandHandler.Create(() => _game!.Infrastructure.ClaimCreditPoints())
                    },
                    new Command("infra")
                    {
                        Handler = CommandHandler.Create(() => _game!.Infrastructure.ClaimProducts())
                    }
                }
            }.Also(c => c.Handler = CommandHandler.Create<bool>(Interactive));
        }

        public async Task Run(string[] args)
        {
            await _rootCommand.InvokeAsync(args);

            /*Plan? plan;
            do
            {
                plan = _input.Get(Resources.App_ParamsHint, (Cin.Parser<Plan>) ParseParams);
            } while (_input.LastException != null);

            if (plan == null) return;

            IArkTask[] tasks = plan.Tasks;

            PrintTasksSummary(tasks);

            Console.WriteLine(Resources.App_ReadyToExecute);
            Console.ReadKey(true);

            ExecuteTasks(tasks);

            Console.WriteLine(Resources.App_AllTasksCompleted);
            Console.ReadKey(true);*/
        }

        private async Task AttachArknights()
        {
            var adbDevice = new AdbDevice(_config["Adb:ExecutablePath"]);
            await adbDevice.Connect(_config["Adb:DeviceSerial"]);

            _game = await Game.FromDevice(adbDevice);
        }

        private async Task Combat(string? levelName, int times, bool wait, bool usePotions, bool useOriginites,
            int maxOriginitesUsage)
        {
            await _game!.Combat.Run(levelName == null ? null : Level.FromName(levelName), new LevelCombatSettings
            {
                RepeatTimes = times,
                WaitWhenNoSanity = wait,
                UseSanityPotions = usePotions,
                UseOriginites = useOriginites,
                MaxOriginitesUsage = maxOriginitesUsage
            });
        }

        private async Task Interactive(bool noLogo)
        {
            if (!noLogo)
            {
                Console.WriteLine(Resources.Logo);
                Console.WriteLine();
                await AttachArknights();
                Console.Clear();
            }
            else
            {
                await AttachArknights();
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

        private static void RunCommandLine(string commandLine)
        {
            Process? process =
                Process.Start(new ProcessStartInfo("cmd.exe", "/c " + commandLine) {CreateNoWindow = true});
            if (process == null) throw new Exception("无法启动cmd");

            if (!process.WaitForExit(5000))
            {
                throw new Exception("命令行运行超时");
            }
        }

        /*private static Exception? ParseParams(string value, out Plan? result)
        {
            if (value != "help")
            {
                result = Plan.Parse(value, Config);
                return null;
            }

            Console.WriteLine(Resources.QuickHelpMessage);
            result = null;
            return new Exception();
        }

        private static void PrintTasksSummary(IReadOnlyList<IArkTask> tasks)
        {
            Console.Write(Resources.App_TaskListHeader);

            for (var i = 0; i < tasks.Count; i++)
            {
                IArkTask task = tasks[i];
                Console.WriteLine($@"[{i + 1}]> {task}");
            }

            Console.Write(Resources.App_TaskListFooter);
        }

        private static void ExecuteTasks(IReadOnlyList<IArkTask> tasks)
        {
            for (var i = 0; i < tasks.Count; i++)
            {
                IArkTask task = tasks[i];

                Log.Information(Resources.App_TaskBegin, i + 1);
                ExecuteResult executeResult = task.Execute();

                if (executeResult.Successful)
                    Log.Information(Resources.App_TaskComplete, i, executeResult.Message);
                else
                    Log.Error(Resources.App_TaskFaulted, i, executeResult.Message);
            }
        }*/
    }
}