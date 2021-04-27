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


        public void Dispose()
        {
            FeatureDetector.Dispose();
            FeatureMatcher.Dispose();
        }

        public override RegisterResult[] Register(Mat model, Mat scene, int minMatchCount)
        {
            // TODO implement minMatchCount
            return new[] {Register(model, scene, Feature2DType.FastFreak)};
        }

        public RegisterResult Register(Mat model, Mat scene, Feature2DType type)
        {
            MatFeature? sceneFeature = null;
            MatFeature? modelFeature = null;
            try
            {
                modelFeature = FeatureDetector.DetectCached(model, type);
                sceneFeature = FeatureDetector.Detect(scene, type);
                (double confidence, Rect circumRect) = FeatureMatcher.Match(modelFeature, sceneFeature);
                return new RegisterResult(circumRect, confidence);
            }
            finally
            {
                if (!_useCache) modelFeature?.Dispose();
                sceneFeature?.Dispose();
            }
        }
    }
}