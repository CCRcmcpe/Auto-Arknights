using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OpenCvSharp;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Retry;
using REVUnit.AutoArknights.Core.CV;
using Serilog;

namespace REVUnit.AutoArknights.Core
{
    public enum RegistrationType
    {
        TemplateMatching,
        FeatureMatching
    }

    public class ClickForException : Exception
    {
        public ClickForException(string message) : base(message)
        {
        }

        public ClickForException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    internal class Interactor
    {
        private static readonly int[] ClickForPolicyRetryInterval = {100, 200, 500, 1000, 1200};

        private static readonly AsyncRetryPolicy ClickForPolicy = Policy.Handle<ClickForException>().WaitAndRetryAsync(
            5,
            retryCount => TimeSpan.FromMilliseconds(ClickForPolicyRetryInterval[retryCount - 1]),
            (_, timeSpan, retryCount, context) => Log.Debug("尝试点击 {ImageKey} 第{RetryCount}次失败，等待{SleepDuration}ms后重试",
                context.OperationKey, retryCount, timeSpan.TotalMilliseconds));


        private const double ConfidenceThreshold = 0.8;

        private static readonly FeatureRegistration FeatureRegistration = new("Cache");

        private static readonly AsyncCachePolicy<RegistrationResult[]> LocateImageCachePolicy =
            Policy.CacheAsync<RegistrationResult[]>(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                TimeSpan.FromMilliseconds(100));

        private static readonly Random Random = new();

        private static readonly AsyncCachePolicy<Mat> ScreenshotCachePolicy =
            Policy.CacheAsync<Mat>(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                TimeSpan.FromMilliseconds(100));

        private static readonly TemplateRegistration TemplateRegistration = new();

        private readonly IDevice _device;
        private readonly Size _deviceResolution;

        private Interactor(IDevice device, Size deviceResolution)
        {
            _device = device;
            _deviceResolution = deviceResolution;
        }

        public void Back()
        {
            _device.Back();
        }

        public Task Click(RelativeArea area)
        {
            return Click(area.For(_deviceResolution));
        }

        public Task Click(Rect rect, bool randomize = true)
        {
            return Click(randomize ? PickRandomPoint(rect) : GetCenter(rect), !randomize);
        }

        public Task Click(Quadrilateral32 quadrilateral32, bool randomize = true)
        {
            return Click(randomize ? PickRandomPoint(quadrilateral32) : quadrilateral32.VertexCentroid.ToPoint(),
                !randomize);
        }

        public Task Click(Point point, bool randomize = true)
        {
            return _device.Click(randomize ? RandomOffset(point) : point);
        }

        public async Task ClickFor(string assetExpr,
            RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            await ClickFor(await GetImageAsset(assetExpr), registrationType);
        }

        public static async Task<Interactor> FromDevice(IDevice device)
        {
            Size resolution = await device.GetResolution();
            return new Interactor(device, resolution);
        }

        public async Task<string> Ocr(RelativeArea area, string? patternName = null)
        {
            Mat screenshot = await GetScreenshot();
            return await Task.Run(() =>
            {
                using Mat sub = area.Of(screenshot);
                TextBlock textBlock = CV.Ocr.Single(sub, patternName);
                if (textBlock.Confidence < ConfidenceThreshold)
                {
                    Log.Debug("OCR 结果可能有误（置信度：{Confidence}）", textBlock.Confidence);
                }

                return textBlock.Text;
            });
        }

        public async Task<bool> TestAppear(string assetExpr, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            ImageAsset asset = await ImageAsset.Get(assetExpr, _deviceResolution);
            RegistrationResult result = await LocateImage(asset, registrationType);

            return IsSuccessful(result);
        }

        private async Task ClickFor(ImageAsset asset,
            RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            PolicyResult<RegistrationResult> policyResult =
                await ClickForPolicy.ExecuteAndCaptureAsync(async context =>
                {
                    RegistrationResult result = await LocateImage(asset, registrationType);
                    if (!IsSuccessful(result))
                    {
                        throw new ClickForException($"无法定位 {context.OperationKey}");
                    }

                    return result;
                }, new Context(asset.Key));
            if (policyResult.Outcome == OutcomeType.Successful)
            {
                await Click(policyResult.Result.Region);
            }
            else
            {
                throw new ClickForException($"尝试点击 {asset.Key} 最终失败", policyResult.FinalException);
            }
        }

        private static Point GetCenter(Rect rect)
        {
            return new(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        private Task<ImageAsset> GetImageAsset(string assetExpr)
        {
            return ImageAsset.Get(assetExpr, _deviceResolution);
        }

        private Task<Mat> GetScreenshot()
        {
            return ScreenshotCachePolicy.ExecuteAsync(_ => _device.GetScreenshot(), new Context(string.Empty));
        }

        private static bool IsSuccessful(RegistrationResult result)
        {
            return result.Confidence > ConfidenceThreshold;
        }

        private async Task<RegistrationResult> LocateImage(ImageAsset image, RegistrationType registrationType =
            RegistrationType.TemplateMatching)
        {
            return (await LocateImage(image, 1, registrationType))[0];
        }

        private Task<RegistrationResult[]> LocateImage(ImageAsset image, int minMatchCount = 1,
            RegistrationType registrationType = RegistrationType.TemplateMatching)
        {
            ImageRegistration imageRegistration = registrationType switch
            {
                RegistrationType.TemplateMatching => TemplateRegistration,
                RegistrationType.FeatureMatching => FeatureRegistration,
                _ => throw new ArgumentOutOfRangeException(nameof(registrationType), registrationType, null)
            };
            return LocateImageCachePolicy.ExecuteAsync(
                async _ => imageRegistration.Register(image.Image, await GetScreenshot(), minMatchCount),
                new Context(image.Key));
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