using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;

namespace REVUnit.AutoArknights.CLI
{
    public partial class Config : ISettings
    {
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

            Remote = new RemoteConfigImpl(this);
            Intervals = new IntervalsConfigImpl(this);
        }

        public IConfiguration Inner { get; }

        public bool ForcedSuspend => Optional("ForcedSuspend", bool.Parse);

        public string Required(params string[] keys)
        {
            return keys.Select(key => Inner[key]).FirstOrDefault(s => s != null) ??
                   throw new Exception($"配置文件需填写 {keys[0]}");
        }

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

        public string? Optional(params string[] keys)
        {
            return keys.Select(key => Inner[key]).FirstOrDefault(s => s != null);
        }

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