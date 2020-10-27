using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class CacheLoadException : Exception
    {
        public CacheLoadException(string cacheFilePath, string node) :
            base($"Error loading node {node} in cache file {cacheFilePath}") { }
    }

    public partial class FeatureDetector : IDisposable
    {
        private readonly Cache? _cache;
        private readonly Feature2D _f2d;

        public FeatureDetector(Feature2D f2d, string? cacheDirPath = null)
        {
            _f2d = f2d;
            if (cacheDirPath != null) _cache = new Cache(cacheDirPath);
        }

        public void Dispose()
        {
            _f2d.Dispose();
        }

        public MatFeature Detect(Mat mat, bool cache = false)
        {
            if (_cache == null || !cache) return DetectCore(mat);

            MatFeature? result = _cache[mat];
            if (result != null) return result;

            result = DetectCore(mat);
            _cache[mat] = result;

            return result;
        }

        private MatFeature DetectCore(Mat mat)
        {
            var des = new Mat();
            _f2d.DetectAndCompute(mat, null, out KeyPoint[] kps, des);
            return new MatFeature(kps, des, mat.Width, mat.Height);
        }
    }
}