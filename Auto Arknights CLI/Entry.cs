#if RELEASE
using System;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;
#endif

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
#if DEBUG
            var app = new AutoArknightsCli();
            app.Run();
#else
            try
            {
                var app = new AutoArknightsCli();
                app.Run();
            }
            catch (Exception e)
            {
                Log.Error(e.Message, withTime: true);
                Log.Debug(e.StackTrace);
                XConsole.AnyKey();
            }
#endif
        }
    }
}