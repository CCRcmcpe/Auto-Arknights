﻿using System.Runtime.InteropServices;

namespace REVUnit.AutoArknights.CLI
{
    internal static class Native
    {
        [DllImport("powrprof.dll")]
        public static extern bool IsPwrHibernateAllowed();

        [DllImport("powrprof.dll")]
        public static extern uint SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
    }
}