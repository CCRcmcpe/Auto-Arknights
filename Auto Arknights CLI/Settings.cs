using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;

namespace REVUnit.AutoArknights.CLI
{
    public class Config
    {
        private readonly IConfiguration _config;

        public Config(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath)) File.Create(jsonFilePath);

            try
            {
                _config = new ConfigurationBuilder().AddJsonFile(jsonFilePath).Build();
            }
            catch (FormatException e)
            {
                throw new FormatException("配置文件无效，请检查语法", e);
            }
        }

        public bool ForcedSuspend => Optional("ForcedSuspend", bool.Parse, false);
        public int? LevelCompleteSleepTime => Optional<int?>("LevelCompleteSleepTime", s => int.Parse(s), null);
        public Log.Level Log_Level => Optional("Log:Level", Log.Level.Get, Log.Level.Info);
        public string Remote_AdbExecutable => Required("Remote:AdbExecutable");
        public string Remote_Address => Required("Remote:Address");
        public string? Remote_ShutdownCommand => Optional("Remote:ShutdownCommand")?.Trim();

        public string Required(string key) => _config[key] ?? throw new Exception($"配置文件需填写 {key}");

        public T Required<T>(string key, Func<string?, T> parser)
        {
            string value = Required(key);

            try
            {
                return parser(value);
            }
            catch (Exception e)
            {
                throw new Exception($"配置文件中 {key} 无效", e);
            }
        }

        public string? Optional(string key) => _config[key];

        public T Optional<T>(string key, Func<string, T> parser, T defaultValue)
        {
            string? value = Optional(key);
            if (value == null) return defaultValue;

            try
            {
                return parser(value);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}