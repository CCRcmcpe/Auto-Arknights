using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OpenCvSharp;

namespace REVUnit.AutoArknights.Core.CV
{
    public class CacheLoadException : Exception
    {
        public CacheLoadException(string cacheFilePath, string node, Exception? innerException = null) :
            base($"Error loading node {node} in cache file {cacheFilePath}", innerException)
        {
        }
    }

    public partial class FeatureDetector
    {
        private class Cache : IDisposable
        {
            private readonly string _cacheDirPath;
            private readonly Dictionary<string, MatFeature> _dict = new();

            public Cache(string cacheDirPath)
            {
                _cacheDirPath = cacheDirPath;
                Directory.CreateDirectory(cacheDirPath);

                foreach (string cacheFile in Directory.EnumerateFiles(cacheDirPath, "*.json.gz"))
                {
                    using var storage = new FileStorage(cacheFile, FileStorage.Modes.Read);

                    T Read<T>(string node, Func<FileNode, T> reader)
                    {
                        FileNode fileNode = storage![node] ?? throw new CacheLoadException(cacheFile, node);
                        try
                        {
                            return reader(fileNode);
                        }
                        catch (Exception e)
                        {
                            throw new CacheLoadException(cacheDirPath, node, e);
                        }
                    }

                    KeyPoint[] keyPoints = Read("Keypoints", x => x.ReadKeyPoints());
                    Mat descriptors = Read("Descriptors", x => x.ReadMat());
                    int originWidth = Read("MatWidth", x => x.ReadInt());
                    int originHeight = Read("MatHeight", x => x.ReadInt());
                    Feature2DType type = Read("Type", x => Enum.Parse<Feature2DType>(x.ReadString()));

                    _dict.Add(Path.GetFileNameWithoutExtension(cacheFile),
                        new MatFeature(keyPoints, descriptors, originWidth, originHeight, type));
                }
            }

            public MatFeature? this[Mat mat]
            {
                get => Get(mat);
                set => Add(mat, value ?? throw new ArgumentNullException(nameof(value)));
            }

            public void Dispose()
            {
                foreach (MatFeature matFeature in _dict.Values) matFeature.Dispose();
            }

            private void Add(Mat mat, MatFeature feature)
            {
                string md5 = GetMd5(mat);

                _dict.Add(md5, feature);

                using var storage =
                    new FileStorage(Path.Combine(_cacheDirPath, $"{md5}.json.gz"), FileStorage.Modes.Write);
                storage.Write("Keypoints", feature.KeyPoints);
                storage.Write("Descriptors", feature.Descriptors);
                storage.Write("OriginWidth", mat.Width);
                storage.Write("OriginHeight", mat.Height);
                storage.Write("Type", feature.Type.ToString());
            }

            private MatFeature? Get(Mat mat)
            {
                _dict.TryGetValue(GetMd5(mat), out MatFeature? result);
                return result;
            }

            private static unsafe string GetMd5(Mat mat)
            {
                long total = mat.Total();
                var data = new Span<byte>(mat.Data.ToPointer(), (int) total);

                return GetMd5(data);
            }

            private static string GetMd5(Span<byte> data)
            {
                byte[] hash = MD5.HashData(data);

                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}