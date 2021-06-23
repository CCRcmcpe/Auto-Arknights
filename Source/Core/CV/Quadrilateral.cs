using System;
using System.Linq;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public struct Quadrilateral
    {
        public Point TopLeft;
        public Point TopRight;
        public Point BottomRight;
        public Point BottomLeft;

        public Quadrilateral(Point topLeft, Point topRight, Point bottomRight, Point bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public static readonly Quadrilateral Empty = new();

        public static Quadrilateral FromVertices(params Point[] vertices)
        {
            return new(vertices[0], vertices[1], vertices[2], vertices[3]);
        }

        public static Quadrilateral FromRect(Rect rect)
        {
            return FromVertices(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top), new Point(rect.Right,
                rect.Bottom), new Point(rect.Left, rect.Bottom));
        }

        public Rect BoundingRectangle =>
            Rect.FromLTRB(Math.Min(TopLeft.X, BottomLeft.X), Math.Min(TopLeft.Y, TopRight.Y),
                Math.Max(TopRight.X, BottomRight.X), Math.Max(BottomLeft.Y, BottomRight.Y));

        public Point[] Vertices => new[] {TopLeft, TopRight, BottomRight, BottomLeft};

        public Point Center => (TopLeft + TopRight + BottomRight + BottomLeft) * (1.0 / 4.0);

        public Quadrilateral ScaleTo(double factor)
        {
            return new(TopLeft * factor, TopRight * factor, BottomRight * factor, BottomLeft * factor);
        }

        public Point PickRandomPoint()
        {
            // Brute force algorithm.
            // Bug: Only returns (x:1064 y:811)

            Rect rect = BoundingRectangle;
            var random = new Random();

            Point point;
            do
            {
                point = new Point(random.Next(rect.X, rect.X + rect.Width),
                    random.Next(rect.Y, rect.Y + rect.Height));
            } while (new[]
            {
                (TopLeft - point).DotProduct(TopRight - TopLeft),
                (TopLeft - point).DotProduct(BottomLeft - TopLeft),
                (TopRight - point).DotProduct(TopLeft - TopLeft),
                (TopRight - point).DotProduct(BottomRight - TopLeft),
                (BottomRight - point).DotProduct(TopRight - TopLeft),
                (BottomRight - point).DotProduct(BottomLeft - TopLeft),
                (BottomLeft - point).DotProduct(BottomRight - TopLeft),
                (BottomLeft - point).DotProduct(TopLeft - TopLeft)
            }.Any(i => i < 0));

            return point;
        }
    }
}