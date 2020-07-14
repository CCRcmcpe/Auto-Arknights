using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;
using REVUnit.Crlib.Input;

namespace REVUnit.AutoArknights.CLI
{
    public class AutoArknightsCli
    {
        private const string ConfigJson = ".\\Config.json";
        private readonly IConfiguration _config;

        public AutoArknightsCli()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            Console.BackgroundColor = ConsoleColor.Black;

            if (!Library.CheckIfSupported())
                throw new NotSupportedException("This program is not supported in this environment");

            if (!File.Exists(ConfigJson)) File.Create(ConfigJson);
            _config = new ConfigurationBuilder().AddJsonFile(ConfigJson).Build();

            Log.LogLevel = Log.Level.Get(Conf("Log:Level")) ??
                           throw new ConfigurationErrorsException("Value of Log:Level in configuration is invalid");
        }

        public void Run()
        {
            var cin = new Cin();
            var mode = cin.Get<LevelRepeater.Mode>("输入模式");
            int repeatTime = -1;
            if (mode == LevelRepeater.Mode.SpecifiedTimes || mode == LevelRepeater.Mode.SpecTimesWithWait)
                repeatTime = cin.Get<int>("输入刷关次数");

            using Device device = new Device(Conf("Adb:Path"), Conf("Adb:Remote"));
            new LevelRepeater(device, mode, repeatTime).Execute();

            Console.Beep();
            XConsole.AnyKey("所有任务完成");
        }

        private string Conf(string key)
        {
            return _config[key] ?? throw new ConfigurationErrorsException($"Configuration key {key} is required");
        }
    }
}