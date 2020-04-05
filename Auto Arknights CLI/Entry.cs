namespace REVUnit.AutoArknights.CLI
{
    public static class Entry
    {
        public static void Main()
        {
            using var app = new AutoArknights();
            app.Run();
        }
    }
}