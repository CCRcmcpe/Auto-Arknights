using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.CLI.Properties;
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
                throw new FormatException(Resources.Config_Exception_InvalidConfig, e);
            }

            Remote = new RemoteConfigImpl(this);
            Intervals = new IntervalsConfigImpl(this);
        }

        public IConfiguration Inner { get; }

        public string Required(params string[] keys)
        {
            return keys.Select(key => Inner[key]).FirstOrDefault(s => s != null) ??
                   throw new Exception(string.Format(Resources.Config_Exception_RequirementsUnmet, keys[0]));
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
                    throw new Exception(string.Format(Resources.Config_Exception_InvalidKey, key));
            }
            else
            {
                try
                {
                    value = parser(raw);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format(Resources.Config_Exception_CannotParse, key), e);
                }
            }

            if (validator != null && !validator(value))
                throw new Exception(string.Format(Resources.Config_Exception_InvalidKey, key));

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
                if (!raw.TryToType(out value))
                    throw new Exception(string.Format(Resources.Config_Exception_InvalidKey, key));
            }
            else
            {
                try
                {
                    value = parser(raw);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format(Resources.Config_Exception_CannotParse, key), e);
                }
            }

            if (validator != null && !validator(value))
                throw new Exception(string.Format(Resources.Config_Exception_InvalidKey, key));

            return value;
        }
    }
}