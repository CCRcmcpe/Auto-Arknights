using System;
using System.Text.RegularExpressions;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public partial class Remote : IDisposable
    {
        private static readonly Random Random = new();

        private static readonly Lazy<Remote> LazyInitializer =
            new(() => new Remote(Library.Settings.Remote.AdbExecutable, Library.Settings.Remote.Serial));

        private static readonly Regex CurrentSanityRegex =
            new(@"(?<current>\d+)\s*\/\s*(?<max>\d+)", RegexOptions.Compiled);

        private readonly Adb _adb;
        private readonly string _targetSerial;
        private Size _resolution;

        private Remote(string adbPath, string targetSerial)
        {
            _targetSerial = targetSerial;
            _adb = new Adb(adbPath, targetSerial);
        }

        public Graphic? Graphical { get; private set; }
        public Text? Textual { get; private set; }

        public static Remote I => LazyInitializer.Value;

        public void Dispose()
        {
            Graphical?.Dispose();
        }

        public void Initialize()
        {
            _adb.StartServer();
            _adb.Connect(_targetSerial);
            _resolution = _adb.GetResolution();
            Graphical = new Graphic(this);
            Textual = new Text(this);
        }

        private static Point Randomize(Point point)
        {
            return new(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));
        }

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
            string text = Textual.Ocr(RelativeArea.CurrentSanity);
            Match match = CurrentSanityRegex.Match(text);
            if (!(int.TryParse(match.Groups["current"].Value, out int current) &&
                  int.TryParse(match.Groups["max"].Value, out int max)))
            {
                throw new Exception();
            }

            return new Sanity(current, max);
        }

        public int GetRequiredSanity()
        {
            string text = Textual.Ocr(RelativeArea.RequiredSanity);
            if (!int.TryParse(text[1..], out int requiredSanity)) throw new Exception();

            return requiredSanity;
        }

        public Mat GetScreenshot()
        {
            return _adb.GetScreenshot();
        }
    }
}