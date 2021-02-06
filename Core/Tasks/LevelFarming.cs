using System;
using System.Diagnostics.CodeAnalysis;
using REVUnit.AutoArknights.Core.Properties;
using Serilog;
using static REVUnit.AutoArknights.Core.UserInterface;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public class LevelFarming : IArkTask
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

        public LevelFarming(Mode farmMode, int times)
        {
            FarmMode = farmMode;
            Times = times;
        }

        public Mode FarmMode { get; set; }
        public int Times { get; set; }

        public ExecuteResult Execute()
        {
            int repeatedTimes = FarmMode switch
            {
                Mode.SpecifiedTimes    => SpecifiedTimes(),
                Mode.SpecTimesWithWait => SpecTimesWithWait(),
                Mode.UntilNoSanity     => UntilNoSanity(),
                Mode.WaitWhileNoSanity => WaitWhileNoSanity(),
                _                      => throw new ArgumentOutOfRangeException(nameof(Mode), FarmMode, null)
            };
            return ExecuteResult.Success(string.Format(Resources.LevelFarming_Success, repeatedTimes));
        }

        private void EnsureSanityEnough()
        {
            if (!HaveEnoughSanity()) WaitForSanityRecovery();
        }

        private bool HaveEnoughSanity()
        {
            Sanity currentSanity = I.GetCurrentSanity();
            // 防止OCR抽风
            if (_previousSanity != null)
            {
                int dSanity = _previousSanity.Value - currentSanity.Value; // 理智变动
                if (Math.Abs(currentSanity.Max - _previousSanity.Max) > 1  // 理智上限变动大于1
                 || dSanity > 0 && _requiredSanity - dSanity > 10)         // 没有嗑药，且理智对于一般情况下的刷关后理智差大于10
                    currentSanity = I.GetCurrentSanity();
            }

            _previousSanity = currentSanity;

            Log.Information(Resources.FarmLevel_CurrentSanity, currentSanity, _requiredSanity);
            return currentSanity.Value >= _requiredSanity;
        }

        private void InitRequiredSanity()
        {
            _requiredSanity = I.GetRequiredSanity();
            Log.Information(Resources.LevelFarming_RequiredSanity, _requiredSanity);
        }

        private static void RunOnce()
        {
            I.Graphical.Click("Ops/Begin");
            I.Graphical.Click("Ops/Start");

            Utils.Sleep(Library.Settings.Intervals.BeforeVerifyInLevel);
            if (!I.Graphical.TestAppear("Ops/TakeOver"))
            {
                Log.Warning(Resources.LevelFarming_Exception_AutoDeploy);
                Log.Warning(Resources.LevelFarming_Exception_AutoDeployHint,
                            Library.Settings.Intervals.BeforeVerifyInLevel);
            }

            Utils.Sleep(50);
            while (I.Graphical.TestAppear("Ops/TakeOver")) { }

            Utils.Sleep(Library.Settings.Intervals.AfterLevelComplete);
            while (true)
            {
                I.Click(RelativeArea.LevelCompletedScreenCloseClick);
                Utils.Sleep(5);
                if (I.Graphical.TestAppear("Ops/Begin")) break;
            }
        }

        private int SpecifiedTimes()
        {
            var currentTime = 0;
            while (currentTime != Times)
            {
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Begin, currentTime + 1, Times);
                RunOnce();
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Complete, ++currentTime, Times);
            }

            return currentTime;
        }

        private int SpecTimesWithWait()
        {
            InitRequiredSanity();
            var currentTime = 0;
            while (currentTime != Times)
            {
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Begin, currentTime + 1, Times);
                EnsureSanityEnough();
                RunOnce();
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Complete, ++currentTime, Times);
            }

            return currentTime;
        }

        private int UntilNoSanity()
        {
            InitRequiredSanity();
            var currentTimes = 0;
            while (true)
            {
                if (HaveEnoughSanity())
                {
                    Log.Information(Resources.LevelFarming_Unlimited_Begin, currentTimes + 1);
                    RunOnce();
                    Log.Information(Resources.LevelFarming_Unlimited_Complete, ++currentTimes);
                }
                else
                {
                    break;
                }
            }

            return currentTimes;
        }

        private void WaitForSanityRecovery()
        {
            Log.Information(Resources.LevelFarming_WaitingForSanityRecovery);

            while (I.GetCurrentSanity().Value < _requiredSanity) Utils.Sleep(5);

            Log.Information(Resources.LevelFarming_SanityRecovered);
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
                    Log.Information(Resources.LevelFarming_Unlimited_Begin, currentTimes + 1);
                    RunOnce();
                    Log.Information(Resources.LevelFarming_Unlimited_Complete, ++currentTimes);
                }
                else
                {
                    WaitForSanityRecovery();
                }
            }
        }

        public override string ToString()
        {
            return FarmMode switch
            {
                Mode.SpecifiedTimes    => string.Format(Resources.LevelFarming_Mode_SpecifiedTimes, Times),
                Mode.SpecTimesWithWait => string.Format(Resources.LevelFarming_Mode_SpecTimesWithWait, Times),
                Mode.UntilNoSanity     => Resources.LevelFarming_Mode_UntilNoSanity,
                Mode.WaitWhileNoSanity => Resources.LevelFarming_Mode_WaitWhileNoSanity,
                _                      => throw new ArgumentOutOfRangeException(nameof(FarmMode), FarmMode, null)
            };
        }
    }
}