using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public readonly struct RelativeArea
    {
        // 系数
        public readonly double CLeft;
        public readonly double CRight;
        public readonly double CTop;
        public readonly double CBottom;

        public static RelativeArea All { get; } = new(0, 0, 1, 1);

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

        public Mat Reduce(Mat super)
        {
            return super.Clone(For(super.Size()));
        }

        #region 常量

        // 开发参考分辨率
        public const double Rw = 1920.0;
        public const double Rh = 1080.0;

        public static RelativeArea CurrentSanity { get; } = Ref1080P(1672, 23, 1919, 97);
        public static RelativeArea RequiredSanity { get; } = Ref1080P(1763, 1014, 1841, 1053);
        public static RelativeArea LevelCompletedScreenCloseClick { get; } = Ref1080P(0, 0, 1920, 700);

        private static RelativeArea Ref1080P(int left, int top, int right, int bottom)
        {
            return new(left / Rw, top / Rh, right / Rw, bottom / Rh);
        }

        #endregion
    }
}