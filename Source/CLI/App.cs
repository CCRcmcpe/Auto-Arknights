using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.CLI.Properties;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extensions;
using Serilog;
using Console = System.Console;

namespace REVUnit.AutoArknights.CLI
{
    public class App
    {
        private readonly IConfiguration _config;
        private readonly RootCommand _rootCommand;
        private Arknights? _arknights;

        public App(IConfiguration config)
        {
            _config = config;

            _rootCommand = new RootCommand("Enter interactive mode")
            {
                new Command("combat")
                {
                    new Option("level"),
                    new Argument<int>("times"),
                    new Option<bool>("wait"),
                    new Option<bool>("use-potions"),
                    new Option<bool>("use-originites"),
                    new Option<int>("max-originites-usage")
                }.Also(c => c.Handler = CommandHandler.Create<string, int, bool, bool, bool, int>(Combat)) /*,
                new Command("collect")
                {
                    new Command("credit-points") {Handler = CommandHandler.Create(() => Console.WriteLine())}
                        .Also(c => c.AddAlias("credit"))
                        .Also(c => c.AddAlias("cp")),
                    new Command("infrastructure") {Handler = CommandHandler.Create(() => Console.WriteLine())}.Also(c =>
                        c.AddAlias("infra"))
                }*/
            }.Also(c => c.Handler = CommandHandler.Create(Interactive));
        }

        private void Interactive()
        {
            while (true)
            {
                string? args = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(args))
                {
                    Log.Information("请输入指令");
                    continue;
                }

                _rootCommand.Invoke(args);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void Combat(string? levelName, int times, bool wait, bool usePotions, bool useOriginites,
            int maxOriginitesUsage)
        {
            _arknights!.Combat.Run(levelName == null ? null : Level.FromName(levelName), new LevelCombatSettings
            {
                RepeatTimes = times,
                WaitWhenNoSanity = wait,
                UseSanityPotions = usePotions,
                UseOriginites = useOriginites,
                MaxOriginitesUsage = maxOriginitesUsage
            });
        }

        public void Start()
        {
            Log.Information(Resources.App_Starting);

            Log.Information("正在连接ADB");

            var adbDevice = new AdbDevice(_config["Adb:ExecutablePath"]);
            adbDevice.Connect(_config["Adb:DeviceSerial"]);

            Log.Information("ADB已连接");

            _arknights = Arknights.FromDevice(adbDevice);

            Log.Information(Resources.App_Started);
        }

        public void Run(string[] args)
        {
            _rootCommand.Invoke(args);

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