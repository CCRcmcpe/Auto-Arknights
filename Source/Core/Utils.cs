using System;
using System.IO;
using System.Threading;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal static class Utils
    {
        public static Mat Imread(string path, ImreadModes mode = ImreadModes.Color)
        {
            return Cv2.ImDecode(File.ReadAllBytes(path), mode);
        }

        public static Point ToCvPoint(this System.Drawing.Point point)
        {
            return new(point.X, point.Y);
        }

        public static System.Drawing.Point ToPoint(this Point point)
        {
            return new(point.X, point.Y);
        }

        public static void Sleep(double seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }
    }
}