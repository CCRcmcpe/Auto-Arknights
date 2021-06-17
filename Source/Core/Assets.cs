using System;
using System.IO;
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

    public class ImageAssets
    {
        private readonly AsyncCachePolicy<Mat> _getAssetCachePolicy =
            Policy.CacheAsync<Mat>(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                TimeSpan.MaxValue);

        private readonly bool _needNormalizeScale;
        private readonly double _resizeRatio;

        public ImageAssets(Size actualResolution)
        {
            _needNormalizeScale = actualResolution != TargetResolution;
            _resizeRatio = (double) actualResolution.Height / TargetResolution.Height;
        }

        public static Size TargetResolution { get; } = new(1920, 1080);

        public Task<Mat> Get(string key)
        {
            return _getAssetCachePolicy.ExecuteAsync(async context =>
            {
                string assetFilePath = GetFilePath(context.OperationKey);
                if (!File.Exists(assetFilePath)) throw new AssetLoadException(key);
                Mat asset = await Utils.Imread(assetFilePath);
                if (asset.Empty()) throw new AssetLoadException(key);

                if (_needNormalizeScale)
                {
                    NormalizeScale(asset);
                }

                return asset;
            }, new Context(key));
        }

        private static string GetFilePath(string key)
        {
            return Path.Combine("Assets", key) + ".png";
        }

        private void NormalizeScale(Mat model)
        {
            Size size = model.Size();
            var normalizedModelSize = new Size(size.Width * _resizeRatio, size.Height * _resizeRatio);

            Resize(model, normalizedModelSize);
        }

        private static void Resize(Mat mat, Size targetSize)
        {
            Size size = mat.Size();
            bool upscale = size.Width < targetSize.Width || size.Height < targetSize.Height;
            Cv2.Resize(mat, mat, targetSize, interpolation: upscale
                ? InterpolationFlags.Cubic
                : InterpolationFlags.Area);
        }
    }
}