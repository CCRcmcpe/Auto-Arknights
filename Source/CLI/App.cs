using System;
using System.Collections.Generic;
using REVUnit.AutoArknights.CLI.Properties;
using REVUnit.AutoArknights.Core;
using REVUnit.AutoArknights.Core.Tasks;
using REVUnit.Crlib.Input;
using Serilog;

namespace REVUnit.AutoArknights.CLI
{
    public class App
    {
        private const string ConfigFilePath = "Auto Arknights CLI.config.json";
        private static readonly Lazy<App> LazyInitializer = new(() => new App());
        private readonly Cin _input;

        private App()
        {
            Library.Settings = Config;
            _input = new Cin();
        }

        public static Config Config { get; } = new(ConfigFilePath);
        public static App Instance => LazyInitializer.Value;

        public static void Initialize()
        {
            Remote.I.Initialize();
        }

        public void Run()
        {
            Plan? plan;
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
            Console.ReadKey(true);
        }

        private static Exception? ParseParams(string value, out Plan? result)
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
        }
    }
}