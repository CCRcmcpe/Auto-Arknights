using System.Runtime.InteropServices;

namespace REVUnit.AutoArknights.CLI
{
    public static class Native
    {
        [DllImport("powrprof.dll")]
        public static extern bool IsPwrHibernateAllowed();

        [DllImport("Powrprof.dll")]
        public static extern uint SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
    }
}