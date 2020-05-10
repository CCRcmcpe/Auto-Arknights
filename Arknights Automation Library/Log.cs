using System;
using System.Drawing;
using Console = Colorful.Console;

namespace REVUnit.AutoArknights.Core
{
    public static class Log
    {
        private static void WriteTime()
        {
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ", Color.Gray);
        }

        public static void Info(string message, bool withTime = false)
        {
            if (withTime) WriteTime();
            Console.Write("[Info]: ", Color.Gray);
            Console.WriteLine(message);
        }

        public static void InfoAlt(string message, bool withTime = false)
        {
            if (withTime) WriteTime();
            Console.Write("[Info]: ", Color.Yellow);
            Console.WriteLine(message);
        }

        public static void Warning(string message, bool withTime = false)
        {
            if (withTime) WriteTime();
            Console.Write("[Warn]: ", Color.Yellow);
            Console.WriteLine(message);
        }

        public static void Error(string message, bool withTime = false)
        {
            if (withTime) WriteTime();
            Console.Write("[ERROR]: ", Color.Red);
            Console.WriteLine(message);
        }
    }
}