using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenCvSharp;
using REVUnit.Crlib.Extension;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public class Adb : IDisposable
    {
        public Adb(string executable)
        {
            Executable = executable;
        }

        public string Target { get; set; }
        public string Executable { get; set; }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private static void Out(string message)
        {
            Log.That(message, Log.Level.Debug, "ADB");
        }

        public bool Connect(string target)
        {
            Target = target;
            return Connect();
        }

        public bool Connect()
        {
            try
            { 
                Exec($"connect {Target}", false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Error(string message)
        {
            Log.Error(message, "ADB");
            Exec($"disconnect {Target}");
            XConsole.AnyKey();
        }

        public void Click(Point point)
        {
            Exec($"-s {Target} shell input tap {point.X} {point.Y}", false);
        }

        public string Exec(string parameter, bool muteOut = true)
        {
            return Encoding.UTF8.GetString(ExecBin(parameter, muteOut));
        }

        private byte[] ExecBin(string parameter, bool muteOut = true)
        {
            if (!muteOut) Out(parameter);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, parameter)
                {
                    StandardOutputEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            using var ms = new MemoryStream();
            Stream stdout = process.StandardOutput.BaseStream;
            int val;
            while ((val = stdout.ReadByte()) != -1) ms.WriteByte((byte) val);

            byte[] bytes = ms.ToArray();
            if (bytes.Length < 200)
            {
                string result = Encoding.UTF8.GetString(bytes).Trim();
                if (result.Contains("cannot connect")||result.Contains("no device") || result.Contains("no emulators") ||
                    result.Contains("device unauthorized") || result.Contains("device still") ||
                    result.Contains("device offline"))
                {
                    ReleaseUnmanagedResources();
                    throw new Exception("无法连接到目标ADB");
                }
            }

            return bytes;
        }

        public Mat GetScreenShot()
        {
            byte[] bytes = ExecBin($"-s {Target} exec-out screencap -p");
            var screenshot = Mat.ImDecode(bytes);
            if (screenshot.Empty())
            {
                throw new Exception("接收到了一个空截图");
            }

            return screenshot;
        }

        ~Adb()
        {
            ReleaseUnmanagedResources();
        }

        private static void ReleaseUnmanagedResources()
        {
            foreach (Process process in Process.GetProcessesByName("adb"))
            {
                process.Kill();
            }
        }
    }
}