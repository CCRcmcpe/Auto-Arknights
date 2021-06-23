using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class FeatureRegistration : ImageRegistration, IDisposable
    {
        private readonly bool _useCache;

        public FeatureRegistration(string? cacheDirPath = null)
        {
            _useCache = cacheDirPath != null;
            FeatureDetector = new FeatureDetector(cacheDirPath);
            FeatureMatcher = new FeatureMatcher();
        }

        public FeatureDetector FeatureDetector { get; }
        public FeatureMatcher FeatureMatcher { get; }


        public void Dispose()
        {
            FeatureDetector.Dispose();
            FeatureMatcher.Dispose();
        }

        public override RegistrationResult[] Register(Mat model, Mat scene, int minMatchCount)
        {
            // TODO implement minMatchCount
            return new[] {Register(model, scene, Feature2DType.FastFreak)};
        }

        public RegistrationResult Register(Mat model, Mat scene, Feature2DType type)
        {
            MatFeature? sceneFeature = null;
            MatFeature? modelFeature = null;
            try
            {
                modelFeature = _useCache
                    ? FeatureDetector.DetectCached(model, type)
                    : FeatureDetector.Detect(model, type);
                sceneFeature = FeatureDetector.Detect(scene, type);
                (double matchCount, Quadrilateral region) = FeatureMatcher.Match(modelFeature, sceneFeature);
                return new RegistrationResult(region, matchCount > 4 ? 1 : 0);
            }
            finally
            {
                if (!_useCache) modelFeature?.Dispose();
                sceneFeature?.Dispose();
            }
        }
    }
}