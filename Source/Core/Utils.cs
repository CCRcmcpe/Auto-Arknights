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

        public static Task<Mat> Imread(string path, ImreadModes mode = ImreadModes.Color)
        {
            return Task.Run(() => Cv2.ImDecode(File.ReadAllBytes(path), mode));
        }
    }
}