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
        private PostAction[]? _postActions;
        private int _repeatTime;

        public AutoArknightsCli()
        {
            if (!Library.CheckIfSupported())
                throw new NotSupportedException("你当前的CPU不支持AVX2指令集，无法运行本程序");

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
            var cin = new Cin {AutoTrim = true, IgnoreCase = true, ThrowOnUndefinedEnum = true};
            var parameters = cin.Get<string>(@"<\d: 模式>[\d+: 刷关次数][\w+: 后续操作]");
            ParseParameters(parameters);
            if (_postActions!.Contains(PostAction.ShutdownEmulator) && _shutdownCommand == null)
                throw new Exception("需要有效的 Remote:ShutdownCommand 才能执行关闭远端操作");
            if (_postActions!.Contains(PostAction.Hibernate) && !Native.IsPwrHibernateAllowed())
                throw new Exception("系统未开启/不支持休眠");

            using var levelRepeatTask = new Task(() =>
            {
                using Device device = new Device(_adbExecutable, _adbRemote);
                new LevelRepeater(device, _mode, _repeatTime).Execute();
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
                Log.Error($"出现异常：{innerException.GetType().Name}，异常信息：\"{innerException.Message}\"");
            }

            if (_postActions!.Any())
            {
                foreach (PostAction postAction in _postActions!) ExecutePostAction(postAction);
            }
            else
            {
                Console.Beep();
                XConsole.AnyKey("所有任务完成");
            }
        }

        private string? ConfigOptional(string key)
        {
            return _config![key];
        }

        private string ConfigRequired(string key, string? messageOnNil = null)
        {
            return _config![key] ?? throw new Exception(messageOnNil ?? $"配置文件需填写 {key}");
        }

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
                    Process.Start(new ProcessStartInfo("cmd.exe", "/c " + _shutdownCommand) {CreateNoWindow = true});
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void ParseParameters(string parameters)
        {
            int modeValue = int.Parse(parameters[..1]);

            _mode = (LevelRepeater.Mode) modeValue;
            if (!Enum.IsDefined(_mode)) throw new Exception("输入的模式超出范围");

            var parsedIndex = 1;
            if (_mode == LevelRepeater.Mode.SpecifiedTimes || _mode == LevelRepeater.Mode.SpecTimesWithWait)
            {
                var end = 0;
                while (end < parameters.Length - 1 && char.IsDigit(parameters, end)) end++;

                if (end <= 1) throw new Exception("在模式 SpecifiedTimes 或 SpecTimesWithWait 下，你应该输入一个有效的刷关次数值");

                _repeatTime = int.Parse(parameters[1..end]);
                if (end == parameters.Length) return;

                parsedIndex = end;
            }

            string postActions = parameters[parsedIndex..];
            _postActions = postActions.Select(c =>
            {
                PostAction postAction = default;
                if (char.IsLetter(c)) // Confirmed as post action spec
                {
                    postAction = (PostAction) (short) c;
                    if (!Enum.IsDefined(postAction)) throw new Exception("无效的后续操作");
                }

                return postAction;
            }).ToArray();
        }

        private enum PostAction : short
        {
            Shutdown = (short) 'c',
            Reboot = (short) 'r',
            Sleep = (short) 's',
            Hibernate = (short) 'h',
            ShutdownEmulator = (short) 'e'
        }
    }
}