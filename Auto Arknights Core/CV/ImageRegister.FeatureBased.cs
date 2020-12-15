using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class FeatureBasedImageRegister : ImageRegister, IDisposable
    {
        private readonly bool _useCache;

        public FeatureBasedImageRegister(string? cacheDirPath = null)
        {
            _useCache = cacheDirPath != null;
            FeatureDetector = new FeatureDetector(cacheDirPath);
            FeatureMatcher = new FeatureMatcher();
        }

        public FeatureDetector FeatureDetector { get; }
        public FeatureMatcher FeatureMatcher { get; }
        public float SuccessThreshold { get; set; } = 1f / 3f;


        public void Dispose()
        {
            FeatureDetector.Dispose();
            FeatureMatcher.Dispose();
        }

        public override RegisterResult Register(Mat model, Mat observed)
        {
            MatFeature? observedFeature = null;
            MatFeature? modelFeature = null;
            try
            {
                modelFeature = FeatureDetector.DetectCached(model, deformationLevel);
                observedFeature = FeatureDetector.Detect(observed, deformationLevel);
                (double confidence, Rect circumRect) = FeatureMatcher.Match(modelFeature, observedFeature);
                return confidence > SuccessThreshold ? RegisterResult.Succeed(circumRect) : RegisterResult.Failed();
            }
            finally
            {
                if (!_useCache) modelFeature?.Dispose();
                observedFeature?.Dispose();
            }
        }
    }
}