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

            if (!Library.CheckIfSupported()) throw new NotSupportedException("CPU不支持AVX2指令集，无法运行本程序");

            Library.Settings = Config;

            Console.WriteLine(Resources.StartupLogo);
            Log.Information("正在初始化设备抽象层");
            _ = UserInterface.I;
            Log.Information("启动成功");
            Console.Clear();

            Parameters? prms = cin.Get(@"<模式>[刷关次数][后续操作]", ParseParameters);

            if (prms == null) return;

            Console.Write(@"
-[任务列表]---------------------------------------

");
            for (var i = 0; i < prms.Tasks.Length; i++)
            {
                IArkTask task = prms.Tasks[i];
                Console.WriteLine($@"[{i}]> {task}");
            }

            Console.Write(@"
--------------------------------------------------
");

            Log.Information("即将执行：{tasks}", prms.Tasks);
            XConsole.AnyKey();

            for (var taskId = 0; taskId < prms.Tasks.Length; taskId++)
            {
                IArkTask task = prms.Tasks[taskId];

                Log.Information("任务[{taskId}]：任务开始", taskId);
                ExecuteResult executeResult = task.Execute();

                if (executeResult.Succeed)
                    Log.Information("任务[{taskId}]完成，信息：{message}", taskId, executeResult.Message);
                else
                    Log.Error("任务[{taskId}]出现错误：{message}", taskId, executeResult.Message);
            }

            XConsole.AnyKey("所有任务完成");
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
                var mode = (FarmLevel.FarmMode) modeValue;
                if (!Enum.IsDefined(mode)) throw new ArgumentException("无效的模式，请输入 \"help\" 来获取快速帮助");

                var tasks = new List<IArkTask>();

                if (mode == FarmLevel.FarmMode.SpecifiedTimes || mode == FarmLevel.FarmMode.SpecTimesWithWait)
                {
                    var b = new StringBuilder();
                    while (true)
                    {
                        int v = reader.Read();
                        if (v == -1) break;
                        var c = (char) v;
                        if (char.IsDigit((char) v)) b.Append(c);
                    }

                    if (b.Length == 0) throw new ArgumentException("无效的刷关次数值，请输入 \"help\" 来获取快速帮助");

                    int repeatTimes = int.Parse(b.ToString());

                    var task = new FarmLevel(mode, repeatTimes);
                    tasks.Add(task);
                }
                else
                {
                    tasks.Add(new FarmLevel(mode, -1));
                }

                if (reader.Peek() == -1)
                    return new Parameters(tasks.ToArray()); // A mode value and maybe a repeat times number parsed

                tasks.AddRange(reader.ReadToEnd().Select(c => PostAction.Parse(c, parseSettings)));

                return new Parameters(tasks.ToArray());
            }
        }
    }
}