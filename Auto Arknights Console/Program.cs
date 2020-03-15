using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;

namespace Auto_Arknights_Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile(".\\Config.json").AddCommandLine(args)
                .Build();
            using var automation = new Automation(config["Adb:Path"], config["Adb:Remote"]);
            (automation.UI.GetCurrentSanity().Value >= automation.UI.GetRequiredSanity()).Cl();
            // var cin = new Cin();
            // var enterTime = false;
            // automation.Schedule.Add(new RepeatLevelJob(
            //     cin.Get<RepeatLevelJob.Mode>("输入模式")
            //         .Also(mode => enterTime = mode == RepeatLevelJob.Mode.SpecifiedTimes),
            //     enterTime ? cin.Get<int>("输入刷关次数") : -1));
            // automation.Schedule.ExecuteAll();
        }
    }
}