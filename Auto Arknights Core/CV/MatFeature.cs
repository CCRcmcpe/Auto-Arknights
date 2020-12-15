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
        public readonly DeformationLevel Type;

        public MatFeature(KeyPoint[] keyPoints, Mat descriptors, int matWidth,
                          int matHeight, DeformationLevel type)
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