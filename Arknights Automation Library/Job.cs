namespace REVUnit.AutoArknights.Core
{
    public abstract class Job
    {
        protected readonly UI Ui;

        protected Job(UI ui)
        {
            Ui = ui;
        }

        public abstract ExecuteResult Execute();
    }
}