using System;
using System.IO;
using System.Reflection;
using REVUnit.AutoArknights.CLI.Properties;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
#if !DEBUG
using REVUnit.Crlib.Extension;

#endif

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            var app = App.Instance;
            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(app.Config.Inner).WriteTo.Console(theme: AnsiConsoleTheme.Code).WriteTo
                        .Debug().WriteTo
                        .File($"Log/{DateTime.Now:yyMMdd-hh:mm:ss}.log").CreateLogger();
#if DEBUG
            app.Run();
#else
            try
            {
                app.Run();
            }
            catch (Exception e)
            {
                Log.Error(string.Format(Resources.Entry_Exception, e.GetType().Name, e.Message));
                Log.Debug(e.StackTrace);
                XConsole.AnyKey();
            }
#endif
        }
    }
}