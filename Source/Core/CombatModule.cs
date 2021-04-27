using System;
using System.Text.RegularExpressions;
using System.Threading;
using Polly;
using Polly.Retry;
using REVUnit.AutoArknights.Core.Properties;
using Serilog;

namespace REVUnit.AutoArknights.Core
{
    public class CombatSettings
    {
        public int IntervalBeforeVerifyInLevel { get; set; } = 50;
        public double IntervalAfterLevelComplete { get; set; } = 5;
    }

    public class CombatModule
    {
        private static readonly Regex CurrentSanityRegex =
            new(@"(?<current>\d+)\s*\/\s*(?<max>\d+)", RegexOptions.Compiled);

        private readonly RetryPolicy<Sanity> _getCurrentSanityPolicy;

        private readonly Interactor _interactor;
        private Sanity? _lastGetSanityResult;
        private int _requiredSanity;

        internal CombatModule(Interactor interactor)
        {
            _interactor = interactor;
            _getCurrentSanityPolicy = Policy.HandleResult<Sanity>(sanity =>
            {
                if (_lastGetSanityResult == null) return false;
                int dSanity = _lastGetSanityResult.Value - sanity.Value;

                return Math.Abs(sanity.Max - _lastGetSanityResult.Max) > 1 // 理智上限变动大于1
                       || dSanity > 0 // 没有嗑药
                       && Math.Abs(_requiredSanity - dSanity) > 10; // 且理智对于一般情况下的刷关后理智差大于10
            }).WaitAndRetry(2, _ => TimeSpan.FromSeconds(2));
        }

        public CombatSettings Settings { get; set; } = new();

        public void Run(Level? level, LevelCombatSettings settings)
        {
            if (level != null)
            {
                // TODO Move to level
            }
            else
            {
                RunCurrentLevel(settings.RepeatTimes, settings.WaitWhenNoSanity);
            }
        }

        private void RunCurrentLevel()
        {
            _interactor.Click("Ops/Begin");
            Utils.Sleep(1);

            _interactor.Click("Ops/Start");
            Utils.Sleep(Settings.IntervalBeforeVerifyInLevel);

            if (!_interactor.TestAppear("Ops/TakeOver"))
            {
                Log.Warning(Resources.LevelFarming_Exception_AutoDeploy);
                Log.Warning(Resources.LevelFarming_Exception_AutoDeployHint,
                    Settings.IntervalBeforeVerifyInLevel);
            }

            Utils.Sleep(50);
            while (_interactor.TestAppear("Ops/TakeOver"))
            {
            }

            Utils.Sleep(Settings.IntervalAfterLevelComplete);
            while (true)
            {
                _interactor.Click(RelativeArea.LevelCompletedScreenCloseClick);
                Utils.Sleep(7);
                if (_interactor.TestAppear("Ops/Begin")) break;
            }
        }

        private void RunCurrentLevel(int times, bool waitWhenNoSanity)
        {
            if (waitWhenNoSanity)
            {
                _requiredSanity = GetRequiredSanity();
            }

            var currentTimes = 0;
            Func<bool> continueCondition = times switch
            {
                -1 => () => true,
                0 => () => GetCurrentSanity().Value < _requiredSanity,
                // ReSharper disable once AccessToModifiedClosure
                _ => () => currentTimes < times
            };

            while (continueCondition())
            {
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Begin, currentTimes + 1, times);
                if (waitWhenNoSanity || times == -1)
                {
                    while (GetCurrentSanity().Value < _requiredSanity)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }

                RunCurrentLevel();
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Complete, ++currentTimes, times);
            }
        }

        private Sanity GetCurrentSanity()
        {
            PolicyResult<Sanity> result = _getCurrentSanityPolicy.ExecuteAndCapture(() =>
            {
                string text = _interactor.Ocr(RelativeArea.CurrentSanity);
                Match match = CurrentSanityRegex.Match(text);
                if (!(int.TryParse(match.Groups["current"].Value, out int current) &&
                      int.TryParse(match.Groups["max"].Value, out int max)))
                {
                    throw new Exception(); // TODO
                }

                return new Sanity(current, max);
            });

            Sanity currentSanity;
            if (result.Result != null)
            {
                currentSanity = result.Result;
            }
            else
            {
                Log.Warning(Resources.LevelFarming_PossiblyOcrError);
                currentSanity = result.FinalHandledResult;
            }

            _lastGetSanityResult = currentSanity;
            return currentSanity;
        }

        private int GetRequiredSanity()
        {
            string text = _interactor.Ocr(RelativeArea.RequiredSanity);
            if (!int.TryParse(text[1..], out int requiredSanity)) throw new Exception();

            Log.Information(Resources.LevelFarming_RequiredSanity, requiredSanity);
            return requiredSanity;
        }
    }
}