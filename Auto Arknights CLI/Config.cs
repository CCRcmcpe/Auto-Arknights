using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using REVUnit.Crlib.Extension;

namespace REVUnit.AutoArknights.CLI
{
    public class Config
    {
        public IConfiguration Inner { get; }
        
        public Config(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath)) File.Create(jsonFilePath);

            try
            {
                Inner = new ConfigurationBuilder().AddJsonFile(jsonFilePath).Build();
            }
            catch (FormatException e)
            {
                throw new FormatException("配置文件无效，请检查语法", e);
            }
        }

        public bool ForcedSuspend => Optional("ForcedSuspend", bool.Parse);
        public int? LevelCompleteSleepTime => Optional<int?>("LevelCompleteSleepTime", s => int.Parse(s));
        public string Remote_AdbExecutable => Required("Remote:AdbExecutable");
        public string Remote_Address => Required("Remote:Address");
        public string? Remote_ShutdownCommand => Optional("Remote:ShutdownCommand")?.Trim();

        public string Required(string key) => Inner[key] ?? throw new Exception($"配置文件需填写 {key}");

        public T Required<T>(string key, Func<string?, T>? parser = null, Predicate<T>? validator = null)
        {
            string raw = Required(key);
            T value;
            if (parser == null)
            {
                if (raw.TryToType(out T? result))
                    value = result!;
                else
                    throw new Exception($"无法解析 {key} 的值");
            }
            else
            {
                try
                {
                    value = parser(raw);
                }
                catch (Exception e)
                {
                    throw new Exception($"配置文件中 {key} 无效", e);
                }
            }

            if (validator != null && !validator(value)) throw new Exception($"配置文件中 {key} 的值无效");

            return value;
        }

        public string? Optional(string key) => Inner[key];

        public T? Optional<T>(string key, Func<string, T>? parser = null, T? defaultValue = default,
                              Predicate<T?>? validator = null)
        {
            string? raw = Optional(key);
            if (raw == null) return defaultValue;

            T? value;

            if (parser == null)
            {
                if (!raw.TryToType(out value)) throw new Exception($"无法解析 {key} 的值");
            }
            else
            {
                try
                {
                    value = parser(raw);
                }
                catch (Exception e)
                {
                    throw new Exception($"配置文件中 {key} 无效", e);
                }
            }

            if (validator != null && !validator(value)) throw new Exception($"配置文件中 {key} 的值无效");

            return value;
        }
    }
}