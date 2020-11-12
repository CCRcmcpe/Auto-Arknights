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
        public Adb(string executable, string targetSerial)
        {
            Executable = executable;
            TargetSerial = targetSerial;

            if (Process.GetProcessesByName("adb").Any()) return;
            ExecuteCore("start-server", out _);

            var job = new Job();
            job.AddProcess(Process.GetProcessesByName("adb")[0].Handle);
        }

        public string Executable { get; set; }
        public string TargetSerial { get; set; }

        public void Click(Point point)
        {
            Execute($"shell input tap {point.X} {point.Y}");
        }

        public Mat GetScreenshot()
        {
            Mat result = Cv2.ImDecode(ExecuteOutBytes("exec-out screencap -p", 5 * 1024 * 1024), ImreadModes.Color);
            if (result.Empty()) throw new AdbException("未接收到有效数据");

            return result;
        }

        public void Execute(string parameter)
        {
            ExecuteCore(parameter, out string stdErr);
            if (!string.IsNullOrWhiteSpace(stdErr)) throw new AdbException($"ADB错误 StdErr: {stdErr}");
        }

        public string ExecuteOut(string parameter, int bufferSize) =>
            Encoding.UTF8.GetString(ExecuteOutBytes(parameter, bufferSize));

        public byte[] ExecuteOutBytes(string parameter, int bufferSize)
        {
            Log.That(parameter, Log.Level.Debug, "ADB");

            EnsureConnected();
            ExecuteCore(parameter, out byte[] stdOutBytes, bufferSize, out string stdErr);
            if (!string.IsNullOrWhiteSpace(stdErr)) throw new AdbException($"ADB错误 StdErr: {stdErr}");

            return stdOutBytes;
        }

        private void EnsureConnected()
        {
            for (var timesTried = 0; timesTried < 2; timesTried++)
            {
                ExecuteCore("get-state", out string state, out _);
                if (state == "device") return;

                if (timesTried == 1) ExecuteCore("kill-server", out _);
                Log.That($"正在连接到 {TargetSerial}", Log.Level.Debug, "ADB");
                ExecuteCore($"connect {TargetSerial}", out _);
            }

            throw new AdbException($"无法连接到 {TargetSerial}");
        }

        private static byte[] ReadToEnd(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];

            int read;
            var totalLen = 0;
            while ((read = stream.Read(buffer, totalLen, bufferSize - totalLen)) > 0) totalLen += read;

            Array.Resize(ref buffer, totalLen);
            return buffer;
        }

        private void ExecuteCore(string parameter, out string stdErr)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, parameter)
                {
                    CreateNoWindow = true, RedirectStandardError = true
                }
            };

            process.Start();

            stdErr = process.StandardError.ReadToEnd().Trim();
        }

        private void ExecuteCore(string parameter, out string stdOut, out string stdErr)
        {
            ExecuteCore(parameter, out byte[] stdOutBytes, 1024, out stdErr);
            stdOut = Encoding.UTF8.GetString(stdOutBytes).Trim();
        }

        private void ExecuteCore(string parameter, out byte[] stdOut, int stdOutBufferSize, out string stdErr)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, parameter)
                {
                    CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
                }
            };

            process.Start();

            stdOut = ReadToEnd(process.StandardOutput.BaseStream, stdOutBufferSize);
            stdErr = process.StandardError.ReadToEnd().Trim();
        }
    }
}