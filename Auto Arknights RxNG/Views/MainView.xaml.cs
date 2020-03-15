using System;
using System.Globalization;
using System.Reactive.Disposables;
using ReactiveUI;
using REVUnit.AutoArknights.ViewModels;

namespace REVUnit.AutoArknights.Views
{
    /// <summary>
    ///     MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            ViewModel = new AppViewModel();
            this.WhenActivated(d =>
            {
                Console.Beep();
                this.BindCommand(ViewModel, vm => vm.OpenSettingsWindow, v => v.ButtonOpenSettings).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ManagedMemory, v => v.TextMemory.Text,
                    l => (l / 1024d / 1024d).ToString("F2", CultureInfo.InvariantCulture) + " MB").DisposeWith(d);
            });
        }
    }
}