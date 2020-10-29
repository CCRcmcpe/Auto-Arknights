using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal class ScreenArea
    {
        public const double Rw = 1920.0;
        public const double Rh = 1080.0;
        public static readonly ScreenArea CurrentSanity = new ScreenArea(1672 / Rw, 23 / Rh, 247 / Rw, 74 / Rh);
        public static readonly ScreenArea RequiredSanity = new ScreenArea(1763 / Rw, 1014 / Rh, 78 / Rw, 39 / Rh);

        public readonly double Ch;
        public readonly double Cw;
        public readonly double Cx;
        public readonly double Cy;

        public ScreenArea(double cx, double cy, double cw, double ch)
        {
            Cx = cx;
            Cy = cy;
            Cw = cw;
            Ch = ch;
        }

        public Mat Apply(Mat super)
        {
            int sw = super.Width;
            int sh = super.Height;
            var x = (int) Math.Round(sw * Cx);
            var y = (int) Math.Round(sh * Cy);
            var w = (int) Math.Round(sw * Cw);
            var h = (int) Math.Round(sh * Ch);
            if (sw - x - w > 0) w = sw - x;

            if (sh - y - h > 0) h = sh - y;

            return super.Clone(new Rect(x, y, w, h));
        }
    }
}