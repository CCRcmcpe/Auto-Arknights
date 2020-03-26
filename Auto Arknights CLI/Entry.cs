namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
            using var app = new AutoArknights();
            app.Run();
            // 经过实验，并不能释放非托管资源。
            // TODO See https://github.com/CCRcmcpe/Auto-Arknights/issues/1
            // var src = new CancellationTokenSource();
            // var task = new Task(() =>
            // {
            //     using var app = new AutoArknights();
            //     app.Run();
            // }, src.Token, TaskCreationOptions.LongRunning);
            // XConsole.Exiting += () =>
            // {
            //     src.Cancel();
            //     Console.WriteLine("已释放非托管资源。");
            //     Environment.Exit(0);
            // };
            // task.Start();
            // task.Wait(src.Token);
        }
    }
}