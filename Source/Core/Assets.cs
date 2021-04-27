using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using REVUnit.AutoArknights.Core.Properties;

namespace REVUnit.AutoArknights.Core
{
    public class AssetLoadException : Exception
    {
        public AssetLoadException(string key) : base(string.Format(Resources.AssetsLoadException, key))
        {
        }
    }

    public class ImageAssets : IDisposable
    {
        private readonly Dictionary<string, Mat> _cache = new();
        private readonly bool _needNormalizeScale;
        private readonly double _resizeRatio;

        public ImageAssets(Size actualResolution)
        {
            _needNormalizeScale = actualResolution != TargetedResolution;
            _resizeRatio = (double) actualResolution.Height / TargetedResolution.Height;
        }

        public static Size TargetedResolution { get; } = new(1920, 1080);

        public void Dispose()
        {
            foreach (Mat asset in _cache.Values) asset.Dispose();
        }

        public Mat Get(string key)
        {
            key = key.Trim();
            if (_cache.TryGetValue(key, out Mat? asset)) return asset;

            string assetFilePath = GetFilePath(key);
            if (!File.Exists(assetFilePath)) throw new AssetLoadException(key);

            asset = Utils.Imread(assetFilePath);
            if (asset.Empty()) throw new AssetLoadException(key);

            if (_needNormalizeScale)
            {
                NormalizeScale(asset);
            }

            _cache.Add(key, asset);
            return asset;
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

        private static string GetFilePath(string key)
        {
            return Path.Combine("Assets", key) + ".png";
        }
    }
}