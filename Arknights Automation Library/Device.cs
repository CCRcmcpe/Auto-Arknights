using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenCvSharp;
using REVUnit.Crlib;
using REVUnit.Crlib.Extension;
using REVUnit.ImageLocator;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public class Device : IDisposable
    {
        private static readonly Random Random = new Random();
        private readonly Adb _adb;
        private readonly Assets _assets = new Assets();

        private readonly FMLocator _loc = new FMLocator(Feature2DType.Sift, "Assets\\Cache");

        public Device(string adbPath, string adbRemote)
        {
            _adb = new Adb(adbPath);
            if (!_adb.Connect(adbRemote)) throw new AdbException("Cannot connect to target device");
        }

        public void Dispose()
        {
            _loc.Dispose();
            _assets.Dispose();
        }

        public void Clk(string asset)
        {
            Clk(Asset(asset));
        }

        public void Clk(Mat model)
        {
            Clk(X.While(() => Loc(model), result => result.Succeed, TimeSpan.FromSeconds(2))!.CenterPoint);
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
            return Ocr(scrn => ScreenArea.CurrentSanity.Apply(scrn), @"(\d+)\s*\/\s*(\d+)",
                (string[] arr, out Sanity? result) =>
                {
                    int[] numbers = arr.SelectCanParse<string, int>(int.TryParse).ToArray();
                    bool valid = numbers.Length == 2;
                    result = valid ? new Sanity(numbers[0], numbers[1]) : null;
                    return valid;
                })!;
        }

        public int GetRequiredSanity()
        {
            return Ocr(scrn => ScreenArea.RequiredSanity.Apply(scrn), @"\d+",
                (string[] arr, out int result) => int.TryParse(arr[0], out result));
        }

        public LocateResult Loc(string expr)
        {
            Mat model = Asset(expr);
            return Loc(model);
        }

        public LocateResult Loc(Mat model)
        {
            using Mat scrn = Scrn();
            return _loc.Locate(model, scrn);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Slp(double sec)
        {
            Thread.Sleep(TimeSpan.FromSeconds(sec));
        }

        public bool TestAp(string expr)
        {
            return Loc(expr).Succeed;
        }

        public void WaitAp(string expr, double waitSec = 3)
        {
            X.While(() => Loc(expr), result => result.Succeed, TimeSpan.FromSeconds(waitSec));
        }

        private Mat Asset(string expr)
        {
            return _assets.Get(expr);
        }

        private T Ocr<T>(Func<Mat, Mat> src, string regex, TryParser<string[], T> tryParser, double waitSec = 1)
        {
            return X.While(() =>
            {
                using Mat scrn = Scrn();
                using Mat sub = src(scrn);
                string result = TxOcr.Ocr(sub);
                return Regex.Match(result, regex);
            }, match =>
            {
                T result = default;
                return (match.Success && tryParser(match.Groups.Values.Select(it => it.Value).ToArray(), out result),
                    result);
            }, TimeSpan.FromSeconds(waitSec))!;
        }

        private static Point Randomize(Point point)
        {
            return new Point(Math.Abs(Random.Next(-3, 3) + point.X),
                Math.Abs(Random.Next(-3, 3) + point.Y));
        }

        private Mat Scrn()
        {
            return X.While(_adb.GetScreenShot, result => !result.Empty())!;
        }
    }
}