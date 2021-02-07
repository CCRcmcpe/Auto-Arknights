using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using REVUnit.AutoArknights.Core.Properties;

namespace REVUnit.AutoArknights.Core.Tasks
{
    public abstract class PostAction : IArkTask
    {
        public abstract ExecuteResult Execute();

        public static PostAction Parse(char c, ISettings settings)
        {
            return c switch
            {
                'c' => new Shutdown(),
                'r' => new Reboot(),
                's' => new Suspend(false) { Forced = settings.ForcedSuspend },
                'h' => new Suspend(true) { Forced = settings.ForcedSuspend },
                'e' => new CloseRemote(settings.Remote.ShutdownCommand ??
                                       throw new Exception(Resources.PostActions_CloseRemote_Exception_EmptyCommand)),
                _ => throw new FormatException(string.Format(Resources.PostAction_Exception_ParseFailed, c))
            };
        }
    }

    public class Shutdown : PostAction
    {
        public override ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/p");
            return ExecuteResult.Success();
        }

        public override string ToString() => Resources.PostAction_Shutdown;
    }

    public class Reboot : PostAction
    {
        public override ExecuteResult Execute()
        {
            Process.Start("shutdown.exe", "/r /t 0");
            return ExecuteResult.Success();
        }

        public override string ToString() => Resources.PostAction_Reboot;
    }

    public class Suspend : PostAction
    {
        public Suspend(bool hibernate)
        {
            if (hibernate && !IsPwrHibernateAllowed())
                throw new NotSupportedException(Resources.PostAction_Suspend_Exception_HibernateNotSupported);
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
            if (Forced) b.Append(Resources.PostAction_Suspend_Forced);

            b.Append(!Hibernate ? Resources.PostAction_Suspend_Sleep : Resources.PostAction_Suspend_Hibernate);
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
            if (process == null) return new ExecuteResult(false, Resources.PostAction_ExecuteCommand_CannotStartCmd);

            return process.WaitForExit(Timeout)
                ? new ExecuteResult(false, Resources.PostAction_ExecuteCommand_Exception_Timeout)
                : new ExecuteResult(true, Resources.PostAction_ExecuteCommand_Completed);
        }

        public override string ToString() => string.Format(Resources.PostAction_ExecuteCommand);
    }

    public class CloseRemote : ExecuteCommand
    {
        public CloseRemote(string command) : base(command) { }
        public override string ToString() => Resources.PostAction_CloseRemote;
    }
}