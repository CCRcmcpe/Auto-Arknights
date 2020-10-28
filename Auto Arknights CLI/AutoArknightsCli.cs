using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;
using REVUnit.Crlib.Input;

namespace REVUnit.AutoArknights.CLI
{
    public sealed class AutoArknightsCli
    {
        private const string ConfigJson = "Auto Arknights CLI.config.json";
        private readonly string _adbExecutable;
        private readonly string _adbRemote;
        private readonly IConfiguration _config;
        private readonly bool _forcedSuspend;
        private readonly string? _shutdownCommand;

        private LevelRepeater.Mode _mode;
        private PostAction[] _postActions = Array.Empty<PostAction>();
        private int _repeatTimes;

        public AutoArknightsCli()
        {
            if (!Library.CheckIfSupported()) throw new NotSupportedException("你当前的CPU不支持AVX2指令集，无法运行本程序");

            if (!File.Exists(ConfigJson)) File.Create(ConfigJson);
            try
            {
                _config = new ConfigurationBuilder().AddJsonFile(ConfigJson).Build();
            }
            catch (FormatException e)
            {
                throw new FormatException("配置文件无效，请检查语法", e);
            }

            Log.LogLevel = Log.Level.Get(ConfigRequired("Log:Level")) ??
                           throw new Exception("配置文件中 Log:Level 的值无效");
            _adbExecutable = ConfigRequired("Remote:AdbExecutable");
            _adbRemote = ConfigRequired("Remote:Address");
            _shutdownCommand = ConfigOptional("Remote:ShutdownCommand")?.Trim();
            string? forcedSuspend = ConfigOptional("ForcedSuspend");
            if (forcedSuspend != null && !bool.TryParse(ConfigOptional("ForcedSuspend"), out _forcedSuspend))
                throw new Exception("配置文件中 ForcedSuspend 的值无效");
        }

        public void Run()
        {
            var cin = new Cin { AutoTrim = true, IgnoreCase = true, ThrowOnUndefinedEnum = true };
            cin.Get<string>(@"<\d: 模式>[\d+: 刷关次数][\w+: 后续操作]", s =>
            {
                ParseParameters(s);
                return null!;
            });

            if (_postActions.Length != 0)
            {
                if (_postActions.Contains(PostAction.ShutdownEmulator) && _shutdownCommand == null)
                    throw new Exception("需要有效的 Remote:ShutdownCommand 才能执行关闭远端操作");
                if (_postActions.Contains(PostAction.Hibernate) && !Native.IsPwrHibernateAllowed())
                    throw new Exception("系统未开启/不支持休眠");
            }

            using var levelRepeatTask = new Task(() =>
            {
                using Device device = new Device(_adbExecutable, _adbRemote);
                new LevelRepeater(device, _mode, _repeatTimes).Execute();
            }, TaskCreationOptions.LongRunning);
            levelRepeatTask.Start();

            // Discard further inputs
            do
            {
                if (Console.KeyAvailable) Console.ReadKey(true);
            } while (!levelRepeatTask.IsCompleted);

            if (levelRepeatTask.IsFaulted)
            {
                Exception innerException = levelRepeatTask.Exception!.InnerExceptions[0];
                throw innerException;
            }

            if (_postActions.Length != 0)
                foreach (PostAction postAction in _postActions)
                    ExecutePostAction(postAction);
            else
            {
                Console.Beep();
                XConsole.AnyKey("所有任务完成");
            }
        }

        private string? ConfigOptional(string key) => _config![key];

        private string ConfigRequired(string key, string? messageOnNil = null) =>
            _config![key] ?? throw new Exception(messageOnNil ?? $"配置文件需填写 {key}");

        private void ExecutePostAction(PostAction action)
        {
            switch (action)
            {
                case PostAction.Shutdown:
                    Process.Start("shutdown.exe", "/p");
                    break;
                case PostAction.Reboot:
                    Process.Start("shutdown.exe", "/r /t 0");
                    break;
                case PostAction.Sleep:
                    Native.SetSuspendState(false, _forcedSuspend, _forcedSuspend);
                    break;
                case PostAction.Hibernate:
                    Native.SetSuspendState(true, _forcedSuspend, _forcedSuspend);
                    break;
                case PostAction.ShutdownEmulator:
                    Process.Start(new ProcessStartInfo("cmd.exe", "/c " + _shutdownCommand) { CreateNoWindow = true });
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void ParseParameters(string parameters)
        {
            int modeValue = int.Parse(parameters[..1]);

            _mode = (LevelRepeater.Mode) modeValue;
            if (!Enum.IsDefined(_mode)) throw new Exception("输入的模式超出范围");

            var index = 1;
            if (_mode == LevelRepeater.Mode.SpecifiedTimes || _mode == LevelRepeater.Mode.SpecTimesWithWait)
            {
                while (index < parameters.Length && char.IsDigit(parameters, index)) index++;

                if (index == 1) throw new Exception("在模式 SpecifiedTimes 或 SpecTimesWithWait 下，你应该输入一个有效的刷关次数值");
                _repeatTimes = int.Parse(parameters[1..index]);
            }

            if (index == parameters.Length) return; // A mode value and maybe a repeat times number parsed

            string postActions = parameters[index..];
            _postActions = postActions.Select(c =>
            {
                if (!char.IsLetter(c)) throw new Exception($"无效的后续操作值 \"{c}\"");
                var postAction = (PostAction) char.ToLowerInvariant(c);
                if (!Enum.IsDefined(postAction)) throw new Exception("无效的后续操作");
                return postAction;
            }).ToArray();
        }

        private enum PostAction : ushort
        {
            Shutdown = 'c',
            Reboot = 'r',
            Sleep = 's',
            Hibernate = 'h',
            ShutdownEmulator = 'e'
        }
    }
}