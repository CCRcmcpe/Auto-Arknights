using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Serilog;
using static REVUnit.AutoArknights.Core.UserInterface;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public class FarmLevel : IArkTask
    {
        public enum FarmMode
        {
            SpecifiedTimes,
            SpecTimesWithWait,
            UntilNoSanity,
            WaitWhileNoSanity
        }

        private Sanity? _previousSanity;

        private int _requiredSanity;

        public FarmLevel(FarmMode farmMode, int times)
        {
            Mode = farmMode;
            Times = times;
        }

        public FarmMode Mode { get; set; }
        public int Times { get; set; }

        public ExecuteResult Execute()
        {
            int repeatedTimes = Mode switch
            {
                FarmMode.SpecifiedTimes    => SpecifiedTimes(),
                FarmMode.SpecTimesWithWait => SpecTimesWithWait(),
                FarmMode.UntilNoSanity     => UntilNoSanity(),
                FarmMode.WaitWhileNoSanity => WaitWhileNoSanity(),
                _                          => throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null)
            };
            return ExecuteResult.Success($"成功刷关{repeatedTimes}次");
        }

        private void EnsureSanityEnough()
        {
            if (!HaveEnoughSanity()) WaitForSanityRecovery();
        }

        private bool HaveEnoughSanity()
        {
            Sanity nowSanity = I.GetCurrentSanity();
            // 防止OCR抽风
            if (_previousSanity != null)
            {
                int dSanity = _previousSanity.Value - nowSanity.Value; // 理智变动
                if (Math.Abs(nowSanity.Max - _previousSanity.Max) > 1  // 理智上限变动大于1
                 || dSanity > 0 && _requiredSanity - dSanity > 10)     // 没有嗑药，且理智对于一般情况下的刷关后理智差大于10
                    nowSanity = I.GetCurrentSanity();
            }

            _previousSanity = nowSanity;

            Log.Information($"当前理智[{nowSanity}]，需要理智[{_requiredSanity}]");
            return nowSanity.Value >= _requiredSanity;
        }

        private void InitRequiredSanity()
        {
            _requiredSanity = I.GetRequiredSanity();
            Log.Information($"检测到此关卡需要[{_requiredSanity}]理智");
        }

        private static void RunOnce()
        {
            I.Graphical.Click("作战/开始");
            I.Graphical.Click("作战/确认");

            Utils.Sleep(Library.Settings.Intervals.BeforeVerifyInLevel);
            if (!I.Graphical.TestAppear("作战/接管作战"))
            {
                Log.Warning("未检测到代理指挥正常运行迹象！");
                Log.Warning("请检查是否在正常代理作战，如果是的话请增加检测代理正常前等待的时间，以避免假警告出现");
            }

            Utils.Sleep(50);
            while (I.Graphical.TestAppear("作战/接管作战")) { }

            Utils.Sleep(Library.Settings.Intervals.AfterLevelComplete);
            while (true)
            {
                I.Click(RelativeArea.LevelCompletedScreenCloseClick);
                Utils.Sleep(5);
                if (I.Graphical.TestAppear("作战/开始")) break;
            }
        }

        private int SpecifiedTimes()
        {
            var currentTime = 0;
            while (currentTime != Times)
            {
                Log.Information($"开始第{currentTime + 1}次刷关");
                RunOnce();
                Log.Information($"关卡完成，目前已刷关{++currentTime}次");
            }

            return currentTime;
        }

        private int SpecTimesWithWait()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (currentTime != Times)
            {
                Log.Information($"开始第{currentTime + 1}次刷关");
                EnsureSanityEnough();
                RunOnce();
                Log.Information($"关卡完成，目前已刷关{++currentTime}次");
            }

            return currentTime;
        }

        private int UntilNoSanity()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (true)
            {
                if (HaveEnoughSanity())
                {
                    Log.Information($"开始第{currentTime + 1}次刷关");
                    RunOnce();
                    Log.Information($"关卡完成，目前已刷关{++currentTime}次");
                }
                else
                {
                    break;
                }
            }

            return currentTime;
        }

        private void WaitForSanityRecovery()
        {
            Log.Information("正在等待理智恢复...");

            while (I.GetCurrentSanity().Value < _requiredSanity) Utils.Sleep(5);

            Log.Information("...理智恢复完成");
        }

        [DoesNotReturn]
        private int WaitWhileNoSanity()
        {
            InitRequiredSanity();
            var currentTimes = 0;
            while (true)
            {
                if (HaveEnoughSanity())
                {
                    currentTimes++;
                    Log.Information("开始第{times}次刷关", currentTimes);
                    RunOnce();
                    Log.Information("关卡完成，目前已刷关{times}次", currentTimes);
                }
                else
                {
                    WaitForSanityRecovery();
                }
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("刷关");
            switch (Mode)
            {
                case FarmMode.SpecifiedTimes:
                    b.Append($"{Times}次");
                    break;
                case FarmMode.SpecTimesWithWait:
                    b.Append($"{Times}次，当理智不足以完成时等待恢复");
                    break;
                case FarmMode.UntilNoSanity:
                    b.Append("直到理智耗尽");
                    break;
                case FarmMode.WaitWhileNoSanity:
                    b.Append("直到手动结束程序");
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return b.ToString();
        }
    }
}