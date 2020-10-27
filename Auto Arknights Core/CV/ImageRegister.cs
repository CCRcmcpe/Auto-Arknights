using System;
using System.Drawing;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Features2D;

namespace REVUnit.AutoArknights.Core.CV
{
    public class ImageRegister : IDisposable
    {
        private readonly Feature2D _f2d;
        private readonly bool _useCache;

        public ImageRegister(string? cacheDirPath = null)
        {
            _f2d = SIFT.Create();

            FeatureDetector = new FeatureDetector(_f2d, cacheDirPath);
            _useCache = cacheDirPath != null;

            FeatureMatcher = new FeatureMatcher(_f2d);

            if (cacheDirPath != null && !Directory.Exists(cacheDirPath)) Directory.CreateDirectory(cacheDirPath);
        }

        public FeatureDetector FeatureDetector { get; }
        public FeatureMatcher FeatureMatcher { get; }

        public void Dispose()
        {
            _f2d.Dispose();
            FeatureDetector.Dispose();
            FeatureMatcher.Dispose();
        }

        public LocateResult Locate(Mat model, Mat observed)
        {
            MatFeature? observedFeature = null;
            MatFeature? modelFeature = null;
            try
            {
                modelFeature = FeatureDetector.Detect(model, true);
                observedFeature = FeatureDetector.Detect(observed);
                bool success = FeatureMatcher.Match(modelFeature, observedFeature, out Rectangle rect);
                return success ? LocateResult.Succeed(rect) : LocateResult.Failed();
            }
            finally
            {
                if (!_useCache) modelFeature?.Dispose();
                observedFeature?.Dispose();
            }
        }
    }
}