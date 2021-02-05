using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public abstract class ImageRegister
    {
        private static void NormalizeScale(Mat mat)
        {
            Size s1 = mat.Size();
            Size s2 = ImageAssets.TargetSize;
            if (s1 == s2) return;

            bool smallerThanNormal = s2.Width > s1.Width || s1.Height > s2.Height;
            Cv2.Resize(mat, mat, s2, interpolation: smallerThanNormal
                           ? InterpolationFlags.Cubic
                           : InterpolationFlags.Area);
        }

        public RegisterResult[] Register(Mat model, Mat observed, int minMatchCount)
        {
            NormalizeScale(observed);
            return RegisterInternal(model, observed, minMatchCount);
        }

        protected abstract RegisterResult[] RegisterInternal(Mat model, Mat observed, int minMatchCount);
    }
}