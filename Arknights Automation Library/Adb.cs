using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            void Fail(string message)
            {
                Log.Error(message, "ADB");
                Exec($"disconnect {Target}");
                XConsole.AnyKey();
            }

            while (true)
            {
                string result = Exec($"connect {Target}", false);

                if (result.Contains("failed to authenticate"))
                {
                    Fail("授权失败，请点击允许调试");
                    continue;
                }


                if (Exec("devices").Split(Environment.NewLine).All(it => it != $"{Target}\tdevice"))
                {
                    Fail($"未能连接到{Target}，请检查设置");
                    return false;
                }

                return true;
            }
        }

        public void Click(Point point)
        {
            Exec($"-s {Target} shell input tap {point.X} {point.Y}", false);
        }

        public string Exec(string parameter, bool muteOut = true)
        {
            using MemoryStream stream = ExecBin(parameter, muteOut);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private MemoryStream ExecBin(string parameter, bool muteOut = true)
        {
            if (!muteOut) Out(parameter);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, parameter)
                {
                    StandardOutputEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                }
            };

            process.Start();
            var ms = new MemoryStream();
            Stream stdout = process.StandardOutput.BaseStream;
            int val;
            while ((val = stdout.ReadByte()) != -1) ms.WriteByte((byte) val);
            process.WaitForExit();

            /*
             * daemon not running; starting now at tcp:5037
             * daemon started successfully
             * error: no devices/emulators found
             *
             * Lengths 112 bytes in UTF-8
             */
            if (ms.Length < 200)
                if (Encoding.UTF8.GetString(ms.ToArray()).Contains("cannot connect"))
                    throw new Exception("未能连接到目标ADB");

            return ms;
        }

        public Mat GetScreenShot()
        {
            using MemoryStream stream = ExecBin($"-s {Target} exec-out screencap -p");
            stream.Position = 0;
            return Mat.FromStream(stream, ImreadModes.Color);
        }

        private void ReleaseUnmanagedResources()
        {
            Exec("kill-server");
        }

        ~Adb()
        {
            ReleaseUnmanagedResources();
        }
    }
}