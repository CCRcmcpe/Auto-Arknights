using System;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;

namespace REVUnit.AutoArknights.Core.CV
{
    public partial class FeatureDetector : IDisposable
    {
        private readonly Cache? _cache;
        private readonly Lazy<Feature2D> _fast = new(() => FastFeatureDetector.Create());
        private readonly Lazy<Feature2D> _freak = new(() => FREAK.Create());
        private readonly Lazy<Feature2D> _sift = new(() => SIFT.Create());

        public FeatureDetector(string? cacheDirPath = null)
        {
            if (cacheDirPath != null) _cache = new Cache(cacheDirPath);
        }

        public void Dispose()
        {
            _cache?.Dispose();
            DisposeIfCreated(_fast);
            DisposeIfCreated(_freak);
            DisposeIfCreated(_sift);
        }

        public MatFeature Detect(Mat mat, Feature2DType type)
        {
            var (detector, descriptor) = GetFeature2D(type);

            KeyPoint[] keyPoints = detector.Detect(mat);

            var descriptors = new Mat();
            descriptor.Compute(mat, ref keyPoints, descriptors);

            return new MatFeature(keyPoints, descriptors, mat.Width, mat.Height, type);
        }

        public MatFeature DetectCached(Mat mat, Feature2DType type)
        {
            if (_cache == null) throw new InvalidOperationException("必须提供缓存保存文件夹才能使用缓存");

            MatFeature? result = _cache[mat];
            if (result != null) return result;

            result = Detect(mat, type);
            _cache[mat] = result;

            return result;
        }

        private static void DisposeIfCreated(Lazy<Feature2D> lazyFeature2D)
        {
            if (lazyFeature2D.IsValueCreated)
            {
                lazyFeature2D.Value.Dispose();
            }
        }

        private (Feature2D detector, Feature2D descriptor) GetFeature2D(Feature2DType feature2DType)
        {
            return feature2DType switch
            {
                Feature2DType.FastFreak => (_fast.Value, _freak.Value),
                Feature2DType.Sift => (_sift.Value, _sift.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(feature2DType), feature2DType, null)
            };
        }
    }
}