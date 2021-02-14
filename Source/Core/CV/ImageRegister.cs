using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public abstract class ImageRegister
    {
        public abstract RegisterResult[] Register(Mat model, Mat scene, int minMatchCount);
    }
}