using System;
using System.Threading;

namespace REVUnit.AutoArknights.Core
{
    public class RepeatLevelJob : Job
    {
        public enum Mode
        {
            SpecifiedTimes,
            SpecTimesWithWait,
            UntilNoSanity,
            WaitWhileNoSanity
        }

        private int _requiredSanity;

        public RepeatLevelJob(UI ui, Mode mode, int repeatTime) : base(ui)
        {
            RepeatMode = mode;
            RepeatTime = repeatTime;
        }

        public Mode RepeatMode { get; set; }
        public int RepeatTime { get; set; }

        public override ExecuteResult Execute()
        {
            Log.Info("任务开始", withTime: true);

            switch (RepeatMode)
            {
                case Mode.SpecifiedTimes:
                    SpecifiedTimes();
                    break;
                case Mode.SpecTimesWithWait:
                    SpecTimesWithWait();
                    break;
                case Mode.UntilNoSanity:
                    UntilNoSanity();
                    break;
                case Mode.WaitWhileNoSanity:
                    WaitWhileNoSanity();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Log.Info("任务完成", withTime: true);
            return ExecuteResult.Success();
        }

        private void InitRequiredSanity()
        {
            _requiredSanity = Ui.GetRequiredSanity();
            Log.Info($"检测到此关卡需要[{_requiredSanity}]理智");
        }

        private bool HaveEnoughSanity()
        {
            Sanity sanity = Ui.GetCurrentSanity();
            Log.Info($"当前理智[{sanity}]，需要理智[{_requiredSanity}]");
            return sanity.Value >= _requiredSanity;
        }

        private void EnsureSanityEnough()
        {
            if (!HaveEnoughSanity()) WaitForSanityRecovery();
        }

        private void WaitForSanityRecovery()
        {
            Log.Info("正在等待理智恢复...", withTime: true);
            while (Ui.GetCurrentSanity().Value < Ui.GetRequiredSanity())
                Thread.Sleep(TimeSpan.FromSeconds(10));
            Log.Info("...理智恢复完成", withTime: true);
        }

        private void SpecifiedTimes()
        {
            for (var currentTime = 1; currentTime <= RepeatTime; currentTime++)
            {
                Log.Info($"正在执行第{currentTime}次刷关", withTime: true);
                RunOnce();
            }
        }

        private void SpecTimesWithWait()
        {
            InitRequiredSanity();
            for (var currentTime = 1; currentTime <= RepeatTime; currentTime++)
            {
                Log.Info($"正在执行第{currentTime}次刷关", withTime: true);
                EnsureSanityEnough();
                RunOnce();
            }
        }

        private void UntilNoSanity()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (true)
                if (HaveEnoughSanity())
                {
                    RunOnce();
                    currentTime++;
                    Log.Info($"关卡完成，目前已刷关{currentTime}次", withTime: true);
                }
                else
                {
                    break;
                }
        }

        private void WaitWhileNoSanity()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (true)
                if (HaveEnoughSanity())
                {
                    RunOnce();
                    currentTime++;
                    Log.Info($"关卡完成，目前已刷关{currentTime}次", withTime: true);
                }
                else
                {
                    WaitForSanityRecovery();
                }
        }

        private void RunOnce()
        {
            Ui.Clk("作战 开始");
            Ui.Clk("作战 确认");
            Ui.WaitAp("作战 完成");
            UI.Slp(2);
            Ui.Clk(5, 5);
        }
    }
}