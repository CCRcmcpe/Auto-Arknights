using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Serilog;

namespace REVUnit.AutoArknights.Core
{
    public class CombatModuleSettings
    {
        public double IntervalAfterLevelComplete { get; set; } = 5;
        public int IntervalBeforeVerifyInLevel { get; set; } = 50;
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
            if (level == null)
            {
                await RunCurrentSelectedLevel(settings.RepeatTimes, settings.WaitWhenNoSanity);
            }
        }

        private async Task<Sanity> GetCurrentSanity()
        {
            PolicyResult<Sanity> policyResult = await _getCurrentSanityPolicy.ExecuteAndCaptureAsync(async () =>
            {
                string text = await _i.Ocr(RelativeArea.CurrentSanityText);
                Match match = CurrentSanityRegex.Match(text);
                if (!(int.TryParse(match.Groups["current"].Value, out int current) &&
                      int.TryParse(match.Groups["max"].Value, out int max)))
                {
                    throw new Exception("无法识别理智值");
                }

                return new Sanity(current, max);
            });

            Sanity currentSanity;
            if (policyResult.Outcome == OutcomeType.Successful)
            {
                currentSanity = policyResult.Result;
            }
            else if (policyResult.FinalException != null)
            {
                throw policyResult.FinalException;
            }
            else
            {
                Log.Warning("OCR识别结果似乎有误");
                currentSanity = policyResult.FinalHandledResult;
            }

            _lastGetSanityResult = currentSanity;
            return currentSanity;
        }

        private async Task<int> GetRequiredSanity()
        {
            string text = await _i.Ocr(RelativeArea.RequiredSanityText);
            if (!int.TryParse(text[1..], out int requiredSanity)) throw new Exception();

            Log.Information("检测到此关卡需要[{RequiredSanity}]理智", requiredSanity);
            return requiredSanity;
        }

        private async Task RunCurrentSelectedLevel(int times, bool waitWhenNoSanity)
        {
            var currentTimes = 0;

            if (waitWhenNoSanity)
            {
                _requiredSanity = await GetRequiredSanity();
            }

            Func<Task<bool>> continueCondition = times switch
            {
                -1 => async () =>
                {
                    if ((await GetCurrentSanity()).Value < _requiredSanity)
                    {
                        Log.Information("正在等待理智恢复...");
                        while ((await GetCurrentSanity()).Value < _requiredSanity)
                        {
                            await Task.Delay(10000);
                        }

                        Log.Information("...理智恢复完成");
                    }

                    return true;
                },
                0 => async () => (await GetCurrentSanity()).Value < _requiredSanity,
                // ReSharper disable once AccessToModifiedClosure
                _ => () => Task.FromResult(currentTimes < times)
            };

            while (await continueCondition())
            {
                if (times > 0)
                {
                    Log.Information("开始第[{CurrentTimes}/{times}]次刷关", currentTimes + 1, times);
                }
                else
                {
                    Log.Information("开始第{CurrentTimes}次刷关", currentTimes + 1);
                }

                await RunCurrentSelectedLevel();

                if (times > 0)
                {
                    Log.Information("关卡完成，目前已刷关[{currentTimes}/{Times}]次", ++currentTimes, times);
                }
                else
                {
                    Log.Information("关卡完成，目前已刷关{CurrentTimes}次", ++currentTimes);
                }
            }
        }

        private async Task RunCurrentSelectedLevel()
        {
            await _i.ClickFor("Combat/Begin");
            await Task.Delay(1000);

            await _i.ClickFor("Combat/Start");
            await Task.Delay(TimeSpan.FromSeconds(Settings.IntervalBeforeVerifyInLevel));

            if (!await _i.TestAppear("Combat/TakeOver"))
            {
                Log.Warning("未检测到代理指挥正常运行迹象！");
                Log.Warning("请检查是否在正常代理作战，如果正常，请增加检测代理正常前等待的时间（现在为{WaitTime}s），以避免假警告出现",
                    Settings.IntervalBeforeVerifyInLevel);
            }

            while (await _i.TestAppear("Combat/TakeOver"))
            {
                await Task.Delay(5000);
            }

            await Task.Delay(TimeSpan.FromSeconds(Settings.IntervalAfterLevelComplete));

            do
            {
                await _i.Click(RelativeArea.LevelCompletedScreenCloseClick);
                await Task.Delay(5000);
            } while (!await _i.TestAppear("Combat/Begin"));
        }
    }
}