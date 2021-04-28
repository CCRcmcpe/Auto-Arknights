using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public abstract class ImageRegistration
    {
        public abstract RegistrationResult[] Register(Mat model, Mat scene, int minMatchCount);
    }
}