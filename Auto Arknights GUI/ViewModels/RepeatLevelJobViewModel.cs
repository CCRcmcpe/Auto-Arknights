using REVUnit.AutoArknights.Core;

namespace REVUnit.AutoArknights.GUI.ViewModels
{
    public class RepeatLevelJobViewModel
    {
        private readonly RepeatLevelJob _model;

        public RepeatLevelJobViewModel(int repeatTime = 1)
        {
            _model = new RepeatLevelJob(RepeatLevelJob.Mode.SpecifiedTimes /* TODO IMPLEMENT MODES */, repeatTime);
        }

        public RepeatLevelJobViewModel(Job model)
        {
            _model = (RepeatLevelJob) model;
        }

        public int RepeatTime
        {
            get => _model.RepeatTime;
            set => _model.RepeatTime = value;
        }
    }
}