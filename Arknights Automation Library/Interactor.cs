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
    public class Interactor : IDisposable
    {
        private static readonly Random Random = new Random();
        private readonly Adb _adb;
        private readonly Assets _assets = new Assets();

        private readonly FMLocator _loc = new FMLocator(Feature2DType.Sift, "Assets\\Cache");

        public Interactor(string adbPath)
        {
            _adb = new Adb(adbPath);
        }

        public Interactor(string adbPath, string adbRemote)
        {
            _adb = new Adb(adbPath);
            NewRemote(adbRemote);
            Connected = true;
        }

        public bool Connected { get; }

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
            return Ocr(GetCurrentSanityPart, @"(\d+)\s*\/\s*(\d+)", (string[] arr, out Sanity result) =>
            {
                int[] ints = arr.SelectCanParse<string, int>(int.TryParse).ToArray();
                bool valid = ints.Length == 2;
                result = valid ? new Sanity(ints[0], ints[1]) : default;
                return valid;
            }, TimeSpan.FromSeconds(1));
        }

        public int GetRequiredSanity()
        {
            return Ocr(GetRequiredSanityPart, @"\d+",
                (string[] arr, out int result) => int.TryParse(arr[0], out result), TimeSpan.FromSeconds(1));
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

        public void NewRemote(string adbRemote)
        {
            if (!_adb.Connect(adbRemote)) throw new Exception("未能连接到目标ADB");
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

        public void WaitAp(string expr, double durationSec = 3)
        {
            X.While(() => Loc(expr), result => result.Succeed, TimeSpan.FromSeconds(durationSec));
        }

        private Mat Asset(string expr)
        {
            return _assets.Get(expr);
        }

        private static Mat GetCurrentSanityPart(Mat super)
        {
            var sanityRect = new Rect((int) (super.Width * 0.88), (int) (super.Height * 0.02),
                (int) (super.Width * 0.1), (int) (super.Height * 0.08));
            return GetPart(super, sanityRect);
        }

        private static Mat GetPart(Mat super, Rect rect)
        {
            return super.Clone(rect);
        }

        private static Mat GetRequiredSanityPart(Mat super)
        {
            var sanityRect = new Rect((int) (super.Width * 0.927), (int) (super.Height * 0.941),
                (int) (super.Width * 0.035), (int) (super.Height * 0.035));
            return GetPart(super, sanityRect);
        }

        private T Ocr<T>(Func<Mat, Mat> src, string regex, TryParser<string[], T> tryParser, TimeSpan waitSpan)
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
            }, waitSpan);
        }

        private static Point Randomize(Point point)
        {
            return new Point(Math.Abs(Random.Next(-3, 3) + point.X),
                Math.Abs(Random.Next(-3, 3) + point.Y));
        }

        private Mat Scrn()
        {
            return X.While(_adb.GetScreenShot, result => !result.Empty());
        }
    }
}