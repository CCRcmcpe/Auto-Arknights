using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class TemplateMatchBasedImageRegister : ImageRegister
    {
        public override RegisterResult Register(Mat model, Mat observed)
        {
            NormalizeScale(observed);
            
        }
    }
}