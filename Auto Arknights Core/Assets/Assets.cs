using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public class Assets : IDisposable
    {
        private readonly Dictionary<string, Mat> _memoryCacheMap = new Dictionary<string, Mat>();

        public void Dispose()
        {
            foreach (Mat mat in _memoryCacheMap.Values) mat.Dispose();
        }

        public Mat Get(string expr)
        {
            expr = expr.Trim();
            if (!_memoryCacheMap.TryGetValue(expr, out Mat? mat))
            {
                string fileName = GetFileName(expr);
                if (!File.Exists(fileName)) throw new IOException($"Asset not found: {expr}");
                mat = Utils.Imread(fileName);
                if (mat.Empty()) throw new IOException($"Invalid asset: {expr}");

                _memoryCacheMap.Add(expr, mat);
            }

            return mat;
        }

        private static string GetFileName(string expr)
        {
            return Path.Combine("Assets", string.Join('\\', expr.Split(' ', StringSplitOptions.RemoveEmptyEntries))) +
                   ".png";
        }
    }
}