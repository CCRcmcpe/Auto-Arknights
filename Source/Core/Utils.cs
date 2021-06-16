using System;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal static class Utils
    {
        // public static void Sleep(double seconds)
        // {
        //     Thread.Sleep(TimeSpan.FromSeconds(seconds));
        // }

        public static Task Delay(double seconds)
        {
            return Task.Delay(TimeSpan.FromSeconds(seconds));
        }

        public static Mat Imread(string path, ImreadModes mode = ImreadModes.Color)
        {
            return Cv2.ImDecode(File.ReadAllBytes(path), mode);
        }
    }
}