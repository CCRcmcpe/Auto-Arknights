using System;
using System.Threading.Tasks;
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

    internal class Interactor
    {
        private const double ConfidenceThreshold = 0.8;
        private static readonly Random Random = new();
        private readonly ImageAssets _assets;

        private readonly AsyncRetryPolicy<RegistrationResult> _definiteRegisterPolicy =
            Policy.HandleResult<RegistrationResult>(result => result.Confidence < ConfidenceThreshold).RetryAsync(3);

        private readonly IDevice _device;
        private readonly FeatureRegistration _featureRegistration = new("Cache");

        private readonly Size _resolution;

        private readonly TemplateRegistration _templateRegistration = new();

        private Mat? _screenshot;

        private Interactor(IDevice device, Size resolution)
        {
            _device = device;
            _resolution = resolution;
            _assets = new ImageAssets(_resolution);
        }

        public void Back()
        {
            _device.Back();
        }

        public Task Click(int x, int y)
        {
            return Click(new Point(x, y));
        }

        public Task Click(RelativeArea area)
        {
            return Click(area.For(_resolution));
        }

        public Task Click(Rect rect, bool randomize = true)
        {
            return Click(randomize ? GetCenter(rect) : PickRandomPoint(rect), !randomize);
        }

        public Task Click(Quadrilateral32 quadrilateral32, bool randomize = true)
        {
            return Click(randomize ? quadrilateral32.VertexCentroid.ToPoint() : PickRandomPoint(quadrilateral32),
                !randomize);
        }

        public Task Click(Point point, bool randomize = true)
        {
            return _device.Click(randomize ? RandomOffset(point) : point);
        }

        public async Task Click(string assetExpr, RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            await Click(await GetImageAsset(assetExpr), registrationType);
        }

        public async Task Click(Mat model, RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            PolicyResult<RegistrationResult> policyResult =
                await _definiteRegisterPolicy.ExecuteAndCaptureAsync(
                    async () => await LocateImage(model, registrationType));
            if (policyResult.FaultType == null)
            {
                await Click(policyResult.Result.Region);
            }
            else
            {
                throw new Exception(); // TODO
            }
        }

        public static async Task<Interactor> FromDevice(IDevice device)
        {
            Size resolution = await device.GetResolution();
            return new Interactor(device, resolution);
        }

        public async Task<RegistrationResult> LocateImage(string assetExpr, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            return await LocateImage(await GetImageAsset(assetExpr), registrationType);
        }

        public async Task<RegistrationResult> LocateImage(Mat model, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            return (await LocateImageMulti(model,
                1, registrationType))[0];
        }

        public async Task<RegistrationResult[]> LocateImageMulti(Mat model, int minMatchCount,
            RegistrationType registrationType =
                RegistrationType.TemplateMatching)
        {
            ImageRegistration imageRegistration = registrationType switch
            {
                RegistrationType.TemplateMatching => _templateRegistration,
                RegistrationType.FeatureMatching => _featureRegistration,
                _ => throw new ArgumentOutOfRangeException(nameof(registrationType), registrationType, null)
            };

            if (_screenshot == null)
            {
                await Update();
            }

            return imageRegistration.Register(model, _screenshot!, minMatchCount);
        }

        public async Task<string> Ocr(RelativeArea area)
        {
            return (await OcrMulti(area))[0];
        }

        public async Task<string[]> OcrMulti(RelativeArea area)
        {
            if (_screenshot == null)
            {
                await Update();
            }

            using Mat sub = area.Reduce(_screenshot!);
            throw new NotImplementedException();
        }

        public async Task<bool> TestAppear(string assetExpr, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            RegistrationResult result = await LocateImage(assetExpr, registrationType);
            return IsSuccessful(result);
        }

        public async Task Update()
        {
            _screenshot?.Dispose();
            _screenshot = await _device.GetScreenshot();
        }

        private static Point GetCenter(Rect rect)
        {
            return new(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        // public async Task<bool> TestTextAppear(string text)
        // {
        //     return (await OcrMulti(RelativeArea.All)).Any(field => field.Contains(text));
        // }
        //
        // public bool TestTextAppear(string text, RelativeArea area)
        // {
        //     return Ocr(area).Contains(text);
        // }
        //
        // public void WaitTextAppear(string text, double waitSec = 5)
        // {
        //     while (!TestTextAppear(text)) Utils.Sleep(waitSec);
        // }
        //
        // public void WaitTextAppear(string text, RelativeArea area, double waitSec = 5)
        // {
        //     while (!TestTextAppear(text, area)) Utils.Sleep(waitSec);
        // }

        private Task<Mat> GetImageAsset(string assetExpr)
        {
            return _assets.Get(assetExpr);
        }

        private static bool IsSuccessful(RegistrationResult result)
        {
            return result.Confidence > ConfidenceThreshold;
        }

        private static Point PickRandomPoint(Rect rect)
        {
            int randX = Random.Next((int) (rect.Width * 0.1), (int) (rect.Width * 0.9)) + rect.X;
            int randY = Random.Next((int) (rect.Height * 0.1), (int) (rect.Height * 0.9)) + rect.Y;
            return new Point(randX, randY);
        }

        private static Point PickRandomPoint(Quadrilateral32 quadrilateral32)
        {
            Quadrilateral32 downScaled = quadrilateral32.ScaleTo(0.8f);
            return downScaled.PickRandomPoint();
        }

        private static Point RandomOffset(Point point)
        {
            return new(Math.Abs(Random.Next(-5, 5) + point.X), Math.Abs(Random.Next(-5, 5) + point.Y));
        }
    }
}