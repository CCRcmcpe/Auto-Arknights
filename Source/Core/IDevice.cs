using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public interface IDevice
    {
        Size GetResolution();
        void Back();
        void Click(Point point);
        Mat GetScreenshot();
    }
}