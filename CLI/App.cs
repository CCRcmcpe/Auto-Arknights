using System;
using System.Collections.Generic;
using REVUnit.AutoArknights.CLI.Properties;
using REVUnit.AutoArknights.Core;
using REVUnit.AutoArknights.Core.Tasks;
using REVUnit.Crlib.Extension;
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
            _ = UserInterface.I;
            _input = new Cin { AutoTrim = true };
        }

        public static Config Config { get; } = new(ConfigFilePath);
        public static App Instance => LazyInitializer.Value;

        public void Run()
        {
            if (!Library.CheckIfSupported()) throw new NotSupportedException(Resources.App_NotSupported);

            Plan? plan;
            do
            {
                plan = _input.Get(Resources.App_ParamsHint, (Cin.Parser<Plan>) ParseParams);
            } while (_input.LastException != null);

            if (plan == null) return;

            IArkTask[] tasks = plan.Tasks;

            PrintTasksSummary(tasks);
            XConsole.AnyKey(Resources.App_ReadyToExecute);

            ExecuteTasks(tasks);
            XConsole.AnyKey(Resources.App_AllTasksCompleted);
        }

        private Exception? ParseParams(string value, out Plan? result)
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
                Console.WriteLine($@"[{i}]> {task}");
            }

            Console.Write(Resources.App_TaskListFooter);
        }

        private static void ExecuteTasks(IReadOnlyList<IArkTask> tasks)
        {
            for (var taskId = 0; taskId < tasks.Count; taskId++)
            {
                IArkTask task = tasks[taskId];

                Log.Information(Resources.App_TaskBegin, taskId);
                ExecuteResult executeResult = task.Execute();

                if (executeResult.Successful)
                    Log.Information(Resources.App_TaskComplete, taskId, executeResult.Message);
                else
                    Log.Error(Resources.App_TaskFaulted, taskId, executeResult.Message);
            }
        }
    }
}