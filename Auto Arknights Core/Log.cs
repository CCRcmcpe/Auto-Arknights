using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Console = Colorful.Console;

namespace REVUnit.AutoArknights.Core
{
    public static class Log
    {
        public static Level LogLevel { get; set; } = Level.Debug;

        public static void Debug(string? message, string? prefix = null, bool withTime = true)
        {
            That(message, Level.Debug, prefix, withTime);
        }

        public static void Error(string? message, string? prefix = null, bool withTime = true)
        {
            That(message, Level.Error, prefix, withTime);
        }

        public static void Info(string? message, string? prefix = null, bool withTime = true)
        {
            That(message, Level.Info, prefix, withTime);
        }

        public static void That(string? message, Level? level = null, string? prefix = null, bool withTime = true)
        {
            if (level != null && level.ImportanceLevel < LogLevel.ImportanceLevel) return;

            if (withTime) Console.Write($"[{DateTime.Now:HH:mm:ss}]", Color.Gray);

            if (prefix != null) Console.Write($"[{prefix}]");

            if (level != null)
            {
                if (level.LevelColor.HasValue)
                    Console.Write($"[{level}]: ", level.LevelColor.Value);
                else
                    Console.Write($"[{level}]: ");

                if (level.MessageColor.HasValue)
                    Console.WriteLine(message, level.MessageColor.Value);
                else
                    Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        public static void Warning(string message, string? prefix = null, bool withTime = false)
        {
            That(message, Level.Warning, prefix, withTime);
        }

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        public class Level
        {
            public static readonly Level None = new() { ImportanceLevel = int.MinValue };

            public static readonly Level Debug = new()
            {
                ImportanceLevel = -1, Name = "Debug", LevelColor = Color.Gray, MessageColor = Color.Gray
            };

            public static readonly Level Info = new() { ImportanceLevel = 0, Name = "Info" };

            public static readonly Level Warning = new()
            {
                ImportanceLevel = 1, Name = "Warn", LevelColor = Color.Yellow
            };

            public static readonly Level Error = new() { ImportanceLevel = 2, Name = "Error", LevelColor = Color.Red };

            public int ImportanceLevel { get; init; }
            public string? Name { get; init; }
            public Color? LevelColor { get; init; }
            public Color? MessageColor { get; init; }

            public static Level Get(string? name)
            {
                return name?.ToLower() switch
                {
                    "debug"   => Debug,
                    "info"    => Info,
                    "warning" => Warning,
                    "error"   => Error,
                    _         => throw new ArgumentOutOfRangeException(nameof(name), name, "不存在此 Level 名")
                };
            }

            public override string ToString() => Name ?? string.Empty;
        }
    }
}