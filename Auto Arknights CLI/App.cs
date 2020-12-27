using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using REVUnit.AutoArknights.Core;
using REVUnit.AutoArknights.Core.Tasks;
using REVUnit.Crlib.Extension;
using REVUnit.Crlib.Input;
using Serilog;
using Serilog.Events;

namespace REVUnit.AutoArknights.CLI
{
    public class App
    {
        private const string ConfigFilePath = "Auto Arknights CLI.config.json";

        private const string Logo =
            @"                                                                                                    
                                   ,                                                                
            (                      @@(                                       .                      
            @@@       /            @@@(  /@,                                  @@@.                  
            @@@  @@@  @@@          @@@/        (@@@                       @@/ @@                    
            @@@  @@@  @@@   ,@@@   @@@/        (@@@        @(             @@@    (@*                
            @@@  @@@  @@@   ,@@@   @@@/        (@@@         @@@           @@@  @@%   @@@            
            @@@ .@@@  @@@*  ,@@@   @@@@#&.     (@@@ .%@@#    @@%          @@@   @@@  @@@            
            @@@  @@@  @@@   ,@@@   @@@/        @@@@       @@@//@*         @@@&       @@@ @@         
    ,@@@,   @@@  @@@  @@@   ,@@@   @@@/        #@@@      ,@@@         #@. @@@ %    /@@@@       (@@  
            (@@@ @@@ %@@@&  ,@@@   @@@/        (@@@      (@@@            (@@   @@@   @@@            
                     @@@    ,@@@   @@@/        (@@       @@@@     @@@@   @@@    @@@  @@@            
                    @@@     ,@@@   @@&       @@@        *@@@      @@@@  @@.          @@@            
                   %@,      ,@@                         @@@       @@@&           %&..@(             
                          .@@                         .@@&        @@@,             #(               
                                                     @&      @   ,@*                                
                                                               %@,                                  
                                                                                                    
    使用本程序后果自行承担      /@@@@@@@@@@@@@@/Auto Arknights/@@@@@@@@@@@@@#           REVUnit 2020   
                                                                                                    

";

        private static readonly Lazy<App> _lazyInitializer = new(new App());

        private App()
        {
            
        }

        public static App Instance => _lazyInitializer.Value;
        public Config Config { get; } = new(ConfigFilePath);

        public void Run()
        {
            if (!Library.CheckIfSupported()) throw new NotSupportedException("你当前的CPU不支持AVX2指令集，无法运行本程序");
            var cin = new Cin { AutoTrim = true };

            Console.Write(Logo);
            Log.Information("正在初始化设备抽象层");
            UserInterface.Initialize(Config.Remote_AdbExecutable, Config.Remote_Address);
            Log.Information("启动成功");
            Console.Clear();

            Parameters parameters = cin.Get(@"<\d: 模式>[\d+: 刷关次数][\w+: 后续操作]", Parameters.Parse) ??
                                    throw new Exception("意外的EOF");

            Console.Write(@"
-[任务列表]---------------------------------------

");
            for (var i = 0; i < parameters.Tasks.Length; i++)
            {
                IArkTask task = parameters.Tasks[i];
                Console.WriteLine($@"[{i}]> {task}");
            }

            Console.Write(@"
--------------------------------------------------

");
            
            XConsole.AnyKey();
            Log.Information("即将执行{tasks}",parameters.Tasks);

            for (var i = 0; i < parameters.Tasks.Length; i++)
            {
                IArkTask task = parameters.Tasks[i];

                Log.Information("任务[{number}]: 任务开始", i);
                ExecuteResult executeResult = task.Execute();

                const string info = "任务[{number}]: {executeResult.Message}";
                if (executeResult.Succeed)
                    Log.Information(info);
                else
                    Log.Error(info);
            }

            XConsole.AnyKey("所有任务完成");
        }

        private class Parameters
        {
            public Parameters(IArkTask[] tasks) => Tasks = tasks;
            public IArkTask[] Tasks { get; }

            public static Parameters Parse(string value)
            {
                var reader = new StringReader(value);

                int modeValue = reader.Read() - '0';
                var _mode = (FarmLevel.FarmMode) modeValue;
                if (!Enum.IsDefined(_mode)) throw new ArgumentException("无效模式");

                var tasks = new List<IArkTask>();

                if (_mode == FarmLevel.FarmMode.SpecifiedTimes || _mode == FarmLevel.FarmMode.SpecTimesWithWait)
                {
                    var b = new StringBuilder();
                    while (true)
                    {
                        int v = reader.Read();
                        if (v == -1) break;
                        var c = (char) v;
                        if (char.IsDigit((char) v)) b.Append(c);
                    }

                    if (b.Length == 0) throw new ArgumentException("无效的刷关次数值");

                    int repeatTimes = int.Parse(b.ToString());

                    var task = new FarmLevel(_mode, repeatTimes);
                    tasks.Add(task);
                }
                else
                {
                    tasks.Add(new FarmLevel(_mode, -1));
                }

                if (reader.Peek() == -1)
                    return new Parameters(tasks.ToArray()); // A mode value and maybe a repeat times number parsed

                string postActions = reader.ReadToEnd();
                IEnumerable<IArkTask> _postActions = postActions.Select<char, IArkTask>();
                tasks.AddRange(_postActions);

                return new Parameters(tasks.ToArray());
            }
            
            private static 
        }
    }
}