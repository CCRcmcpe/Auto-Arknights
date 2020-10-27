using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class MatFeature : IDisposable
    {
        public readonly Mat Descriptors;
        public readonly KeyPoint[] KeyPoints;
        public readonly int OriginHeight;
        public readonly int OriginWidth;

        public MatFeature(KeyPoint[] keyPoints, Mat descriptors, int originWidth,
                          int originHeight)
        {
            KeyPoints = keyPoints;
            Descriptors = descriptors;
            OriginWidth = originWidth;
            OriginHeight = originHeight;
        }

        public void Dispose()
        {
            Descriptors.Dispose();
        }
    }
}