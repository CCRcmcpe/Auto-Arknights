using System;
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
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            Console.WriteLine(Resources.StartupLogo);

            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(App.Config.Inner).WriteTo.Console(theme: AnsiConsoleTheme.Code).WriteTo
                        .Debug().WriteTo
                        .File($"Log/{DateTime.Now:yyyy-MM-dd hh.mm.ss}.log").CreateLogger();

            Log.Information(Resources.App_Starting);

            var app = App.Instance;

            Log.Information(Resources.App_Started);
            Console.Clear();
#if DEBUG
            app.Run();
#else
            try
            {
                app.Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, Resources.Entry_FatalException);
                XConsole.AnyKey();
            }
#endif
        }
    }
}