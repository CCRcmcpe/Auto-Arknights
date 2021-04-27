using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public interface IDevice
    {
        Size GetResolution();
        void Click(Point point);
        Mat GetScreenshot();
    }
}