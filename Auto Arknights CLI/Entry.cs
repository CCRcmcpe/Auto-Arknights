using System;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
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
        }
    }
}