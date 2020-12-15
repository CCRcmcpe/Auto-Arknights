using OpenCvSharp;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core.CV
{
    public class RegisterResult
    {
        public double Confidence { get; init; }
        public Rect CircumRect { get; init; }
        public Point CenterPoint { get; init; }

        public static RegisterResult Failed() => new();

        public static RegisterResult Succeed(Rect rect) =>
            new()
            {
                CircumRect = rect,
                CenterPoint = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2)
            };
    }
}