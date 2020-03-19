using System;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Input;

namespace REVUnit.AutoArknights.CLI
{
    public class AutoArknights : IDisposable
    {
        private readonly Automation _automation;

        public AutoArknights()
        {
            if (!Library.CheckIfSupported())
            {
                Console.WriteLine("你的CPU不支持当前OpenCV构建，无法正常运行本程序，抱歉。");
                Console.ReadKey(true);
                return;
            }
            
            IConfiguration config = new ConfigurationBuilder().AddJsonFile(".\\Config.json").Build();
            _automation = new Automation(config["Adb:Path"], config["Adb:Remote"]);
        }

        public void Run()
        {
            var cin = new Cin();
            _automation.Schedule.Add(new RepeatLevelJob(RepeatLevelJob.Mode.SpecifiedTimes, cin.Get<int>("输入刷关次数")));
            _automation.Schedule.ExecuteAll();
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing!");
            _automation.Dispose();
        }
    }
}