using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly AsyncRetryPolicy<Mat> _getScreenshotPolicy = Policy.HandleResult<Mat>(mat => mat.Empty())
            .RetryAsync(GetScreenshotRetryTimes,
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

        public async Task<Size> GetResolution()
        {
            await ThrowIfServiceUnavailable();
            string result = Encoding.UTF8.GetString((await Execute("shell wm size")).stdOut);
            Match match = Regex.Match(result, @"Physical size: (\d+)x(\d+)");
            if (!match.Success) throw new Exception(Resources.Adb_Exception_GetResolution);

            int a = int.Parse(match.Groups[1].Value);
            int b = int.Parse(match.Groups[2].Value);
            return new Size(a > b ? a : b, a > b ? b : a);
        }

        public async Task Back()
        {
            await KeyEvent("KEYCODE_BACK");
        }

        public async Task Click(Point point)
        {
            await ThrowIfServiceUnavailable();
            await Execute($"shell input tap {point.X} {point.Y}");
        }

        public Task<Mat> GetScreenshot()
        {
            return Task.Run(() =>
            {
                return _getScreenshotPolicy.ExecuteAsync(async () =>
                    Cv2.ImDecode((await Execute("exec-out screencap -p", 5 * 1024 * 1024, 0)).stdOut,
                        ImreadModes.Color));
            });
        }

        public async Task Connect(string targetSerial)
        {
            Log.Information(Resources.Adb_Connecting, targetSerial);

            KillConnectedProcesses(targetSerial);

            if (!_serverStarted)
            {
                StartServer();
            }

            await Execute($"connect {targetSerial}", targeted: false);
            if (!await GetDeviceOnline())
            {
                throw new Exception(Resources.Adb_Exception_ConnectFailed);
            }

            TargetSerial = targetSerial;

            Log.Information(Resources.Adb_Connected);
        }

        // https://developer.android.com/reference/android/view/KeyEvent
        public async Task KeyEvent(string keyCodeOrName)
        {
            await ThrowIfServiceUnavailable();
            await Execute($"shell input keyevent {keyCodeOrName}");
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

        private async Task<(byte[] stdOut, byte[] stdErr)> Execute(string arguments,
            int stdOutBufferSize = DefaultBufferSize,
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
                stdOut = await ReadToEnd(process.StandardOutput.BaseStream, stdOutBufferSize);

            if (stdErrBufferSize > 0)
                stdErr = await ReadToEnd(process.StandardError.BaseStream, stdErrBufferSize);

            if (waitForExit)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await process.WaitForExitAsync(cts.Token);
            }

            return (stdOut!, stdErr!);
        }

        private async Task<bool> GetDeviceOnline()
        {
            string state = Encoding.UTF8.GetString((await Execute("get-state", targeted: false)).stdOut).Trim();
            Log.Debug(Resources.Adb_DeviceState, state);
            return state == "device";
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
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
                if (split.Length != 5 || split[2] != targetSerial)
                {
                    return;
                }

                int pid = int.Parse(split[4]);
                if (pid == 0)
                {
                    return;
                }

                var process = Process.GetProcessById(pid);
                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    Log.Debug(e, "尝试杀死PID为{PID}的程序时出错", pid);
                }
            };
            netstat.Start();
            netstat.BeginOutputReadLine();
            netstat.WaitForExit(1000);
        }

        private static async Task<byte[]> ReadToEnd(Stream stream, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];

            int read;
            var totalLen = 0;
            while ((read = await stream.ReadAsync(buffer, totalLen, bufferSize - totalLen)) > 0) totalLen += read;

            Array.Resize(ref buffer, totalLen);
            return buffer;
        }

        private async Task ThrowIfServiceUnavailable()
        {
            if (!_serverStarted || !await GetDeviceOnline())
            {
                throw new Exception(); // TODO Message
            }
        }
    }
}