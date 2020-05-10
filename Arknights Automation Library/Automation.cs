using System;

namespace REVUnit.AutoArknights.Core
{
    public class Automation : IDisposable
    {
        public Automation(string adbPath)
        {
            Ui = new UI(adbPath);
        }

        public Automation(string adbPath, string adbRemote)
        {
            Ui = new UI(adbPath, adbRemote);
        }

        public UI Ui { get; }

        public Schedule Schedule { get; set; } = new Schedule();

        public void Dispose()
        {
            Ui.Dispose();
        }

        public void Connect(string adbRemote)
        {
            Ui.NewRemote(adbRemote);
        }
    }
}