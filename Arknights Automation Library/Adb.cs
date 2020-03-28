using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using OpenCvSharp;
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

        public bool Connect(string target)
        {
            Target = target;
            return Connect();
        }

        public bool Connect()
        {
            string result = Exec($"connect {Target}", false);
            return !result.Contains("cannot connect");
        }

        public void Click(Point point)
        {
            Exec($"shell input tap {point.X} {point.Y}", false);
        }

        public string Exec(string parameter, bool muteOut = true)
        {
            using MemoryStream stream = ExecBin(parameter, muteOut);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private MemoryStream ExecBin(string parameter, bool muteOut = true)
        {
            if (!muteOut) Console.WriteLine(parameter);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, parameter)
                {
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
                    throw new Exception("Cannot connect to remote");

            return ms;
        }

        public Mat GetScreenShot()
        {
            using MemoryStream stream = ExecBin("exec-out screencap -p");
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