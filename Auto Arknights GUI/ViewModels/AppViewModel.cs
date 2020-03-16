using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using REVUnit.AutoArknights.GUI.Views;

namespace REVUnit.AutoArknights.GUI.ViewModels
{
    public sealed class AppViewModel : ReactiveObject, IDisposable
    {
        private readonly Process _currentProcess = Process.GetCurrentProcess();

        private readonly ObservableAsPropertyHelper<long> _managedMemory;

        public AppViewModel()
        {
            _managedMemory = Observable.Interval(TimeSpan.FromMilliseconds(500))
                .Select(_ =>
                {
                    _currentProcess.Refresh();
                    return _currentProcess.PrivateMemorySize64;
                }).ToProperty(this, vm => vm.ManagedMemory, scheduler: RxApp.MainThreadScheduler);
        }

        public ReactiveCommand<Unit, Unit> OpenSettingsWindow { get; } = ReactiveCommand.Create(() =>
        {
            new SettingsWindow().ShowDialog();
        });

        public long ManagedMemory => _managedMemory.Value;
        //[ObservableAsProperty] public long ManagedMemory { get; }
        //not working, see https://github.com/reactiveui/ReactiveUI/issues/2243

        public void Dispose()
        {
            _currentProcess.Dispose();
        }
    }
}