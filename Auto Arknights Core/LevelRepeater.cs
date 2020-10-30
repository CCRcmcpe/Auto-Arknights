using System;
using System.Diagnostics.CodeAnalysis;

namespace REVUnit.AutoArknights.Core
{
    public class LevelRepeater : ArkTask
    {
        public enum Mode
        {
            SpecifiedTimes,
            SpecTimesWithWait,
            UntilNoSanity,
            WaitWhileNoSanity
        }

        private Sanity? _previousSanity;

        private int _requiredSanity;

        public LevelRepeater(Device device, Mode mode, int repeatTime) : base(device)
        {
            RepeatMode = mode;
            RepeatTime = repeatTime;
        }

        public Mode RepeatMode { get; set; }
        public int RepeatTime { get; set; }

        public override ExecuteResult Execute()
        {
            Log.Info("任务开始");

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
                default: throw new ArgumentOutOfRangeException(nameof(RepeatMode), RepeatMode, null);
            }

            Log.Info("任务完成");
            return ExecuteResult.Success();
        }

        private void EnsureSanityEnough()
        {
            if (!HaveEnoughSanity()) WaitForSanityRecovery();
        }

        private bool HaveEnoughSanity()
        {
            Sanity nowSanity = Device.GetCurrentSanity();
            // 防止OCR抽风
            if (_previousSanity != null)
            {
                int dSanity = _previousSanity.Value - nowSanity.Value; // 理智变动
                if (Math.Abs(nowSanity.Max - _previousSanity.Max) > 1  // 理智上限变动大于1
                 || dSanity > 0 && _requiredSanity - dSanity > 10)     // 没有嗑药，且理智对于一般情况下的刷关后理智差大于10
                    nowSanity = Device.GetCurrentSanity();
            }

            _previousSanity = nowSanity;

            Log.Info($"当前理智[{nowSanity}]，需要理智[{_requiredSanity}]");
            return nowSanity.Value >= _requiredSanity;
        }

        private void InitRequiredSanity()
        {
            _requiredSanity = Device.GetRequiredSanity();
            Log.Info($"检测到此关卡需要[{_requiredSanity}]理智");
        }

        private void RunOnce()
        {
            Device.Click("作战 开始");
            Device.Click("作战 确认");
            Device.WaitAppear("作战 完成");
            Device.Sleep(3);
            Device.Click(5, 5);
        }

        private void SpecifiedTimes()
        {
            for (var currentTime = 1; currentTime <= RepeatTime; currentTime++)
            {
                Log.Info($"开始第{currentTime}次刷关");
                RunOnce();
                Log.Info($"关卡完成，目前已刷关{currentTime}次");
            }
        }

        private void SpecTimesWithWait()
        {
            InitRequiredSanity();
            for (var currentTime = 1; currentTime <= RepeatTime; currentTime++)
            {
                Log.Info($"开始第{currentTime}次刷关");
                EnsureSanityEnough();
                RunOnce();
                Log.Info($"关卡完成，目前已刷关{currentTime}次");
            }
        }

        private void UntilNoSanity()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (true)
            {
                if (HaveEnoughSanity())
                {
                    currentTime++;
                    Log.Info($"开始第{currentTime}次刷关");
                    RunOnce();
                    Log.Info($"关卡完成，目前已刷关{currentTime}次");
                }
                else
                {
                    break;
                }
            }
        }

        private void WaitForSanityRecovery()
        {
            Log.Info("正在等待理智恢复...");

            while (Device.GetCurrentSanity().Value < _requiredSanity) Device.Sleep(5);

            Log.Info("...理智恢复完成");
        }

        [DoesNotReturn]
        private void WaitWhileNoSanity()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (true)
            {
                if (HaveEnoughSanity())
                {
                    currentTime++;
                    Log.Info($"开始第{currentTime}次刷关");
                    RunOnce();
                    Log.Info($"关卡完成，目前已刷关{currentTime}次");
                }
                else
                {
                    WaitForSanityRecovery();
                }
            }
        }
    }
}