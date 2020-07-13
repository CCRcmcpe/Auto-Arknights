using System;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
#if DEBUG
            using var app = new AutoArknights();
            app.Run();
#else
            try
            {
                using var app = new AutoArknights();
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