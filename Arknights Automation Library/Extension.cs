using System;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public static class Extension
    {
        private static readonly Random Random = new Random();

        public static Point ToCvPoint(this System.Drawing.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static System.Drawing.Point ToPoint(this Point point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }

        public static void Click(this System.Drawing.Point point)
        {
            static int NewConfusementValue()
            {
                return Random.Next(-3, 3);
            }

            // 反脚本措施反制机制，使每次点击的位置都有所区别
            point = new System.Drawing.Point(Math.Abs(point.X + NewConfusementValue()),
                Math.Abs(point.Y + NewConfusementValue()));
            Console.WriteLine(point);
            //Automation.I.Adb.Clk(point);
        }
    }
}