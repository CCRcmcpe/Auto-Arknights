using System.IO;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    internal static class Util
    {
        public static Mat Imread(string path, ImreadModes mode = ImreadModes.Color)
        {
            return Cv2.ImDecode(File.ReadAllBytes(path), mode);
        }
    }
}