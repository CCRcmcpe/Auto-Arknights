using System;
using System.Linq;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public partial class Remote : IDisposable
    {
        private static readonly Random Random = new();

        private static readonly Lazy<Remote> LazyInitializer =
            new(() => new Remote(Library.Settings.Remote.AdbExecutable, Library.Settings.Remote.Serial));

        private readonly Adb _adb;
        private readonly Size _resolution;

        private Remote(string adbPath, string serial)
        {
            _adb = new Adb(adbPath, serial);
            _resolution = _adb.GetResolution();
            Graphical = new Graphic(this);
            Textual = new Text(this);
        }

        public Graphic Graphical { get; }
        public Text Textual { get; }

        public static Remote I => LazyInitializer.Value;

        public void Dispose()
        {
            Graphical.Dispose();
        }

        private static Point Randomize(Point point) =>
            new(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));

        private static Point Randomize(Rect rect)
        {
            int randX = Random.Next(0, rect.Width) + rect.X;
            int randY = Random.Next(0, rect.Height) + rect.Y;
            return new Point(randX, randY);
        }

        public void Click(int x, int y) => Click(new Point(x, y));

        public void Click(RelativeArea area) => Click(area.For(_resolution));

        public void Click(Rect rect) => Click(Randomize(rect));

        public void Click(Point point) => _adb.Click(Randomize(point));

        public Sanity GetCurrentSanity()
        {
            return Textual.Ocr(RelativeArea.CurrentSanity, @"(\d+)\s*\/\s*(\d+)", matches =>
            {
                int[] nums = matches.Select(s => (success: int.TryParse(s, out int i), i))
                                    .Where(result => result.success).Select(result => result.i)
                                    .ToArray();

                return new Sanity(nums[0], nums[1]);
            });
        }

        public int GetRequiredSanity() =>
            Textual.Ocr(RelativeArea.RequiredSanity, @"\d+", matches => int.Parse(matches[0]));

        public Mat GetScreenshot() => _adb.GetScreenshot();
    }
}