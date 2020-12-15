using System;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.XFeatures2D;

namespace REVUnit.AutoArknights.Core.CV
{
    public partial class FeatureDetector : IDisposable
    {
        public enum FeatureType
        {
            USurf,
            Surf,
            Sift
        }

        private readonly Cache? _cache;
        private readonly SIFT _sift = SIFT.Create();
        private readonly SURF _surf = SURF.Create(400, 2);

        public FeatureDetector(string? cacheDirPath = null)
        {
            if (cacheDirPath != null) _cache = new Cache(cacheDirPath);
        }

        public void Dispose()
        {
            _sift.Dispose();
            _surf.Dispose();
        }

        public MatFeature DetectCached(Mat mat, DeformationLevel deformationLevel)
        {
            if (_cache == null)
                throw new InvalidOperationException("Cannot use cached detect when cache directory is not set");

            MatFeature? result = _cache[mat];
            if (result != null) return result;

            result = Detect(mat, deformationLevel);
            _cache[mat] = result;

            return result;
        }

        public MatFeature Detect(Mat mat, DeformationLevel deformationLevel)
        {
            Feature2D feature2D;
            switch (deformationLevel)
            {
                case DeformationLevel.Fast:
                    feature2D = _surf;
                    _surf.Upright = true;
                    break;
                case DeformationLevel.Medium:
                    feature2D = _surf;
                    _surf.Upright = false;
                    break;
                case DeformationLevel.Slow:
                    feature2D = _sift;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(deformationLevel), deformationLevel, null);
            }

            var des = new Mat();
            feature2D.DetectAndCompute(mat, null, out KeyPoint[] kps, des);
            return new MatFeature(kps, des, mat.Width, mat.Height, deformationLevel);
        }
    }
}