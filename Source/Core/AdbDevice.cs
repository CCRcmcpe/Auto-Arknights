using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using OpenCvSharp;
using Polly;
using Polly.Retry;
using REVUnit.AutoArknights.Core.Properties;
using Serilog;

namespace REVUnit.AutoArknights.Core
{
    public class AdbDevice : IDevice
    {
        private const int DefaultBufferSize = 1024;

        private const int GetScreenshotRetryTimes = 3;

        private readonly RetryPolicy<Mat> _getScreenshotPolicy = Policy.HandleResult<Mat>(mat => mat.Empty())
            .Retry(GetScreenshotRetryTimes,
                (_, i) => Log.Warning(string.Format(Resources.Adb_Exception_GetScreenshot, i,
                    GetScreenshotRetryTimes)));

        private int _serverPort;
        private bool _serverStarted;

        public AdbDevice(string executable)
        {
            Executable = executable;
        }

        public string Executable { get; set; }
        public string? TargetSerial { get; set; }

        public Size GetResolution()
        {
            ThrowIfServiceUnavailable();
            string result = Encoding.UTF8.GetString(Execute("shell wm size").stdOut);
            Match match = Regex.Match(result, @"Physical size: (\d+)x(\d+)");
            if (!match.Success) throw new Exception(Resources.Adb_Exception_GetResolution);

            int a = int.Parse(match.Groups[1].Value);
            int b = int.Parse(match.Groups[2].Value);
            return new Size(a > b ? a : b, a > b ? b : a);
        }

        public void Back()
        {
            KeyEvent("KEYCODE_BACK");
        }

        public void Click(Point point)
        {
            ThrowIfServiceUnavailable();
            Execute($"shell input tap {point.X} {point.Y}");
        }

        public Mat GetScreenshot()
        {
            return _getScreenshotPolicy.Execute(() =>
                       Cv2.ImDecode(Execute("exec-out screencap -p", 5 * 1024 * 1024, 0).stdOut, ImreadModes.Color)) ??
                   throw new Exception(Resources.Adb_Exception_GetScreenshotFailed);
        }

        // https://developer.android.com/reference/android/view/KeyEvent
        public void KeyEvent(string keyCodeOrName)
        {
            ThrowIfServiceUnavailable();
            Execute($"shell input keyevent {keyCodeOrName}");
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void ThrowIfServiceUnavailable()
        {
            if (!_serverStarted || !GetDeviceOnline())
            {
                throw new Exception(); // TODO Message
            }
        }

        private bool GetDeviceOnline()
        {
            string state = Encoding.UTF8.GetString(Execute("get-state", targeted: false).stdOut).Trim();
            Log.Debug(Resources.Adb_DeviceState, state);
            return state == "device";
        }

        public void StartServer()
        {
            Log.Information(Resources.Adb_StartingServer);

            int port = GetFreeTcpPort();

            var adbServerProcess = new Process
            {
                StartInfo =
                    new ProcessStartInfo(Executable, $"nodaemon server -P {port}") {CreateNoWindow = true},
                EnableRaisingEvents = true
            };

            if (!adbServerProcess.Start())
            {
                throw new Exception(Resources.Adb_Exception_StartServer);
            }

            _serverPort = port;
            adbServerProcess.Exited += (_, _) => throw new Exception("ADB 服务器进程意外停止");
            ChildProcessTracker.Track(adbServerProcess);

            Log.Information("已启动 ADB 服务器，端口：{Port}", _serverPort);

            _serverStarted = true;
        }

        [MemberNotNull(nameof(TargetSerial))]
        public void Connect(string targetSerial)
        {
            Log.Information(Resources.Adb_Connecting, targetSerial);

            KillConnectedProcesses(targetSerial);

            if (!_serverStarted)
            {
                StartServer();
            }

            Execute($"connect {targetSerial}", targeted: false);
            if (!GetDeviceOnline())
            {
                throw new Exception(Resources.Adb_Exception_ConnectFailed);
            }

            TargetSerial = targetSerial;

            Log.Information(Resources.Adb_Connected);
        }

        private static void KillConnectedProcesses(string targetSerial)
        {
            var netstat = new Process
            {
                StartInfo = new ProcessStartInfo("netstat", "-no")
                    {CreateNoWindow = true, RedirectStandardOutput = true}
            };
            netstat.OutputDataReceived += (_, args) =>
            {
                string? line = args.Data;
                if (string.IsNullOrEmpty(line))
                {
                    return;
                }

                string[] split = Regex.Split(line, @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (split.Length != 5)
                {
                    return;
                }

                if (split[2] == targetSerial)
                {
                    var process = Process.GetProcessById(int.Parse(split[4]));
                    process.Kill();
                }
            };
            netstat.Start();
            netstat.BeginOutputReadLine();
            netstat.WaitForExit(1000);
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

        private (byte[] stdOut, byte[] stdErr) Execute(string arguments, int stdOutBufferSize = DefaultBufferSize,
            int stdErrBufferSize = DefaultBufferSize,
            bool waitForExit = false, bool targeted = true)
        {
            var sb = new StringBuilder();
            sb.Append($"-P {_serverPort} ");
            if (targeted)
            {
                sb.Append($"-s {TargetSerial} ");
            }

            sb.Append(arguments);

            arguments = sb.ToString();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, arguments)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            process.Start();
            ChildProcessTracker.Track(process);

            byte[]? stdOut = null;
            byte[]? stdErr = null;

            if (stdOutBufferSize > 0)
                stdOut = ReadToEnd(process.StandardOutput.BaseStream, stdOutBufferSize);

            if (stdErrBufferSize > 0)
                stdErr = ReadToEnd(process.StandardError.BaseStream, stdErrBufferSize);

            if (waitForExit)
            {
                process.WaitForExit();
            }

            return (stdOut!, stdErr!);
        }
    }
}