﻿using System;
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
    public sealed class Device : IDisposable
    {
        private static readonly Random Random = new Random();
        private readonly Adb _adb;
        private readonly Assets _assets = new Assets();

        private readonly FMLocator _loc = new FMLocator(Feature2DType.Sift, "Assets\\Cache");

        public Device(string adbPath, string adbRemote)
        {
            _adb = new Adb(adbPath);
            if (!_adb.Connect(adbRemote)) throw new AdbException("无法连接到目标设备");
        }

        public void Dispose()
        {
            _assets.Dispose();
            _loc.Dispose();
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
            return Ocr(ScreenArea.CurrentSanity, @"(\d+)\s*\/\s*(\d+)",
                matches =>
                {
                    int[] numbers = matches.SelectCanParse<string, int>(int.TryParse).ToArray();
                    if (numbers.Length != 2) throw new FormatException();
                    return new Sanity(numbers[0], numbers[1]);
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
            return _loc.Locate(model, scrn);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Slp(double sec)
        {
            Thread.Sleep(TimeSpan.FromSeconds(sec));
        }

        public bool TestAp(string expr) => Loc(expr).Succeed;

        public void WaitAp(string expr, double waitSec = 3)
        {
            X.While(() => Loc(expr), result => result.Succeed, TimeSpan.FromSeconds(waitSec));
        }

        private Mat Asset(string expr) => _assets.Get(expr);

        private T Ocr<T>(Rect2f area, string regex, Func<string[], T> parser, double waitSec = 1)
        {
            T ret = default;
            X.While(() =>
            {
                using Mat scrn = Scrn();
                using Mat sub = area.Apply(scrn);
                string result = TxOcr.Ocr(sub);
                return Regex.Match(result, regex);
            }, match =>
            {
                return match.Success &&
                       new TryParser<string[], T>(parser).TryParse(match.Groups.Values.Select(it => it.Value).ToArray(),
                           out ret);
            }, TimeSpan.FromSeconds(waitSec));
            return ret!;
        }

        private static Point Randomize(Point point) =>
            new Point(Math.Abs(Random.Next(-3, 3) + point.X),
                Math.Abs(Random.Next(-3, 3) + point.Y));

        private Mat Scrn()
        {
            return X.While(_adb.GetScreenShot, result => !result.Empty())!;
        }
    }
}