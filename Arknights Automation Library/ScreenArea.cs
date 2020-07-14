using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal static class ScreenArea
    {
        public static Rect2f CurrentSanity = new Rect2f(0.875f, 0.023f, 0.11f, 0.065f);
        public static Rect2f RequiredSanity = new Rect2f(0.9271f, 0.9426f, 0.0323f, 0.0296f);

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