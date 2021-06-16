using System.Threading.Tasks;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public interface IDevice
    {
        Task Back();
        Task Click(Point point);
        Task<Size> GetResolution();
        Task<Mat> GetScreenshot();
    }
}