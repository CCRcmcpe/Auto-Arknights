using System.Linq;
using OpenCvSharp;

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
                return OcrMulti(RelativeArea.All).Any(field => field.Contains(text));
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

            public string[] OcrMulti(RelativeArea area)
            {
                using Mat scrn = _remote.GetScreenshot();
                using Mat sub = area.Reduce(scrn);
                return TencentOcr.OcrMulti(sub);
            }

            public string Ocr(RelativeArea area) => OcrMulti(area)[0];
        }
    }
}