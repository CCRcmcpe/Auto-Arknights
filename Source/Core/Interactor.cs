using System;
using System.Linq;
using OpenCvSharp;
using Polly;
using Polly.Retry;
using REVUnit.AutoArknights.Core.CV;

namespace REVUnit.AutoArknights.Core
{
    internal class Interactor : IDisposable
    {
        private const double ConfidenceThreshold = 0.8;
        private static readonly Random Random = new();
        private readonly ImageAssets _assets;

        private readonly IDevice _device;
        private readonly TemplateRegister _register = new();

        private readonly RetryPolicy<RegisterResult> _registerPolicy =
            Policy.HandleResult<RegisterResult>(result => result.Confidence < ConfidenceThreshold).Retry(3);

        private readonly Size _resolution;

        public Interactor(IDevice device)
        {
            _device = device;
            _resolution = device.GetResolution();
            _assets = new ImageAssets(_resolution);
        }

        public void Dispose()
        {
            _assets.Dispose();
        }

        private Mat Asset(string assetExpr)
        {
            return _assets.Get(assetExpr);
        }

        private static Point Randomize(Point point)
        {
            return new(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));
        }

        private static Point PickRandomPoint(Rect rect)
        {
            int randX = Random.Next((int) (rect.Width * 0.1), (int) (rect.Width * 0.9)) + rect.X;
            int randY = Random.Next((int) (rect.Height * 0.1), (int) (rect.Height * 0.9)) + rect.Y;
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
            Click(PickRandomPoint(rect));
        }

        public void Click(Point point)
        {
            _device.Click(Randomize(point));
        }

        public void Click(string assetExpr)
        {
            Click(Asset(assetExpr));
        }

        public void Click(Mat model)
        {
            PolicyResult<RegisterResult> policyResult = _registerPolicy.ExecuteAndCapture(() => Locate(model));
            if (policyResult.FaultType == null)
            {
                Click(policyResult.Result.CircumRect);
            }
            else
            {
                throw new Exception(); // TODO
            }
        }

        public bool TestTextAppear(string text)
        {
            return OcrMulti(RelativeArea.All).Any(field => field.Contains(text));
        }

        public bool TestTextAppear(string text, RelativeArea area)
        {
            return Ocr(area).Contains(text);
        }

        public void WaitTextAppear(string text, double waitSec = 5)
        {
            while (!TestAppear(text)) Utils.Sleep(waitSec);
        }

        public void WaitTextAppear(string text, RelativeArea area, double waitSec = 5)
        {
            while (!TestTextAppear(text, area)) Utils.Sleep(waitSec);
        }

        public string Ocr(RelativeArea area)
        {
            return OcrMulti(area)[0];
        }

        public string[] OcrMulti(RelativeArea area)
        {
            using Mat scrn = _device.GetScreenshot();
            using Mat sub = area.Reduce(scrn);
            throw new NotImplementedException();
        }

        public RegisterResult Locate(string assetExpr)
        {
            Mat model = Asset(assetExpr);
            return Locate(model);
        }

        public RegisterResult Locate(Mat model)
        {
            using Mat scrn = _device.GetScreenshot();
            RegisterResult result = _register.Register(model, scrn, 1)[0];
            return result;
        }

        public RegisterResult[] LocateMulti(Mat model, int minMatchCount = 1)
        {
            using Mat scrn = _device.GetScreenshot();
            RegisterResult[] result = _register.Register(model, scrn, minMatchCount);
            return result;
        }

        private static bool IsSuccessful(RegisterResult result)
        {
            return result.Confidence > ConfidenceThreshold;
        }

        public bool TestAppear(string assetExpr)
        {
            return IsSuccessful(Locate(assetExpr));
        }
    }
}