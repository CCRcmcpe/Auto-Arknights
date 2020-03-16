namespace REVUnit.AutoArknights.GUI.Core
{
    public abstract class Job
    {
        //public int RetryThreshold { get; set; }
        public abstract ExecuteResult Execute(UI ui);

        // public IEnumerable<Job> ReadJobs(string jobsDir)
        // {
        //     return Directory.EnumerateFiles(jobsDir,
        //             "*.json",
        //             SearchOption.AllDirectories)
        //         .Select(jsonFile =>
        //             JsonConvert.DeserializeObject<Job>(File.ReadAllText(jsonFile)));
        // }
    }
}