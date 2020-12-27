using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public abstract class PostAction : IArkTask
    {
        public abstract ExecuteResult Execute();

        public PostAction Parse(char c, ParseSettings parseSettings)
        {
            return c switch
            {
                'c' => new Shutdown(),
                'r' => new Reboot(),
                's' => new Suspend(false) { Forced = parseSettings.ForcedSuspend },
                'h' => new Suspend(true) { Forced = parseSettings.ForcedSuspend },
                'e' => new ExecuteCommand(parseSettings.Command ?? throw new ArgumentNullException(nameof(
                                                  parseSettings.Command),
                                              "需要有效的 Remote:ShutdownCommand 才能执行\"执行命令\"后续操作")),
                _ => throw new FormatException($"无效的后续操作标识符 \"{c}\"")
            };
        }

        public class ParseSettings
        {
            public bool ForcedSuspend { get; init; }
            public string? Command { get; init; }
        }
    }

    public class Shutdown : PostAction
    {
        public override ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/p");
            return ExecuteResult.Success();
        }

        public override string ToString() => "关机";
    }

    public class Reboot : PostAction
    {
        public override ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/r /t 0");
            return ExecuteResult.Success();
        }

        public override string ToString() => "重启";
    }

    public class Suspend : PostAction
    {
        public Suspend(bool hibernate)
        {
            if (hibernate && !IsPwrHibernateAllowed()) throw new NotSupportedException("系统未开启或不支持休眠");
            Hibernate = hibernate;
        }

        public bool Hibernate { get; set; }
        public bool Forced { get; set; }

        public override ExecuteResult Execute()
        {
            SetSuspendState(Hibernate, Forced, Forced);
            return ExecuteResult.Success();
        }

        [DllImport("powrprof.dll")]
        private static extern bool IsPwrHibernateAllowed();

        [DllImport("powrprof.dll")]
        private static extern uint SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        public override string ToString()
        {
            var b = new StringBuilder();
            if (Forced) b.Append("强制");

            b.Append(!Hibernate ? "睡眠" : "休眠");
            return b.ToString();
        }
    }

    public class ExecuteCommand : PostAction
    {
        public ExecuteCommand(string command) => Command = command;
        public string Command { get; set; }
        public int Timeout { get; set; } = 2000;

        public override ExecuteResult Execute()
        {
            Process? process =
                Process.Start(new ProcessStartInfo("cmd.exe", "/c " + Command) { CreateNoWindow = true });
            if (process == null) return new ExecuteResult(false, "无法启动cmd");

            return process.WaitForExit(Timeout) ? new ExecuteResult(false, "命令超时") : new ExecuteResult(true, "命令已执行");
        }

        public override string ToString() => $"执行命令：{Command[..Command.IndexOf(' ')]}...";
    }
}