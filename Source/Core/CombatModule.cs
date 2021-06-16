using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using REVUnit.AutoArknights.Core.Properties;
using Serilog;

namespace REVUnit.AutoArknights.Core
{
    public class CombatModuleSettings
    {
        public int IntervalBeforeVerifyInLevel { get; set; } = 50;
        public double IntervalAfterLevelComplete { get; set; } = 5;
    }

    public class CombatModule
    {
        private static readonly Regex CurrentSanityRegex =
            new(@"(?<current>\d+)\s*\/\s*(?<max>\d+)", RegexOptions.Compiled);

        private readonly AsyncRetryPolicy<Sanity> _getCurrentSanityPolicy;

        private readonly Interactor _i;
        private Sanity? _lastGetSanityResult;
        private int _requiredSanity;

        internal CombatModule(Interactor interactor)
        {
            _i = interactor;
            _getCurrentSanityPolicy = Policy.HandleResult<Sanity>(sanity =>
            {
                if (_lastGetSanityResult == null) return false;
                int dSanity = _lastGetSanityResult.Value - sanity.Value;

                return Math.Abs(sanity.Max - _lastGetSanityResult.Max) > 1 // 理智上限变动大于1
                       || dSanity > 0 // 没有嗑药
                       && Math.Abs(_requiredSanity - dSanity) > 10; // 且理智对于一般情况下的刷关后理智差大于10
            }).WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(2));
        }

        public CombatModuleSettings Settings { get; set; } = new();

        public async Task Run(Level? level, LevelCombatSettings settings)
        {
            if (level != null)
            {
                // TODO Move to level
            }
            else
            {
                await RunCurrentLevel(settings.RepeatTimes, settings.WaitWhenNoSanity);
            }
        }

        private async Task<Sanity> GetCurrentSanity()
        {
            PolicyResult<Sanity> result = await _getCurrentSanityPolicy.ExecuteAndCaptureAsync(async () =>
            {
                string text = await _i.Ocr(RelativeArea.CurrentSanityText);
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

        private async Task<int> GetRequiredSanity()
        {
            string text = await _i.Ocr(RelativeArea.RequiredSanityText);
            if (!int.TryParse(text[1..], out int requiredSanity)) throw new Exception();

            Log.Information(Resources.LevelFarming_RequiredSanity, requiredSanity);
            return requiredSanity;
        }

        private async Task RunCurrentLevel()
        {
            await _i.Click("Combat/Begin");
            await Utils.Delay(1);

            await _i.Click("Combat/Start");
            await Utils.Delay(Settings.IntervalBeforeVerifyInLevel);

            if (!await _i.TestAppear("Combat/TakeOver"))
            {
                Log.Warning(Resources.LevelFarming_Exception_AutoDeploy);
                Log.Warning(Resources.LevelFarming_Exception_AutoDeployHint,
                    Settings.IntervalBeforeVerifyInLevel);
            }

            while (await _i.TestAppear("Combat/TakeOver"))
            {
                await Utils.Delay(5);
            }

            await Utils.Delay(Settings.IntervalAfterLevelComplete);
            while (true)
            {
                _i.Click(RelativeArea.LevelCompletedScreenCloseClick);
                await Utils.Delay(7);
                if (await _i.TestAppear("Combat/Begin")) break;
            }
        }

        private async Task RunCurrentLevel(int times, bool waitWhenNoSanity)
        {
            if (waitWhenNoSanity)
            {
                _requiredSanity = await GetRequiredSanity();
            }

            var currentTimes = 0;
            Func<Task<bool>> continueCondition = times switch
            {
                -1 => () => Task.FromResult(true),
                0 => async () => (await GetCurrentSanity()).Value < _requiredSanity,
                // ReSharper disable once AccessToModifiedClosure
                _ => () => Task.FromResult(currentTimes < times)
            };

            while (await continueCondition())
            {
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Begin, currentTimes + 1, times);
                if (waitWhenNoSanity || times == -1)
                {
                    while ((await GetCurrentSanity()).Value < _requiredSanity)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }

                await RunCurrentLevel();
                Log.Information(Resources.LevelFarming_SpecifiedTimes_Complete, ++currentTimes, times);
            }
        }
    }
}