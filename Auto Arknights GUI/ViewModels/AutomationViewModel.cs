using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using REVUnit.AutoArknights.Core;
using REVUnit.AutoArknights.GUI.Properties;

namespace REVUnit.AutoArknights.GUI.ViewModels
{
    public class AutomationViewModel : ReactiveObject
    {
        public AutomationViewModel()
        {
            BaiduOcr.ApiKey = Settings.Default.ApiKey;
            BaiduOcr.SecretKey = Settings.Default.SecretKey;
            Model = new Automation(Settings.Default.AdbPath, Settings.Default.Remote);
            // IObservable<bool> connected = Observable.Repeat(Unit.Default).Select(async _ =>
            // {
            //     try
            //     {
            //         Model.Connect(Settings.Default
            //             .Remote);
            //         return true;
            //     }
            //     catch
            //     {
            //         return false;
            //     }
            // });
            NewJobCommand = ReactiveCommand.Create(() =>
            {
                Model.Schedule.Add(new RepeatLevelJob(RepeatLevelJob.Mode.SpecifiedTimes, 1));
                this.RaisePropertyChanged(nameof(Jobs));
            });
            ExecuteScheduleCommand =
                ReactiveCommand.CreateFromTask(() => Task.Run(() => Model.Schedule.ExecuteAll()));
        }

        public ImageSource Diagram { [ObservableAsProperty] get; }
        public ReactiveCommand<Unit, Unit> NewJobCommand { get; }
        public ReactiveCommand<Unit, Unit> ExecuteScheduleCommand { get; }
        private Automation Model { get; }

        public IEnumerable<RepeatLevelJobViewModel> Jobs =>
            Model.Schedule.Jobs.Select(job => new RepeatLevelJobViewModel(job));
    }
}