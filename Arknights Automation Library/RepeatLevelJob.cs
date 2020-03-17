using System;
using REVUnit.Crlib.Extension;

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

        public override ExecuteResult Execute(UI i)
        {
            var currentTime = 0;
            Func<int, bool> advanceCondiction = RepeatMode switch
            {
                Mode.SpecifiedTimes => t => t < RepeatTime,
                Mode.UntilNoSanity => _ => i.GetCurrentSanity().Value < i.GetRequiredSanity(),
                Mode.WaitWhileNoSanity => _ => X.While(() => i.GetCurrentSanity().Value < i.GetRequiredSanity(),
                    it => it, TimeSpan.FromSeconds(15)),
                _ => throw new ArgumentOutOfRangeException()
            };
            while (advanceCondiction(currentTime))
            {
                StandardProcess(i);
                currentTime++;
            }

            return ExecuteResult.Success();
        }

        private static void StandardProcess(UI i)
        {
            i.Clk("作战 开始");
            i.Clk("作战 确认");
            i.WaitAp("作战 完成");
            i.Slp(1.8);
            i.Clk(5, 5);
        }
    }
}