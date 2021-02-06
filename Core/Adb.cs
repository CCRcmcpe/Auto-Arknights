using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenCvSharp;
using Polly;
using REVUnit.AutoArknights.Core.Properties;
using Serilog;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public class AdbException : Exception
    {
        public AdbException(string? message) : base(message) { }
    }

    public class Adb
    {
        private readonly ILogger _logger;

        public Adb(string executable, string targetSerial)
        {
            _logger = Log.ForContext<Adb>();

            Executable = executable;
            TargetSerial = targetSerial;

            RestartServer();
            Connect();
        }

        public string Executable { get; set; }
        public string TargetSerial { get; set; }

        private void StartServer()
        {
            _logger.Information(Resources.Adb_StartingServer);
            ExecuteCore("start-server", out _);

            var job = new ProcessTerminator();
            job.Track(Process.GetProcessesByName("adb").ElementAtOrDefault(0) ??
                      throw new AdbException(Resources.Adb_Exception_StartServer));
        }

        public Size GetResolution()
        {
            Connect();
            string result = ExecuteOut("shell wm size");
            Match match = Regex.Match(result, @"Physical size: (\d+)x(\d+)");
            if (!match.Success) throw new AdbException(Resources.Adb_Exception_GetResolution);

            return new Size(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
        }

        public void Click(Point point)
        {
            Connect();
            Execute($"shell input tap {point.X} {point.Y}");
        }

        public Mat GetScreenshot()
        {
            const int retryCount = 3;
            return Policy.HandleResult<Mat>(mat => mat.Empty())
                         .Retry(retryCount,
                                (_, i) => Log.Warning(
                                    string.Format(Resources.Adb_Exception_GetScreenshot, i, retryCount)))
                         .Execute(() => Cv2.ImDecode(ExecuteOutBytes("exec-out screencap -p", 5 * 1024 * 1024),
                                                     ImreadModes.Color)) ??
                   throw new AdbException(Resources.Adb_Exception_GetScreenshotFailed);
        }

        public void Execute(string parameter)
        {
            ExecuteCore(parameter, out string stdErr);
            if (!string.IsNullOrWhiteSpace(stdErr))
                throw new AdbException(string.Format(Resources.Adb_Exception_Execute, stdErr));
        }

        public string ExecuteOut(string parameter, int bufferSize = 1024) =>
            Encoding.UTF8.GetString(ExecuteOutBytes(parameter, bufferSize));

        public byte[] ExecuteOutBytes(string parameter, int bufferSize)
        {
            _logger.Verbose(Resources.Adb_ExecutingCommand, parameter);

            Connect();
            ExecuteCore(parameter, out byte[] stdOutBytes, bufferSize, out string stdErr);
            if (!string.IsNullOrWhiteSpace(stdErr))
                throw new AdbException(string.Format(Resources.Adb_Exception_Execute, stdErr));

            return stdOutBytes;
        }

        private bool GetIfDeviceOnline()
        {
            ExecuteCore("get-state", out string state, out _);
            _logger.Debug(Resources.Adb_DeviceState, state);
            return state == "device";
        }

        private static void KillServer()
        {
            foreach (Process process in Process.GetProcessesByName("adb")) process.Kill();
        }

        private void Connect()
        {
            if (GetIfDeviceOnline()) return;
            const int retryCount = 3;
            bool succeed = Policy.HandleResult<bool>(b => !b).Retry(retryCount, (_, i) =>
            {
                _logger.Error(string.Format(Resources.Adb_Exception_Connect, i, retryCount));
                RestartServer();
            }).Execute(() =>
            {
                _logger.Debug(Resources.Adb_Connecting, TargetSerial);
                ExecuteCore($"connect {TargetSerial}", out _);
                return GetIfDeviceOnline();
            });
            if (!succeed) throw new AdbException(Resources.Adb_Exception_ConnectFailed);

            _logger.Debug(Resources.Adb_Connected);
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