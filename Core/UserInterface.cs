using System;
using System.Linq;
using OpenCvSharp;
using REVUnit.Crlib.Extension;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public partial class UserInterface
    {
        private static readonly Random Random = new();

        private static readonly Lazy<UserInterface> LazyInitializer = new(() => new UserInterface(
                                                                              Library.Settings.Remote.AdbExecutable,
                                                                              Library.Settings.Remote.Serial));

        private readonly Adb _adb;

        private readonly Size _resolution;

        private UserInterface(string exePath, string targetSerial)
        {
            _adb = new Adb(exePath, targetSerial);
            _resolution = _adb.GetResolution();
            Graphical = new GraphicalInterface(this);
            Textual = new TextualInterface(this);
        }

        public GraphicalInterface Graphical { get; }
        public TextualInterface Textual { get; }

        public static UserInterface I => LazyInitializer.Value;

        private static Point Randomize(Point point) =>
            new(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));

        private static Point Randomize(Rect rect)
        {
            int randX = Random.Next(0, rect.Width) + rect.X;
            int randY = Random.Next(0, rect.Height) + rect.Y;
            return new Point(randX, randY);
        }

        public void Click(int x, int y)
        {
            Click(new Point(x, y));
        }

        public void Click(RelativeArea area)
        {
            Click(area.For(_resolution));
        }

        public void Click(Rect rect)
        {
            Click(Randomize(rect));
        }

        public void Click(Point point)
        {
            _adb.Click(Randomize(point));
        }

        public Sanity GetCurrentSanity()
        {
            return Textual.Ocr(RelativeArea.CurrentSanity, @"(\d+)\s*\/\s*(\d+)", matches =>
            {
                int[] numbers = matches.SelectCanParse<string, int>(int.TryParse).ToArray();
                if (numbers.Length != 2) throw new Exception();

                return new Sanity(numbers[0], numbers[1]);
            })!;
        }

        public int GetRequiredSanity()
        {
            return Textual.Ocr(RelativeArea.RequiredSanity, @"\d+", matches => int.Parse(matches[0]));
        }

        public Mat GetScreenshot() => _adb.GetScreenshot();
    }
}