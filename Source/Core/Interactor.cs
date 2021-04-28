using System;
using System.Linq;
using OpenCvSharp;
using Polly;
using Polly.Retry;
using REVUnit.AutoArknights.Core.CV;

namespace REVUnit.AutoArknights.Core
{
    public enum RegistrationType
    {
        TemplateMatching,
        FeatureMatching
    }

    internal class Interactor : IDisposable
    {
        private const double ConfidenceThreshold = 0.8;
        private static readonly Random Random = new();
        private readonly ImageAssets _assets;

        private readonly IDevice _device;
        private readonly FeatureRegistration _featureRegistration = new("Cache");

        private readonly RetryPolicy<RegistrationResult> _registerPolicy =
            Policy.HandleResult<RegistrationResult>(result => result.Confidence < ConfidenceThreshold).Retry(3);

        private readonly Size _resolution;

        private readonly TemplateRegistration _templateRegistration = new();

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

        private Mat GetImageAsset(string assetExpr)
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

        public void Back()
        {
            _device.Back();
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

        public void Click(string assetExpr, RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            Click(GetImageAsset(assetExpr), registrationType);
        }

        public void Click(Mat model, RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            PolicyResult<RegistrationResult> policyResult = _registerPolicy.ExecuteAndCapture(() => LocateImage(model,
                registrationType));
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
            while (!TestTextAppear(text)) Utils.Sleep(waitSec);
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

        public RegistrationResult LocateImage(string assetExpr, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            return LocateImage(
                GetImageAsset(assetExpr),
                registrationType);
        }

        public RegistrationResult LocateImage(Mat model, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            return LocateImageMulti(model,
                1, registrationType)[0];
        }

        public RegistrationResult[] LocateImageMulti(Mat model, int minMatchCount, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            ImageRegistration imageRegistration = registrationType switch
            {
                RegistrationType.TemplateMatching => _templateRegistration,
                RegistrationType.FeatureMatching => _featureRegistration,
                _ => throw new ArgumentOutOfRangeException(nameof(registrationType), registrationType, null)
            };
            using Mat scrn = _device.GetScreenshot();
            RegistrationResult[] result = imageRegistration.Register(model, scrn, minMatchCount);
            return result;
        }

        private static bool IsSuccessful(RegistrationResult result)
        {
            return result.Confidence > ConfidenceThreshold;
        }

        public bool TestAppear(string assetExpr, RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            return IsSuccessful(LocateImage(assetExpr, registrationType));
        }

        public bool TestAppear(string assetExpr, out RegistrationResult result, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            result = LocateImage(assetExpr, registrationType);
            return IsSuccessful(result);
        }
    }
}