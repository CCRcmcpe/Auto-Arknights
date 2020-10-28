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
        private static readonly Random Random = new Random();
        private readonly Adb _adb;
        private readonly Assets _assets = new Assets();
        private readonly ImageRegister _register = new ImageRegister("Assets\\Cache");

        public Device(string adbPath, string adbRemote)
        {
            _adb = new Adb(adbPath);
            _adb.Connect(adbRemote);
        }

        public void Dispose()
        {
            _assets.Dispose();
            _register.Dispose();
        }

        public void Clk(string asset)
        {
            Clk(Asset(asset));
        }

        public void Clk(Mat model)
        {
            Clk(X.While(() => Loc(model), result => result.IsSucceed, TimeSpan.FromSeconds(2))!.CenterPoint);
        }

        public void Clk(int x, int y)
        {
            Clk(new Point(x, y));
        }

        public void Clk(Point point)
        {
            _adb.Click(Randomize(point));
        }

        public Sanity GetCurrentSanity()
        {
            return Ocr(ScreenArea.CurrentSanity, @"(\d+)\s*\/\s*(\d+)", matches =>
            {
                int[] numbers = matches.SelectCanParse<string, int>(int.TryParse).ToArray();
                int value, max;
                if (numbers.Length != 2 || (max = numbers[1]) > 150 || (value = numbers[0]) > max)
                    throw new Exception();
                return new Sanity(value, max);
            })!;
        }

        public int GetRequiredSanity()
        {
            return Ocr(ScreenArea.RequiredSanity, @"\d+", matches => int.Parse(matches[0]));
        }

        public LocateResult Loc(string expr)
        {
            Mat model = Asset(expr);
            return Loc(model);
        }

        public LocateResult Loc(Mat model)
        {
            using Mat scrn = Scrn();
            return _register.Locate(model, scrn);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Slp(double sec)
        {
            Thread.Sleep(TimeSpan.FromSeconds(sec));
        }

        public bool TestAp(string expr) => Loc(expr).IsSucceed;

        public void WaitAp(string expr, double waitSec = 3)
        {
            X.While(() => Loc(expr), result => result.IsSucceed, TimeSpan.FromSeconds(waitSec));
        }

        private Mat Asset(string expr) => _assets.Get(expr);

        private T Ocr<T>(ScreenArea area, string regex, Func<string[], T> parser, double waitSec = 1)
        {
            T ret = default;
            X.While(() =>
                    {
                        using Mat scrn = Scrn();
                        using Mat sub = area.Apply(scrn);
                        string result = TxOcr.Ocr(sub);
                        return Regex.Match(result, regex);
                    },
                    match =>
                    {
                        return match.Success &&
                               new TryParser<string[], T>(parser)
                                  .TryParse(match.Groups.Values.Select(it => it.Value).ToArray(),
                                            out ret);
                    }, TimeSpan.FromSeconds(waitSec));
            return ret!;
        }

        private static Point Randomize(Point point) =>
            new Point(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));

        private Mat Scrn()
        {
            return X.While(_adb.GetScreenShot, result => !result.Empty())!;
        }
    }
}