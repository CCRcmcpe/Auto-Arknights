﻿using System;

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(RepeatMode), RepeatMode, null);
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
            Sanity sanity = Device.GetCurrentSanity();
            Log.Info($"当前理智[{sanity}]，需要理智[{_requiredSanity}]");
            return sanity.Value >= _requiredSanity;
        }

        private void InitRequiredSanity()
        {
            _requiredSanity = Device.GetRequiredSanity();
            Log.Info($"检测到此关卡需要[{_requiredSanity}]理智");
        }

        private void RunOnce()
        {
            Device.Clk("作战 开始");
            Device.Clk("作战 确认");
            Device.WaitAp("作战 完成");
            Device.Slp(3);
            Device.Clk(5, 5);
        }

        private void SpecifiedTimes()
        {
            for (var currentTime = 1; currentTime <= RepeatTime; currentTime++)
            {
                Log.Info($"正在执行第{currentTime}次刷关");
                RunOnce();
            }
        }

        private void SpecTimesWithWait()
        {
            InitRequiredSanity();
            for (var currentTime = 1; currentTime <= RepeatTime; currentTime++)
            {
                Log.Info($"正在执行第{currentTime}次刷关");
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
                    Log.Info($"关卡完成，目前已刷关{currentTime}次");
                }
                else
                {
                    break;
                }
        }

        private void WaitForSanityRecovery()
        {
            Log.Info("正在等待理智恢复...");
            while (Device.GetCurrentSanity().Value < Device.GetRequiredSanity())
                Device.Slp(5);
            Log.Info("...理智恢复完成");
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
                    Log.Info($"关卡完成，目前已刷关{currentTime}次");
                }
                else
                {
                    WaitForSanityRecovery();
                }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}