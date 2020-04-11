using System;
using System.IO;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public class Assets : IDisposable
    {
        private readonly Cache<string, Mat> _cache = new Cache<string, Mat>();

        public void Dispose()
        {
            foreach (Mat mat in _cache.CachedItems) mat.Dispose();
        }

        public Mat Get(string expr)
        {
            expr = expr.Trim();
            if (!_cache.Get(expr, out Mat mat))
            {
                string fileName = GetFileName(expr);
                if (!File.Exists(fileName)) throw new IOException($"Asset not found: {expr}");
                mat = Util.Imread(fileName);
                if (mat.Empty()) throw new IOException($"Invalid asset: {expr}");

                _cache.Register(expr, mat);
            }

            return mat;
        }

        private string GetFileName(string expr)
        {
            return Path.GetFullPath(Path.Combine("Assets",
                                        string.Join('\\',
                                            expr.Split(' ', StringSplitOptions.RemoveEmptyEntries))) +
                                    ".png");
        }
    }
}