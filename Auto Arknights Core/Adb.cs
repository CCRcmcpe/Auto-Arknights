using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenCvSharp;
using Polly;
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

            RestartServer();
            Connect();
        }

        public string Executable { get; set; }
        public string TargetSerial { get; set; }

        private void StartServer()
        {
            Log.That("正在启动服务器", Log.Level.Debug, "ADB");
            ExecuteCore("start-server", out _);

            var job = new ProcessTerminator();
            job.Track(Process.GetProcessesByName("adb").ElementAtOrDefault(0) ?? throw new AdbException("服务器未能正常启动"));
        }

        public Size GetResolution()
        {
            Connect();
            string result = ExecuteOut("shell wm size");
            Match match = Regex.Match(result, @"Physical size: (\d+)x(\d+)");
            if (!match.Success) throw new AdbException("无法获取远端分辨率信息");

            return new Size(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }

        public void Click(Point point)
        {
            Connect();
            Execute($"shell input tap {point.X} {point.Y}");
        }

        public Mat GetScreenshot()
        {
            Policy.HandleResult<Mat>(mat => mat.Empty()).Retry(3, (ret, i) => Log.Warning($"截图失败，正在重试，次数={i}")).ExecuteAndCapture(() => Cv2.ImDecode(ExecuteOutBytes("exec-out screencap -p", 5 * 1024 * 1024),
                                   ImreadModes.Color));
            Mat result = ;
            if (result.Empty()) throw new AdbException("未接收到有效数据");

            return result;
        }

        public void Execute(string parameter)
        {
            ExecuteCore(parameter, out string stdErr);
            if (!string.IsNullOrWhiteSpace(stdErr)) throw new AdbException($"ADB错误 StdErr: {stdErr}");
        }

        public string ExecuteOut(string parameter, int bufferSize = 1024) =>
            Encoding.UTF8.GetString(ExecuteOutBytes(parameter, bufferSize));

        public byte[] ExecuteOutBytes(string parameter, int bufferSize)
        {
            Log.That(parameter, Log.Level.Debug, "ADB");

            Connect();
            ExecuteCore(parameter, out byte[] stdOutBytes, bufferSize, out string stdErr);
            if (!string.IsNullOrWhiteSpace(stdErr)) throw new AdbException($"ADB错误 StdErr: {stdErr}");

            return stdOutBytes;
        }

        private bool GetIfDeviceOnline()
        {
            ExecuteCore("get-state", out string state, out _);
            return state == "device";
        }

        private static void KillServer()
        {
            foreach (Process process in Process.GetProcessesByName("adb")) process.Kill();
        }

        private void Connect()
        {
            var tryedTimes = 0;
            while (!GetIfDeviceOnline())
            {
                switch (tryedTimes)
                {
                    case 1:
                        RestartServer();
                        break;
                    case 2: throw new AdbException($"无法连接到 {TargetSerial}");
                }

                Log.That($"正在连接到 {TargetSerial}", Log.Level.Debug, "ADB");
                ExecuteCore($"connect {TargetSerial}", out _);
                tryedTimes++;
            }
        }

        private void RestartServer()
        {
            KillServer();
            StartServer();
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