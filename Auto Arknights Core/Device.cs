using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenCvSharp;
using REVUnit.AutoArknights.Core.CV;
using REVUnit.Crlib;
using REVUnit.Crlib.Extension;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public sealed class Device : IDisposable
    {
        private static readonly Random Random = new();
        private readonly Adb _adb;
        private readonly Assets _assets = new();
        private readonly ImageRegister _register = new("Assets/Cache");

        public Device(string exePath, string targetSerial) => _adb = new Adb(exePath, targetSerial);

        public void Dispose()
        {
            _assets.Dispose();
            _register.Dispose();
        }

        public void Click(string assetExpr)
        {
            Click(Asset(assetExpr));
        }

        public void Click(Mat model)
        {
            Click(X.While(() => Locate(model), result => result.IsSucceed, TimeSpan.FromSeconds(2))!.CenterPoint);
        }

        public void Click(int x, int y)
        {
            Click(new Point(x, y));
        }

        public void Click(Point point)
        {
            _adb.Click(Randomize(point));
        }

        public Sanity GetCurrentSanity()
        {
            return Ocr(ScreenArea.CurrentSanity, @"(\d+)\s*\/\s*(\d+)", matches =>
            {
                int[] numbers = matches.SelectCanParse<string, int>(int.TryParse).ToArray();
                if (numbers.Length != 2) throw new Exception();

                return new Sanity(numbers[0], numbers[1]);
            })!;
        }

        public int GetRequiredSanity()
        {
            return Ocr(ScreenArea.RequiredSanity, @"\d+", matches => int.Parse(matches[0]));
        }

        public LocateResult Locate(string assetExpr)
        {
            Mat model = Asset(assetExpr);
            return Locate(model);
        }

        public LocateResult Locate(Mat model)
        {
            using Mat scrn = Screenshot();
            return _register.Locate(model, scrn);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global （为了统一性，这里使用非静态函数）
        public void Sleep(double seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public bool TestAppear(string assetExpr) => Locate(assetExpr).IsSucceed;

        public void WaitAppear(string assetExpr, double waitSec = 3)
        {
            X.While(() => Locate(assetExpr), result => result.IsSucceed, TimeSpan.FromSeconds(waitSec));
        }

        private Mat Asset(string assetExpr) => _assets.Get(assetExpr);

        private T Ocr<T>(ScreenArea area, string regex, Func<string[], T> parser, double waitSec = 1)
        {
            T ret = default;
            X.While(() =>
                    {
                        using Mat scrn = Screenshot();
                        using Mat sub = area.Apply(scrn);
                        string result = TxOcr.Ocr(sub);
                        return Regex.Match(result, regex);
                    },
                    match =>
                    {
                        return match.Success &&
                               new TryParser<string[], T>(parser).TryParse(
                                   match.Groups.Values.Select(it => it.Value).ToArray(), out ret);
                    },
                    TimeSpan.FromSeconds(waitSec));
            return ret!;
        }

        private static Point Randomize(Point point) =>
            new(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));

        private Mat Screenshot() => _adb.GetScreenshot();
    }
}