using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class MatFeature : IDisposable
    {
        public readonly Mat Descriptors;
        public readonly KeyPoint[] KeyPoints;
        public readonly int MatHeight;
        public readonly int MatWidth;
        public readonly Feature2DType Type;

        public MatFeature(KeyPoint[] keyPoints, Mat descriptors, int matWidth,
            int matHeight, Feature2DType type)
        {
            KeyPoints = keyPoints;
            Descriptors = descriptors;
            MatWidth = matWidth;
            MatHeight = matHeight;
            Type = type;
        }

        public void Dispose()
        {
            Descriptors.Dispose();
        }
    }
}