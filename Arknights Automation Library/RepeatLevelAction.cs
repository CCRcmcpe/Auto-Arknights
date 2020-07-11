using System;
using System.Threading;

namespace REVUnit.AutoArknights.Core
{
    public class RepeatLevelAction : ArkAction
    {
        public enum Mode
        {
            SpecifiedTimes,
            SpecTimesWithWait,
            UntilNoSanity,
            WaitWhileNoSanity
        }

        private Interactor _ia;

        private int _requiredSanity;

        public RepeatLevelAction(Mode mode, int repeatTime)
        {
            RepeatMode = mode;
            RepeatTime = repeatTime;
        }

        public Mode RepeatMode { get; set; }
        public int RepeatTime { get; set; }

        public override ExecuteResult Execute(Interactor interactor)
        {
            Log.Info("任务开始", withTime: true);
            _ia = interactor;

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
            _ia = null;
            return ExecuteResult.Success();
        }

        private void EnsureSanityEnough()
        {
            if (!HaveEnoughSanity()) WaitForSanityRecovery();
        }

        private bool HaveEnoughSanity()
        {
            Sanity sanity = _ia.GetCurrentSanity();
            Log.Info($"当前理智[{sanity}]，需要理智[{_requiredSanity}]");
            return sanity.Value >= _requiredSanity;
        }

        private void InitRequiredSanity()
        {
            _requiredSanity = _ia.GetRequiredSanity();
            Log.Info($"检测到此关卡需要[{_requiredSanity}]理智");
        }

        private void RunOnce()
        {
            _ia.Clk("作战 开始");
            _ia.Clk("作战 确认");
            _ia.WaitAp("作战 完成");
            _ia.Slp(2);
            _ia.Clk(5, 5);
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

        private void WaitForSanityRecovery()
        {
            Log.Info("正在等待理智恢复...", withTime: true);
            while (_ia.GetCurrentSanity().Value < _ia.GetRequiredSanity())
                Thread.Sleep(TimeSpan.FromSeconds(10));
            Log.Info("...理智恢复完成", withTime: true);
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

            // ReSharper disable once FunctionNeverReturns
        }
    }
}