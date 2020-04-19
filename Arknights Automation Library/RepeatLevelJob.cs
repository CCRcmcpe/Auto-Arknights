using System;
using System.Threading;

namespace REVUnit.AutoArknights.Core
{
    public class RepeatLevelJob : Job
    {
        public enum Mode
        {
            SpecifiedTimes,
            UntilNoSanity,
            WaitWhileNoSanity
        }

        public RepeatLevelJob(Mode mode, int repeatTime)
        {
            RepeatMode = mode;
            RepeatTime = repeatTime;
        }

        public Mode RepeatMode { get; set; }
        public int RepeatTime { get; set; }

        public override ExecuteResult Execute(UI ui)
        {
            Console.WriteLine(">>>任务开始");
            switch (RepeatMode)
            {
                case Mode.SpecifiedTimes:
                    SpecifiedTimes(ui, RepeatTime);
                    break;
                case Mode.UntilNoSanity:
                    UntilNoSanity(ui);
                    break;
                case Mode.WaitWhileNoSanity:
                    WaitWhileNoSanity(ui);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine(">>>任务完成");
            return ExecuteResult.Success();
        }

        private static void WaitWhileNoSanity(UI ui)
        {
            var currentTime = 0;
            int requiredSanity = ui.GetRequiredSanity();
            while (true)
            {
                Sanity sanity = ui.GetCurrentSanity();
                bool flag = sanity.Value >= requiredSanity;
                Console.WriteLine($">>当前理智[{sanity}]，需要理智[{requiredSanity}]，{(flag ? "继续" : "暂停")}");
                if (flag)
                {
                    RunOnce(ui);
                    currentTime++;
                    Console.WriteLine($">>关卡完成，目前已刷关{currentTime}次");
                }
                else
                {
                    Console.WriteLine(">>正在等待理智恢复...");
                    while (ui.GetCurrentSanity().Value - ui.GetRequiredSanity() < 0)
                        Thread.Sleep(TimeSpan.FromSeconds(15));
                }
            }
        }

        private static void UntilNoSanity(UI ui)
        {
            var currentTime = 0;
            int requiredSanity = ui.GetRequiredSanity();
            while (true)
            {
                Sanity sanity = ui.GetCurrentSanity();
                bool flag = sanity.Value >= requiredSanity;
                Console.WriteLine($">>当前理智[{sanity}]，需要理智[{requiredSanity}]，{(flag ? "继续" : "停止")}");
                if (flag)
                {
                    RunOnce(ui);
                    currentTime++;
                    Console.WriteLine($">>关卡完成，目前已刷关{currentTime}次");
                }
                else
                {
                    break;
                }
            }
        }

        private static void SpecifiedTimes(UI ui, int time)
        {
            for (var currentTime = 1; currentTime <= time; currentTime++)
            {
                Console.WriteLine($">>正在执行第{currentTime}次刷关");
                RunOnce(ui);
            }
        }

        private static void RunOnce(UI i)
        {
            i.Clk("作战 开始");
            i.Clk("作战 确认");
            i.WaitAp("作战 完成");
            i.Slp(2);
            i.Clk(5, 5);
        }
    }
}