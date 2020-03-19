using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using REVUnit.AutoArknights.Core;
using REVUnit.Crlib.Extension;
using REVUnit.Crlib.Input;

namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
            var src = new CancellationTokenSource();
            var task = new Task(() =>
            {
                using var app = new AutoArknights();
                app.Run();
            }, src.Token, TaskCreationOptions.LongRunning);
            XConsole.Exiting += () =>
            {
                src.Cancel();
                Console.WriteLine("已释放非托管资源。");
                Environment.Exit(0);
            };
            task.Start();
            task.Wait(src.Token);
        }
    }
}