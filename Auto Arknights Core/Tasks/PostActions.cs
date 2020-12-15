using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public class Shutdown : IArkTask
    {
        public ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/p");
            return ExecuteResult.Success();
        }

        public override string ToString() => "关机";
    }

    public class Reboot : IArkTask
    {
        public ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/r /t 0");
            return ExecuteResult.Success();
        }

        public override string ToString() => "重启";
    }

    public class Suspend : IArkTask
    {
        public Suspend(bool hibernate)
        {
            if (hibernate && !IsPwrHibernateAllowed()) throw new NotSupportedException("系统未开启或不支持休眠");
            Hibernate = hibernate;
        }

        public bool Hibernate { get; set; }
        public bool Forced { get; set; }

        public ExecuteResult Execute()
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
            if (Forced)
            {
                b.Append("强制");
            }

            b.Append(!Hibernate ? "睡眠" : "休眠");
            return b.ToString();
        }
    }

    public class ExecuteCommand : IArkTask
    {
        public ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException("命令不能为空", nameof(command));
            Command = command;
        }

        public string Command { get; set; }
        public int Timeout { get; set; } = 2000;

        public ExecuteResult Execute()
        {
            Process? process =
                Process.Start(new ProcessStartInfo("cmd.exe", "/c " + Command) { CreateNoWindow = true });
            if (process == null) return new ExecuteResult(false, "无法启动cmd");

            return process.WaitForExit(Timeout) ? new ExecuteResult(false, "命令超时") : new ExecuteResult(true, "命令已执行");
        }

        public override string ToString() => $"执行命令：{Command[..Command.IndexOf(' ')]}...";
    }
}