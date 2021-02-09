using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public abstract class ImageRegister
    {
        private static void NormalizeScale(Mat mat)
        {
            Size size = mat.Size();
            Size targetSize = ImageAssets.TargetSize;
            if (size == targetSize) return;

            bool upscale = size.Width < targetSize.Width || size.Height < targetSize.Height;
            Cv2.Resize(mat, mat, targetSize, interpolation: upscale
                           ? InterpolationFlags.Cubic
                           : InterpolationFlags.Area);
        }

        public RegisterResult[] Register(Mat model, Mat scene, int minMatchCount)
        {
            NormalizeScale(scene);
            return RegisterInternal(model, scene, minMatchCount);
        }

        protected abstract RegisterResult[] RegisterInternal(Mat model, Mat scene, int minMatchCount);
    }
}