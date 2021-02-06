using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private static readonly Lazy<App> LazyInitializer = new(new App());

        private App() { }

        public static App Instance => LazyInitializer.Value;
        public Config Config { get; } = new(ConfigFilePath);

        public void Run()
        {
            var cin = new Cin { AutoTrim = true };

            if (!Library.CheckIfSupported()) throw new NotSupportedException(Resources.App_NotSupported);

            Library.Settings = Config;

            Console.WriteLine(Resources.StartupLogo);
            Log.Information(Resources.App_Starting);
            _ = UserInterface.I;
            Log.Information(Resources.App_Started);
            Console.Clear();

            Parameters? prms = cin.Get(Resources.App_ParamsHint, ParseParameters);

            if (prms == null) return;

            Console.Write(Resources.App_TaskListHeader);

            for (var i = 0; i < prms.Tasks.Length; i++)
            {
                IArkTask task = prms.Tasks[i];
                Console.WriteLine($@"[{i}]> {task}");
            }

            Console.Write(Resources.App_TaskListFooter);

            XConsole.AnyKey(Resources.App_ReadyToExecute);

            for (var taskId = 0; taskId < prms.Tasks.Length; taskId++)
            {
                IArkTask task = prms.Tasks[taskId];

                Log.Information(Resources.App_TaskBegin, taskId);
                ExecuteResult executeResult = task.Execute();

                if (executeResult.Successful)
                    Log.Information(Resources.App_TaskComplete, taskId, executeResult.Message);
                else
                    Log.Error(Resources.App_TaskFaulted, taskId, executeResult.Message);
            }

            XConsole.AnyKey(Resources.App_AllTasksCompleted);
        }

        private Parameters ParseParameters(string s)
        {
            if (s != "help") return Parameters.Parse(s, Config);

            Console.WriteLine(Resources.QuickHelpMessage);
            throw new Exception();
        }

        private class Parameters
        {
            public Parameters(IArkTask[] tasks) => Tasks = tasks;
            public IArkTask[] Tasks { get; }

            public static Parameters Parse(string value, ISettings parseSettings)
            {
                var reader = new StringReader(value);

                int modeValue = reader.Read() - '0';
                var mode = (LevelFarming.Mode) modeValue;
                if (!Enum.IsDefined(mode)) throw new ArgumentException(Resources.Parameters_Exception_InvalidMode);

                var tasks = new List<IArkTask>();

                if (mode == LevelFarming.Mode.SpecifiedTimes || mode == LevelFarming.Mode.SpecTimesWithWait)
                {
                    var b = new StringBuilder();
                    while (true)
                    {
                        int v = reader.Read();
                        if (v == -1) break;
                        var c = (char) v;
                        if (char.IsDigit((char) v)) b.Append(c);
                    }

                    if (b.Length == 0) throw new ArgumentException(Resources.Parameters_Exception_InvalidTimes);

                    int repeatTimes = int.Parse(b.ToString());

                    var task = new LevelFarming(mode, repeatTimes);
                    tasks.Add(task);
                }
                else
                {
                    tasks.Add(new LevelFarming(mode, -1));
                }

                if (reader.Peek() == -1)
                    return new Parameters(tasks.ToArray()); // A mode value and maybe a repeat times number parsed

                tasks.AddRange(reader.ReadToEnd().Select(c => PostAction.Parse(c, parseSettings)));

                return new Parameters(tasks.ToArray());
            }
        }
    }
}