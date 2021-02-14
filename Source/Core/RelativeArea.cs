using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public class RelativeArea
    {
        // 系数
        public readonly double CBottom;
        public readonly double CLeft;
        public readonly double CRight;
        public readonly double CTop;

        public RelativeArea(double cLeft, double cTop, double cRight, double cBottom)
        {
            CLeft = cLeft;
            CTop = cTop;
            CRight = cRight;
            CBottom = cBottom;
        }

        public Rect For(Size size)
        {
            int refW = size.Width;
            int refH = size.Height;
            var left = (int) Math.Round(refW * CLeft);
            var top = (int) Math.Round(refH * CTop);
            var right = (int) Math.Round(refW * CRight);
            var bottom = (int) Math.Round(refH * CBottom);

            if (right > refW) right = refW;
            if (bottom > refH) bottom = refH;

            return Rect.FromLTRB(left, top, right, bottom);
        }

        public Mat Reduce(Mat super) => super.Clone(For(super.Size()));

        #region 列表

        // 开发参考分辨率
        public const double Rw = 1920.0;
        public const double Rh = 1080.0;

        public static readonly RelativeArea CurrentSanity = Ref1080P(1672, 23, 1919, 97);
        public static readonly RelativeArea RequiredSanity = Ref1080P(1763, 1014, 1841, 1053);

        public static readonly RelativeArea LevelCompletedScreenCloseClick = Ref1080P(0, 0, 1920, 700);
        //public static readonly RelativeArea LevelCompletedText = Ref1080P(23, 860, 637, 1026);

        private static RelativeArea Ref1080P(int left, int top, int right, int bottom) =>
            new(left / Rw, top / Rh, right / Rw, bottom / Rh);

        #endregion
    }
}