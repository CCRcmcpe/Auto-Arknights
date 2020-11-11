using System.Drawing;

namespace REVUnit.AutoArknights.Core.CV
{
    public class LocateResult
    {
        public bool IsSucceed { get; init; }
        public Point CenterPoint { get; init; }
        public Rectangle Rect { get; init; }

        public static LocateResult Failed() => new();

        public static LocateResult Succeed(Rectangle rect) =>
            new()
            {
                IsSucceed = true,
                Rect = rect,
                CenterPoint = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2)
            };
    }
}