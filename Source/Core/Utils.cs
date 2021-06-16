using System.IO;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal static class Utils
    {
        // public static void Sleep(double seconds)
        // {
        //     Thread.Sleep(TimeSpan.FromSeconds(seconds));
        // }

        public static Mat Imread(string path, ImreadModes mode = ImreadModes.Color)
        {
            return Cv2.ImDecode(File.ReadAllBytes(path), mode);
        }
    }
}