using System;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

#if RELEASE
using REVUnit.Crlib.Extension;
#endif

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
            var app = App.Instance;
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(app.Config.Inner).WriteTo.Console(theme: AnsiConsoleTheme.Code).WriteTo.Debug().WriteTo
                                                  .File($@"Log\{DateTime.Now}.log").CreateLogger();
#if DEBUG
            app.Run();
#elif RELEASE
            try
            {
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