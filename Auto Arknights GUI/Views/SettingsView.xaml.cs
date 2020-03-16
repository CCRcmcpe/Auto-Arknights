using System.Reactive.Disposables;
using ReactiveUI;
using REVUnit.AutoArknights.GUI.ViewModels;

namespace REVUnit.AutoArknights.GUI.Views
{
    /// <summary>
    ///     SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            ViewModel = new SettingsViewModel();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Properties, v => v.GridSettings.ItemsSource).DisposeWith(d);
            });
        }
    }
}