using System.Reactive.Disposables;
using ReactiveUI;
using REVUnit.AutoArknights.GUI.ViewModels;

namespace REVUnit.AutoArknights.GUI.Views
{
    /// <summary>
    ///     AutomationView.xaml 的交互逻辑
    /// </summary>
    public partial class AutomationView
    {
        public AutomationView()
        {
            InitializeComponent();
            ViewModel = new AutomationViewModel();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Jobs, view => view.JobList.ItemsSource)
                    .DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.NewJobCommand, view => view.VAdd).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ExecuteScheduleCommand, view => view.VExecute).DisposeWith(d);
                //VAdd.Events().Click.Select(_ => true).BindTo(this, view => view.VDialogHost.IsOpen).DisposeWith(d);
            });
        }
    }
}