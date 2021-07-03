using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OpenCvSharp;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using REVUnit.AutoArknights.Core.Properties;

namespace REVUnit.AutoArknights.Core
{
    public class AssetLoadException : Exception
    {
        public AssetLoadException(string key) : base(string.Format(Resources.AssetsLoadException, key))
        {
        }
    }

    public class ImageAsset
    {
        private static readonly AsyncCachePolicy<ImageAsset> GetAssetCachePolicy =
            Policy.CacheAsync<ImageAsset>(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                TimeSpan.MaxValue /* Currently, even if all those images are cached, it won't take a huge amount of memory. */);

        static ImageAsset()
        {
            using FileStream fileStream = File.Open(@"Assets\info.json", FileMode.Open);
            using JsonDocument json = JsonDocument.Parse(fileStream);
            JsonElement element = json.RootElement.GetProperty("TargetResolution");
            int width = element.GetProperty("Width").GetInt32();
            int height = element.GetProperty("Height").GetInt32();
            TargetResolution = new Size(width, height);
        }

        public ImageAsset(string key, Mat image)
        {
            Key = key;
            Image = image;
        }

        public Mat Image { get; }

        public string Key { get; }

        public static Size TargetResolution { get; }

        public static Task<ImageAsset> Get(string assetExpr, Size actualResolution)
        {
            return GetAssetCachePolicy.ExecuteAsync(async context =>
            {
                string assetFilePath = GetFilePath(context.OperationKey);
                if (!File.Exists(assetFilePath)) throw new AssetLoadException(assetExpr);

                Mat image = await Utils.Imread(assetFilePath);
                if (image.Empty()) throw new AssetLoadException(assetExpr);
                AdaptResolution(image, actualResolution);

                return new ImageAsset(assetExpr, image);
            }, new Context(assetExpr));
        }

        private static void AdaptResolution(Mat image, Size actualResolution)
        {
            if (actualResolution == TargetResolution) return;
            Size size = image.Size();
            double ratio = (double) actualResolution.Height / TargetResolution.Height;
            var normalizedSize = new Size(size.Width * ratio, size.Height * ratio);
            bool upscale = size.Height < normalizedSize.Height;
            Cv2.Resize(image, image, normalizedSize, interpolation: upscale
                ? InterpolationFlags.Cubic
                : InterpolationFlags.Area);
        }

        private static string GetFilePath(string assetExpr)
        {
            return Path.Combine("Assets", assetExpr) + ".png";
        }
    }
}