using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class FeatureRegister : ImageRegister, IDisposable
    {
        private readonly bool _useCache;

        public FeatureRegister(string? cacheDirPath = null)
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

        protected override RegisterResult[] RegisterInternal(Mat model, Mat observed, int minMatchCount)
        {
            return new[] { Register(model, observed, Feature2DType.FastFreak) };
        }

        public RegisterResult Register(Mat model, Mat observed, Feature2DType type)
        {
            MatFeature? observedFeature = null;
            MatFeature? modelFeature = null;
            try
            {
                modelFeature = FeatureDetector.DetectCached(model, type);
                observedFeature = FeatureDetector.Detect(observed, type);
                (double confidence, Rect circumRect) = FeatureMatcher.Match(modelFeature, observedFeature);
                return confidence > SuccessThreshold
                    ? RegisterResult.Succeed(circumRect, confidence)
                    : RegisterResult.Failed();
            }
            finally
            {
                if (!_useCache) modelFeature?.Dispose();
                observedFeature?.Dispose();
            }
        }
    }
}