using OpenCvSharp;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core.CV
{
    public class RegistrationResult
    {
        public RegistrationResult(Rect circumRect, double confidence)
        {
            CircumRect = circumRect;
            CenterPoint = new Point(circumRect.Left + circumRect.Width / 2, circumRect.Top + circumRect.Height / 2);
            Confidence = confidence;
        }

        public Rect CircumRect { get; }
        public Point CenterPoint { get; }
        public double Confidence { get; }
    }
}