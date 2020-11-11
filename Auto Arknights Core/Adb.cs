using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenCvSharp;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public class AdbException : Exception
    {
        public AdbException(string? message) : base(message) { }
    }

    public sealed class Adb
    {
        private static readonly string[] FailSigns =
        {
            "cannot connect", "no device", "no emulators", "device unauthorized", "device still", "device offline"
        };

        public Adb(string executable) => Executable = executable;

        public string? Target { get; set; }
        public string Executable { get; set; }

        public void Connect(string target)
        {
            Target = target;
            ExecuteCore($"connect {target}");
        }

        public void Click(Point point)
        {
            ExecuteCore($"shell input tap {point.X} {point.Y}");
        }

        public Mat GetScreenshot()
        {
            Mat result = Cv2.ImDecode(ExecuteCore("exec-out screencap -p", 5 * 1024 * 1024), ImreadModes.Color);
            if (result.Empty()) throw new AdbException("未接收到有效数据");

            return result;
        }

        public string Execute(string parameter) => Encoding.UTF8.GetString(ExecuteCore(parameter));

        private byte[] ExecuteCore(string parameter, int bufferSize = 1024)
        {
            Log.That(parameter, Log.Level.Debug, "ADB");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, $"-s {Target} {parameter}")
                {
                    CreateNoWindow = true, RedirectStandardOutput = true
                }
            };

            process.Start();

            byte[] buffer = new byte[bufferSize];
            Stream stdOut = process.StandardOutput.BaseStream;

            int read;
            var totalLen = 0;
            while ((read = stdOut.Read(buffer, totalLen, bufferSize - totalLen)) > 0) totalLen += read;

            Array.Resize(ref buffer, totalLen);

            // 检查ADB连接状态, 但是不在收到截图之类较大的数据的时候检查
            if (totalLen > 200) return buffer;
            string result = Encoding.UTF8.GetString(buffer);
            if (FailSigns.Any(failSign => result.Contains(failSign))) throw new AdbException("不能在未连接的情况下使用ADB");
            return buffer;
        }
    }
}