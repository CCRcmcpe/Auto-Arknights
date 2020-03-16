using System;

namespace REVUnit.AutoArknights.GUI.Core
{
    public class Automation : IDisposable
    {
        public Automation(string adbPath)
        {
            UI = new UI(adbPath);
            Schedule = new Schedule(UI);
        }

        public Automation(string adbPath, string adbRemote)
        {
            UI = new UI(adbPath, adbRemote);
            Schedule = new Schedule(UI);
        }

        public UI UI { get; }

        public Schedule Schedule { get; set; }

        public void Dispose()
        {
            UI.Dispose();
        }

        public void Connect(string adbRemote)
        {
            UI.NewRemote(adbRemote);
        }
    }
}