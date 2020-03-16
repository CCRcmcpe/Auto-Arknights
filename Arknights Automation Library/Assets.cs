using System;
using System.IO;
using OpenCvSharp;

namespace REVUnit.AutoArknights.GUI.Core
{
    public static class Assets
    {
        private static readonly Cache<string, Mat> Cache = new Cache<string, Mat>();

        static Assets()
        {
            if (!Directory.Exists("Assets")) Directory.CreateDirectory("Assets");
            Initialized = true;
        }

        public static bool Initialized { get; }

        public static Mat Get(string expression)
        {
            expression = expression.Trim();
            return Cache.Get(expression, () =>
            {
                var mat = new Mat(GetFileName(expression));
                if (mat.Empty()) throw new Exception($"Asset not found : {expression}");
                return mat;
            });
        }

        private static string GetFileName(string expression)
        {
            return Path.GetFullPath(Path.Combine("Assets", expression.Replace(' ', '\\')) + ".png");
        }
    }
}