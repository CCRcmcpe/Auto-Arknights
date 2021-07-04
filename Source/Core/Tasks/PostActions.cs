using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public class PostActionsSettings
    {
        private string? _shutdownRemoteCommand;

        public string? ShutdownRemoteCommand
        {
            get => _shutdownRemoteCommand;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("指令不能为空");
                }

                _shutdownRemoteCommand = value;
            }
        }
    }

    public class Shutdown : IArkTask
    {
        public ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/p");
            return ExecuteResult.Success();
        }

        public override string ToString()
        {
            return "关机";
        }
    }

    public class Reboot : IArkTask
    {
        public ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/r /t 0");
            return ExecuteResult.Success();
        }

        public override string ToString()
        {
            return "重启";
        }
    }

    public class Suspend : IArkTask
    {
        public Suspend(bool hibernate)
        {
            if (hibernate && !IsPwrHibernateAllowed())
                throw new NotSupportedException("系统未开启或不支持休眠");
            Hibernate = hibernate;
        }

        public bool Hibernate { get; set; }

        public ExecuteResult Execute()
        {
            bool success = SetSuspendState(Hibernate, false, false);
            return new ExecuteResult(success);
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append(Hibernate ? "休眠" : "睡眠");
            return b.ToString();
        }

        [DllImport("powrprof.dll")]
        private static extern bool IsPwrHibernateAllowed();

        [DllImport("powrprof.dll")]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical /* useless */,
            bool disableWakeEvent);
    }

    public class ExecuteCommand : IArkTask
    {
        public ExecuteCommand(string command)
        {
            Command = command;
        }

        public string Command { get; set; }
        public int Timeout { get; set; } = 2000;

        public ExecuteResult Execute()
        {
            Process? process =
                Process.Start(new ProcessStartInfo("cmd.exe", "/c " + Command) {CreateNoWindow = true});
            if (process == null) return new ExecuteResult(false, "无法启动cmd");

            return process.WaitForExit(Timeout)
                ? new ExecuteResult(false, "指令超时")
                : new ExecuteResult(true, "指令已执行");
        }

        public override string ToString()
        {
            return "执行指令";
        }
    }

    public class ShutdownRemote : ExecuteCommand
    {
        public ShutdownRemote(string command) : base(command)
        {
        }

        public override string ToString()
        {
            return "关闭远端";
        }
    }
}