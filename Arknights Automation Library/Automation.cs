using System;
using System.Collections.Generic;

namespace REVUnit.AutoArknights.Core
{
    public class Automation : IDisposable
    {
        public Automation(string adbPath)
        {
            Interactor = new Interactor(adbPath);
        }

        public Automation(string adbPath, string adbRemote)
        {
            Interactor = new Interactor(adbPath, adbRemote);
        }

        public Queue<ArkAction> Actions { get; set; } = new Queue<ArkAction>();

        public Interactor Interactor { get; }

        public void Dispose()
        {
            Interactor.Dispose();
        }

        public void Connect(string adbRemote)
        {
            Interactor.NewRemote(adbRemote);
        }

        public void DoAll()
        {
            if (!Interactor.Connected) throw new Exception("未连接到目标ADB，不能执行");
            for (var i = 0; i < Actions.Count; i++)
            {
                ArkAction action = Actions.Dequeue();
                action.Execute(Interactor);
            }
        }
    }
}