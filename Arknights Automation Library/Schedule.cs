using System.Collections.Generic;

namespace REVUnit.AutoArknights.GUI.Core
{
    public class Schedule
    {
        private readonly List<Job> _jobs = new List<Job>();
        private readonly UI _ui;

        public Schedule(UI ui)
        {
            _ui = ui;
        }

        public IReadOnlyList<Job> Jobs => _jobs;

        public void Add(Job job)
        {
            _jobs.Add(job);
        }

        public void ExecuteAll()
        {
            foreach (Job job in _jobs) job.Execute(_ui);
        }
    }
}