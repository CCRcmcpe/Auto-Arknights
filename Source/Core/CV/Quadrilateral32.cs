using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    /// <summary>
    ///     Represents a convex quadrilateral, using 32-bit floating point numbers.
    /// </summary>
    public struct Quadrilateral32
    {
        public Point2f TopLeft;
        public Point2f TopRight;
        public Point2f BottomRight;
        public Point2f BottomLeft;

        public Quadrilateral32(Point2f topLeft, Point2f topRight, Point2f bottomRight, Point2f bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public static readonly Quadrilateral32 Empty = new();

        public static Quadrilateral32 FromVertices(params Point2f[] vertices)
        {
            return new(vertices[0], vertices[1], vertices[2], vertices[3]);
        }

        public static Quadrilateral32 FromRect(Rect rect)
        {
            return FromVertices(new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top), new Point(rect.Right,
                rect.Bottom), new Point(rect.Left, rect.Bottom));
        }

        public Rect BoundingRectangle =>
            Rect.FromLTRB((int) MathF.Round(MathF.Min(TopLeft.X, BottomLeft.X)),
                (int) MathF.Round(MathF.Min(TopLeft.Y, TopRight.Y)),
                (int) MathF.Round(MathF.Max(TopRight.X, BottomRight.X)),
                (int) MathF.Round(MathF.Max(BottomLeft.Y, BottomRight.Y)));

        public Point2f[] Vertices => new[] {TopLeft, TopRight, BottomRight, BottomLeft};

        public Point2f VertexCentroid => (TopLeft + TopRight + BottomRight + BottomLeft).Multiply(1.0f / 4.0f);

        public Quadrilateral32 ScaleTo(float factor)
        {
            Point2f r = VertexCentroid.Multiply(1 - factor);
            return new Quadrilateral32(TopLeft.Multiply(factor) + r, TopRight.Multiply(factor) + r,
                BottomRight.Multiply(factor) + r, BottomLeft.Multiply(factor) + r);
        }

        public Point PickRandomPoint()
        {
            // Brute force algorithm.

            Rect rect = BoundingRectangle;
            var random = new Random();

            Point point;
            do
            {
                point = new Point(random.Next(rect.X, rect.X + rect.Width),
                    random.Next(rect.Y, rect.Y + rect.Height));
            } while (!IsPointInPolygon(point, Vertices));

            return point;
        }

        private static unsafe bool IsPointInPolygon(Point2f point, Point2f[] vertices)
        {
            static void Swap<T>(T* a, T* b) where T : unmanaged
            {
                T t = *a;
                *a = *b;
                *b = t;
            }

            // Split polygons along set of x axes

            int verticeCount = vertices.Length;

            Span<Point2f> v = stackalloc Point2f[verticeCount];
            for (var i = 0; i < verticeCount; i++)
            {
                v[i] = vertices[i];
            }

            float* len = stackalloc float[3];

            int spCount = verticeCount - 2;
            SizePlanePair* sp = stackalloc SizePlanePair[spCount];
            int pCount = 3 * spCount;
            PlaneSet* p = stackalloc PlaneSet[pCount];
            PlaneSet* pOrigin = p;

            float v0X = v[0].X;
            float v0Y = v[0].Y;

            for (int p1 = 1, p2 = 2; p2 < verticeCount; p1++, p2++)
            {
                p->Vx = v0Y - v[p1].Y;
                p->Vy = v[p1].X - v0X;
                p->C = p->Vx * v0X + p->Vy * v0Y;
                len[0] = p->Vx * p->Vx + p->Vy * p->Vy;
                p->IsExterior = p1 == 1;
                /* Sort triangles by areas, so compute (twice) the area here */
                sp[p1 - 1].PPlaneSet = p;
                sp[p1 - 1].Size =
                    v[0].X * v[p1].Y +
                    v[p1].X * v[p2].Y +
                    v[p2].X * v[0].Y -
                    v[p1].X * v[0].Y -
                    v[p2].X * v[p1].Y -
                    v[0].X * v[p2].Y;
                p++;

                p->Vx = v[p1].Y - v[p2].Y;
                p->Vy = v[p2].X - v[p1].X;
                p->C = p->Vx * v[p1].X + p->Vy * v[p1].Y;
                len[1] = p->Vx * p->Vx + p->Vy * p->Vy;
                p->IsExterior = true;
                p++;

                p->Vx = v[p2].Y - v0Y;
                p->Vy = v0X - v[p2].X;
                p->C = p->Vx * v[p2].X + p->Vy * v[p2].Y;
                len[2] = p->Vx * p->Vx + p->Vy * p->Vy;
                p->IsExterior = p2 == verticeCount - 1;

                /* find an average point which must be inside of the triangle */
                float tx1 = (v0X + v[p1].X + v[p2].X) / 3.0f;
                float ty1 = (v0Y + v[p1].Y + v[p2].Y) / 3.0f;

                /* check sense and reverse if test point is not thought to be inside
                 * first triangle
                 */
                if (p->Vx * tx1 + p->Vy * ty1 >= p->C)
                {
                    /* back up to start of plane set */
                    p -= 2;
                    /* point is thought to be outside, so reverse sense of edge
                     * normals so that it is correctly considered inside.
                     */
                    for (var i = 0; i < 3; i++)
                    {
                        *p = -*p;
                        p++;
                    }
                }
                else
                {
                    p++;
                }

                /* sort the planes based on the edge lengths */
                p -= 3;
                for (var i = 0; i < 2; i++)
                {
                    for (int j = i + 1; j < 3; j++)
                    {
                        if (len[i] < len[j])
                        {
                            Swap(p + i, p + j);
                            Swap(len + i, len + j);
                        }
                    }
                }

                p += 3;
            }

            SizePlanePair[] spHeap = new SizePlanePair[spCount];
            for (var i = 0; i < spCount; i++)
            {
                spHeap[i] = sp[i];
            }

            /* sort the triangles based on their areas */
            Array.Sort(spHeap);

            p = pOrigin;
            /* make the plane sets match the sorted order */
            for (var i = 0; i < spCount; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    Swap(p + j, spHeap[i].PPlaneSet);
                }
            }

            // Check point for inside of three "planes" formed by triangle edges

            float tx = point.X;
            float ty = point.Y;

            for (int i = verticeCount - 1; i >= 0; i--)
            {
                if (p->Vx * tx + p->Vy * ty < p->C)
                {
                    p++;
                    if (p->Vx * tx + p->Vy * ty < p->C)
                    {
                        p++;
                        /* note: we make the third edge have a slightly different
                         * equality condition, since this third edge is in fact
                         * the next triangle's first edge.  Not fool-proof, but
                         * it doesn't hurt (better would be to keep track of the
                         * triangle's area sign so we would know which kind of
                         * triangle this is).  Note that edge sorting nullifies
                         * this special inequality, too.
                         */
                        if (p->Vx * tx + p->Vy * ty <= p->C)
                        {
                            /* point is inside polygon */
                            return true;
                        }
                        /* check if outside exterior edge */

                        if (p->IsExterior)
                        {
                            return false;
                        }

                        p++;
                    }
                    else
                    {
                        /* check if outside exterior edge */
                        if (p->IsExterior) return false;
                        /* get past last two plane tests */
                        p += 2;
                    }
                }
                else
                {
                    /* check if outside exterior edge */
                    if (p->IsExterior) return false;
                    /* get past all three plane tests */
                    p += 3;
                }
            }

            /* for convex, if we make it to here, all triangles were missed */
            return false;
        }

        private struct PlaneSet
        {
            public float Vx, Vy, C; /* edge equation  vx*X + vy*Y + c = 0 */
            public bool IsExterior; /* TRUE == exterior edge of polygon */

            public PlaneSet(float vx, float vy, float c, bool isExterior)
            {
                Vx = vx;
                Vy = vy;
                C = c;
                IsExterior = isExterior;
            }

            public static PlaneSet operator -(PlaneSet p)
            {
                return new(-p.Vx, -p.Vy, -p.C, p.IsExterior);
            }
        }

        private unsafe struct SizePlanePair : IComparable<SizePlanePair>
        {
            public float Size;
            public PlaneSet* PPlaneSet;

            public int CompareTo(SizePlanePair other)
            {
                return Size.CompareTo(other.Size);
            }
        }
    }
}