#if !DEBUG
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
                Log.Error($"出现异常：{e.GetType().Name}，异常信息：\"{e.Message}\"");
                Log.Debug(e.StackTrace);
                XConsole.AnyKey();
            }
#endif
        }
    }
}