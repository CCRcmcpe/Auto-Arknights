using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public partial class FeatureDetector
    {
        private class Cache : IDisposable
        {
            private static readonly string _coreAssemblySerial =
                GetMd5(File.ReadAllBytes(Assembly.GetCallingAssembly().Location));

            private readonly string _cacheDirPath;
            private readonly Dictionary<string, MatFeature> _md5map = new();
            private readonly string _serialFilePath;

            public Cache(string cacheDirPath)
            {
                _cacheDirPath = cacheDirPath;
                Directory.CreateDirectory(cacheDirPath);

                _serialFilePath = Path.Combine(_cacheDirPath, "version");

                // Rebuild cache when updated

                var rebuildCache = true;
                if (File.Exists(_serialFilePath))
                {
                    string serial = File.ReadAllText(_serialFilePath);
                    rebuildCache = _coreAssemblySerial != serial;
                }

                if (rebuildCache)
                    foreach (string cacheFile in GetCacheFiles(cacheDirPath))
                        File.Delete(cacheFile);
                else
                    foreach (string cacheFile in GetCacheFiles(cacheDirPath))
                    {
                        using var storage = new FileStorage(cacheFile, FileStorage.Mode.Read);

                        FileNode GetNode(string node) => storage[node] ?? throw new CacheLoadException(cacheFile, node);

                        KeyPoint[] keyPoints = GetNode("Keypoints").ReadKeyPoints();
                        Mat descriptors = GetNode("Descriptors").ReadMat();
                        int originWidth = GetNode("OriginWidth").ReadInt();
                        int originHeight = GetNode("OriginHeight").ReadInt();

                        _md5map.Add(Path.GetFileNameWithoutExtension(cacheFile),
                                    new MatFeature(keyPoints, descriptors, originWidth, originHeight));
                    }
            }

            public MatFeature? this[Mat mat]
            {
                get => GetCache(mat);
                set => DoCache(mat, value ?? throw new ArgumentNullException(nameof(value)));
            }

            public void Dispose()
            {
                foreach (MatFeature matFeature in _md5map.Values) matFeature.Dispose();
            }

            private static string[] GetCacheFiles(string cacheDirPath) => Directory.GetFiles(cacheDirPath, "*.json.gz");

            private static string GetMd5(byte[] data)
            {
                byte[] hash = MD5.HashData(data);

                var hashStr = new StringBuilder();
                foreach (byte @byte in hash) hashStr.Append(@byte.ToString("x2"));

                return hashStr.ToString();
            }

            private static string GetMd5(Mat mat)
            {
                long total = mat.Total();
                var data = new byte[total];
                Marshal.Copy(mat.Data, data, 0, (int) total);

                return GetMd5(data);
            }

            private MatFeature? GetCache(Mat mat)
            {
                _md5map.TryGetValue(GetMd5(mat), out MatFeature? result);
                return result;
            }

            private void DoCache(Mat mat, MatFeature feature)
            {
                string md5 = GetMd5(mat);

                _md5map.Add(md5, feature);

                using var storage =
                    new FileStorage(Path.Combine(_cacheDirPath, $"{md5}.json.gz"), FileStorage.Mode.Write);
                storage.Write("Keypoints", feature.KeyPoints);
                storage.Write("Descriptors", feature.Descriptors);
                storage.Write("OriginWidth", mat.Width);
                storage.Write("OriginHeight", mat.Height);

                File.WriteAllText(_serialFilePath, _coreAssemblySerial);
            }
        }
    }
}