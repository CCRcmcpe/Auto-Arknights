using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core
{
    public class AssetLoadException : Exception
    {
        public AssetLoadException(string key) : base($"无法载入资源: {key}") { }
    }

    public class ImageAssets : IDisposable
    {
        private readonly Dictionary<string, Mat> _cache = new();
        public static Size TargetSize { get; } = new (1920, 1080);

        public void Dispose()
        {
            foreach (Mat mat in _cache.Values) mat.Dispose();
        }

        public Mat Get(string key)
        {
            key = key.Trim();
            if (_cache.TryGetValue(key, out Mat? mat)) return mat;

            string fileName = GetFilePath(key);
            if (!File.Exists(fileName)) throw new AssetLoadException(key);

            mat = Utils.Imread(fileName);
            if (mat.Empty()) throw new AssetLoadException(key);

            _cache.Add(key, mat);
            return mat;
        }

        private static string GetFilePath(string key) => Path.Combine("Assets", key) + ".png";
    }
}