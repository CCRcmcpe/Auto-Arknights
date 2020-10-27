using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace REVUnit.AutoArknights.Core.CV
{
    public class FeatureMatcher : IDisposable
    {
        private readonly Feature2D _f2d;

        public FeatureMatcher(Feature2D feature2D)
        {
            _f2d = feature2D;
            Matcher = new BFMatcher((NormTypes) feature2D.DefaultNorm);
        }

        public DescriptorMatcher Matcher { get; }

        /// <summary>
        ///     Nearest-neighbour matching ratio, usually between 0.4 and 0.6.
        /// </summary>
        public float NnMatchRatio { get; set; } = 0.6f;

        /// <summary>
        ///     RANSAC inlier threshold.
        /// </summary>
        public double RansacThreshold { get; set; } = 3f;

        public float SuccessThreshold { get; set; } = 1f / 3f;

        public void Dispose()
        {
            Matcher.Dispose();
        }

        public bool Match(MatFeature mf, MatFeature of, out Rectangle rect)
        {
            rect = Rectangle.Empty;

            if (mf.Descriptors.Empty() || of.Descriptors.Empty() || !mf.KeyPoints.Any() || !of.KeyPoints.Any())
                return false;

            mf.Descriptors.ConvertTo(mf.Descriptors, _f2d.DescriptorType);
            of.Descriptors.ConvertTo(of.Descriptors, _f2d.DescriptorType);

            DMatch[][] matches = Matcher.KnnMatch(mf.Descriptors, of.Descriptors, 2);

            var modelPoints = new List<Point2f>();
            var observedPoints = new List<Point2f>();
            using var mask = new Mat(matches.Length, 1, MatType.CV_8U);

            for (var i = 0; i < mask.Rows; i++) mask.Set(i, true);

            for (var i = 0; i < matches.Length; i++)
            {
                DMatch[] dim = matches[i];
                DMatch matchA = dim[0];
                DMatch matchB = dim[1];
                if (matchA.Distance < NnMatchRatio * matchB.Distance)
                {
                    modelPoints.Add(mf.KeyPoints[matchA.QueryIdx].Pt);
                    observedPoints.Add(of.KeyPoints[matchA.TrainIdx].Pt);
                    mask.Set(i, true);
                }
                else
                    mask.Set(i, false);
            }

            if ((float) mask.CountNonZero() / matches.Length < SuccessThreshold) return false;

            int validMatchCount = VoteForSizeAndOrientation(mf.KeyPoints, of.KeyPoints, matches, mask, 1.5f, 20);
            if ((float) validMatchCount / matches.Length < SuccessThreshold) return false;

            using Mat homography = Cv2.FindHomography(InputArray.Create(modelPoints), InputArray.Create(observedPoints),
                                                      HomographyMethods.Ransac, RansacThreshold, mask);

            if (homography.Empty()) return false;

            Point2f[] mCorners =
            {
                new Point2f(0, 0), new Point2f(mf.OriginWidth, 0), new Point2f(mf.OriginWidth, mf.OriginHeight),
                new Point2f(0, mf.OriginHeight)
            };

            Point2f[] mCornersFt = Cv2.PerspectiveTransform(mCorners, homography);
            Point[] mCornersT = mCornersFt.Select(it => new Point(it.X, it.Y)).ToArray();

            rect = new Rectangle(mCornersT[0].X, mCornersT[0].Y,
                                 mCornersT[1].X - mCornersT[0].X,
                                 mCornersT[3].Y - mCornersT[0].Y);

            return rect.Width >= 10 && rect.Height >= 10 && rect.X >= 0 && rect.Y >= 0 &&
                   rect.Height <= of.OriginHeight && rect.Width <= of.OriginWidth && !rect.IsEmpty;
        }

        private static int VoteForSizeAndOrientation(KeyPoint[] modelKeyPoints, KeyPoint[] observedKeyPoints,
                                                     DMatch[][] matches, Mat mask, float scaleIncrement,
                                                     int rotationBins)
        {
            var idx = 0;
            var nonZero = 0;
            var logScale = new List<float>();
            var rotations = new List<float>();
            double maxS = -1.0e-10f;
            double minS = 1.0e10f;

            for (var i = 0; i < mask.Rows; i++)
            {
                if (mask.At<byte>(i) > 0)
                {
                    KeyPoint modelKeyPoint = modelKeyPoints[i];
                    KeyPoint observedKeyPoint = observedKeyPoints[matches[i][0].TrainIdx];
                    double s = Math.Log10(observedKeyPoint.Size / modelKeyPoint.Size);
                    logScale.Add((float) s);
                    maxS = s > maxS ? s : maxS;
                    minS = s < minS ? s : minS;

                    double r = observedKeyPoint.Angle - modelKeyPoint.Angle;
                    r = r < 0.0f ? r + 360.0f : r;
                    rotations.Add((float) r);
                }
            }

            var scaleBinSize = (int) Math.Ceiling((maxS - minS) / Math.Log10(scaleIncrement));
            if (scaleBinSize < 2) scaleBinSize = 2;
            float[] scaleRanges = { (float) minS, (float) (minS + scaleBinSize + Math.Log10(scaleIncrement)) };

            using var scalesMat = new Mat<float>(logScale.Count, 1, logScale.ToArray());
            using var rotationsMat = new Mat<float>(rotations.Count, 1, rotations.ToArray());
            using var flagsMat = new Mat<float>(logScale.Count, 1);
            using var hist = new Mat();
            flagsMat.SetTo(new Scalar(0.0f));

            int[] histSize = { scaleBinSize, rotationBins };
            int[] channels = { 0, 1 };
            Rangef[] ranges =
            {
                new Rangef(scaleRanges[0], scaleRanges[1]), new Rangef(rotations.Min(), rotations.Max())
            };

            Mat[] arrs = { scalesMat, rotationsMat };
            Cv2.CalcHist(arrs, channels, null, hist, 2, histSize, ranges);
            Cv2.MinMaxLoc(hist, out _, out double maxVal);

            Cv2.Threshold(hist, hist, maxVal * 0.5, 0, ThresholdTypes.Tozero);
            Cv2.CalcBackProject(arrs, channels, hist, flagsMat, ranges);

            MatIndexer<float> flagsMatIndexer = flagsMat.GetIndexer();

            for (var i = 0; i < mask.Rows; i++)
            {
                if (mask.At<bool>(i))
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (flagsMatIndexer[idx++] != 0.0f)
                        nonZero++;
                    else
                        mask.Set(i, false);
                }
            }

            return nonZero;
        }
    }
}