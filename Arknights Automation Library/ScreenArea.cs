using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal static class ScreenArea
    {
        public static Rect2f CurrentSanity = new Rect2f(0.872f, 0.021f, 0.125f, 0.07f);
        public static Rect2f RequiredSanity = new Rect2f(0.92f, 0.9426f, 0.04f, 0.0296f);

        public static Mat Apply(this Rect2f area, Mat super)
        {
            int w = super.Width;
            int h = super.Height;
            return super.Clone(new Rect((int) Math.Round(w * area.X), (int) Math.Round(h * area.Y),
                (int) Math.Round(w * area.Width),
                (int) Math.Round(h * area.Height)));
        }
    }
}