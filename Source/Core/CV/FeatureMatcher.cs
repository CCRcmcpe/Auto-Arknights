using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using REVUnit.AutoArknights.Core.Properties;

namespace REVUnit.AutoArknights.Core.CV
{
    public class FeatureMatcher : IDisposable
    {
        private readonly BFMatcher _matcher = new();
        public float NearestNeighborThreshold { get; set; } = 0.75f;

        public void Dispose()
        {
            _matcher.Dispose();
        }

        public (int matchCount, Rect circumRect) Match(MatFeature modelF, MatFeature sceneF)
        {
            if (modelF.Type != sceneF.Type)
                throw new ArgumentException(string.Format(Resources.FeatureMatcher_Exception_FeatureTypesMismatch,
                    nameof(modelF), nameof(sceneF)));
            if (modelF.KeyPoints.Length == 0 || modelF.Descriptors.Empty() || sceneF.KeyPoints.Length == 0 ||
                sceneF.Descriptors.Empty())
                return default;

            DMatch[][] matches = _matcher.KnnMatch(modelF.Descriptors, sceneF.Descriptors, 2);

            using var mask = new Mat(matches.Length, 1, MatType.CV_8U);
            var mPoints = new List<Point2f>();
            var sPoints = new List<Point2f>();

            for (var i = 0; i < matches.Length; i++)
            {
                DMatch[] dim = matches[i];
                DMatch a = dim[0];
                DMatch b = dim[1];
                if (a.Distance < NearestNeighborThreshold * b.Distance)
                {
                    mPoints.Add(modelF.KeyPoints[a.QueryIdx].Pt);
                    sPoints.Add(sceneF.KeyPoints[a.TrainIdx].Pt);
                    mask.Set(i, true);
                }
                else
                {
                    mask.Set(i, false);
                }
            }

            if (mask.CountNonZero() < 4)
            {
                return default;
            }

            int nonZero = VoteForSizeAndOrientation(modelF.KeyPoints, sceneF.KeyPoints, matches, mask, 1.5f, 20);
            if (nonZero < 4) return (nonZero, Rect.Empty);

            using Mat homography = Cv2.FindHomography(InputArray.Create(mPoints), InputArray.Create(sPoints),
                HomographyMethods.Ransac);

            if (homography.Empty()) return default;

            Point2f[] mCorners =
            {
                new(0, 0), new(modelF.MatWidth, 0), new(modelF.MatWidth, modelF.MatHeight), new(0, modelF.MatHeight)
            };

            Point2f[] mCornersFt = Cv2.PerspectiveTransform(mCorners, homography);
            Point[] mCornersT = mCornersFt.Select(it => new Point(it.X, it.Y)).ToArray();

            Point topLeft = mCornersT[0];
            Point bottomRight = mCornersT[2];
            Rect circumRect = Rect.FromLTRB(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);

            if (circumRect.Width < 10 || circumRect.Height < 10 || circumRect.X < 0 && circumRect.Y < 0 ||
                circumRect.Height > sceneF.MatHeight && circumRect.Width > sceneF.MatWidth)
                return default;
            return (nonZero, circumRect);
        }

        private static int VoteForSizeAndOrientation(KeyPoint[] modelKeyPoints, KeyPoint[] sceneKeyPoints,
            DMatch[][] matches, Mat mask, float scaleIncrement,
            int rotationBins)
        {
            var logScale = new List<float>();
            var rotations = new List<float>();
            double maxS = -1.0e-10f;
            double minS = 1.0e10f;

            for (var i = 0; i < mask.Rows; i++)
            {
                if (!mask.At<bool>(i)) continue;

                KeyPoint modelKeyPoint = modelKeyPoints[i];
                KeyPoint sceneKeyPoint = sceneKeyPoints[matches[i][0].TrainIdx];
                double s = Math.Log10(sceneKeyPoint.Size / modelKeyPoint.Size);
                logScale.Add((float) s);
                maxS = s > maxS ? s : maxS;
                minS = s < minS ? s : minS;

                double r = sceneKeyPoint.Angle - modelKeyPoint.Angle;
                r = r < 0.0f ? r + 360.0f : r;
                rotations.Add((float) r);
            }

            var scaleBinSize = (int) Math.Ceiling((maxS - minS) / Math.Log10(scaleIncrement));
            if (scaleBinSize < 2) scaleBinSize = 2;

            float[] scaleRanges = {(float) minS, (float) (minS + scaleBinSize + Math.Log10(scaleIncrement))};

            using var flagsMat = new Mat<float>(logScale.Count, 1);
            using var hist = new Mat();

            int[] histSize = {scaleBinSize, rotationBins};
            int[] channels = {0, 1};
            Rangef[] ranges = {new(scaleRanges[0], scaleRanges[1]), new(rotations.Min(), rotations.Max())};

            using var scalesMat = new Mat<float>(logScale.Count, 1, logScale.ToArray());
            using var rotationsMat = new Mat<float>(rotations.Count, 1, rotations.ToArray());
            Mat[] scalesAndRotations = {scalesMat, rotationsMat};

            Cv2.CalcHist(scalesAndRotations, channels, null, hist, 2, histSize, ranges);
            Cv2.MinMaxLoc(hist, out _, out double maxVal);

            Cv2.Threshold(hist, hist, maxVal * 0.5, 0, ThresholdTypes.Tozero);
            Cv2.CalcBackProject(scalesAndRotations, channels, hist, flagsMat, ranges);

            return flagsMat.CountNonZero();
        }
    }
}