using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.GUI.Core;
using REVUnit.Crlib.Extension;
using REVUnit.Crlib.Input;

namespace REVUnit.AutoArknights.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile(".\\Config.json").AddCommandLine(args)
                .Build();
            using var automation = new Automation(config["Adb:Path"], config["Adb:Remote"]);
            var cin = new Cin();
            automation.Schedule.Add(new RepeatLevelJob(RepeatLevelJob.Mode.SpecifiedTimes, cin.Get<int>("输入刷关次数")));
            automation.Schedule.ExecuteAll();
        }
    }
}