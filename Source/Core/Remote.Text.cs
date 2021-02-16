using System;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCvSharp;
using REVUnit.Crlib;

namespace REVUnit.AutoArknights.Core
{
    public partial class Remote
    {
        public class Text
        {
            private readonly Remote _remote;

            public Text(Remote remote) => _remote = remote;

            public bool TestAppear(string text)
            {
                return Ocr().Any(field => field.Contains(text));
            }

            public bool TestAppear(string text, RelativeArea area) => Ocr(area).Contains(text);

            public void WaitAppear(string text, double waitSec = 5)
            {
                while (!TestAppear(text)) Utils.Sleep(waitSec);
            }

            public void WaitAppear(string text, RelativeArea area, double waitSec = 5)
            {
                while (!TestAppear(text, area)) Utils.Sleep(waitSec);
            }

            public string[] Ocr()
            {
                using Mat scrn = _remote.GetScreenshot();
                return TencentOcr.OcrMulti(scrn);
            }

            public string Ocr(RelativeArea area)
            {
                using Mat scrn = _remote.GetScreenshot();
                using Mat sub = area.Reduce(scrn);
                return TencentOcr.Ocr(sub);
            }

            public T Ocr<T>(RelativeArea area, string regex, Func<string[], T> parser,
                            double failedRetryIntervalSecond = 3)
            {
                while (true)
                {
                    string result = Ocr(area);
                    Match match = Regex.Match(result, regex);
                    if (match.Success &&
                        new TryParser<string[], T>(parser).TryParse(
                            match.Groups.Values.Select(it => it.Value).ToArray(), out T? ret))
                        return ret;
                    Utils.Sleep(failedRetryIntervalSecond);
                }
            }
        }
    }
}