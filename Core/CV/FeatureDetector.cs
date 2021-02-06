using System;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;
using REVUnit.AutoArknights.Core.Properties;

namespace REVUnit.AutoArknights.Core.CV
{
    public partial class FeatureDetector : IDisposable
    {
        private readonly Cache? _cache;
        private readonly Feature2D _fast = FastFeatureDetector.Create();
        private readonly Feature2D _freak = FREAK.Create();
        private readonly Feature2D _sift = SIFT.Create();

        public FeatureDetector(string? cacheDirPath = null)
        {
            if (cacheDirPath != null) _cache = new Cache(cacheDirPath);
        }

        public void Dispose()
        {
            _cache?.Dispose();
            _fast.Dispose();
            _freak.Dispose();
            _sift.Dispose();
        }

        public MatFeature DetectCached(Mat mat, Feature2DType type)
        {
            if (_cache == null) throw new InvalidOperationException(Resources.FeatureDetector_Exception_NoCachePath);

            MatFeature? result = _cache[mat];
            if (result != null) return result;

            result = Detect(mat, type);
            _cache[mat] = result;

            return result;
        }

        public MatFeature Detect(Mat mat, Feature2DType type)
        {
            var (detector, descriptor) = GetFeature2D(type);

            KeyPoint[] keyPoints = detector.Detect(mat);

            var descriptors = new Mat();
            descriptor.Compute(mat, ref keyPoints, descriptors);

            return new MatFeature(keyPoints, descriptors, mat.Width, mat.Height, type);
        }

        private (Feature2D detector, Feature2D descriptor) GetFeature2D(Feature2DType feature2DType)
        {
            return feature2DType switch
            {
                Feature2DType.FastFreak => (_fast, _freak),
                Feature2DType.Sift => (_sift, _sift),
                _ => throw new ArgumentOutOfRangeException(nameof(feature2DType), feature2DType, null)
            };
        }
    }
}