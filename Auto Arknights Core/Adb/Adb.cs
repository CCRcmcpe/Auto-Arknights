using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenCvSharp;
using Point = System.Drawing.Point;

namespace REVUnit.AutoArknights.Core
{
    public sealed class Adb
    {
        private static readonly string[] FailSigns =
        {
            "cannot connect", "no device", "no emulators",
            "device unauthorized", "device still", "device offline"
        };

        public Adb(string executable)
        {
            Executable = executable;
        }

        public string? Target { get; set; }
        public string Executable { get; set; }

        public void Click(Point point)
        {
            Execute($"shell input tap {point.X} {point.Y}");
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
                ExecuteCore($"connect {Target}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Execute(string parameter)
        {
            return Encoding.UTF8.GetString(ExecuteCore(parameter));
        }

        public Mat GetScreenShot()
        {
            byte[] bytes = ExecuteCore("exec-out screencap -p");
            Mat screenshot = Mat.ImDecode(bytes);
            if (screenshot.Empty()) throw new AdbException("Empty screenshot received from adb");

            return screenshot;
        }

        private byte[] ExecuteCore(string parameter)
        {
            Log.That(parameter, Log.Level.Debug, "ADB");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Executable, $"-s {Target} {parameter}")
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
                if (FailSigns.Contains(result))
                    throw new AdbException("Cannot connect to target device");
            }

            return bytes;
        }
    }
}