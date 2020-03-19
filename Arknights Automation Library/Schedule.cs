using System.Collections.Generic;

namespace REVUnit.AutoArknights.Core
{
    public class Schedule
    {
        private readonly Queue<Job> _jobs = new Queue<Job>();
        private readonly UI _ui;

        public Schedule(UI ui)
        {
            _ui = ui;
        }

        public IEnumerable<Job> Jobs => _jobs;

        public void Add(Job job)
        {
            _jobs.Enqueue(job);
        }

        public void ExecuteAll()
        {
            for (var i = 0; i < _jobs.Count; i++)
            {
                Job job = _jobs.Dequeue();
                job.Execute(_ui);
            }
        }
    }
}