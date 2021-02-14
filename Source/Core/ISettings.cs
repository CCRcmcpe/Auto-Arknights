namespace REVUnit.AutoArknights.Core
{
    public interface ISettings
    {
        public bool ForcedSuspend { get; }

        public IRemote Remote { get; }
        public IIntervals Intervals { get; }

        public interface IRemote
        {
            public string AdbExecutable { get; }
            public string Serial { get; }
            public string? ShutdownCommand { get; }
        }

        public interface IIntervals
        {
            public double BeforeVerifyInLevel { get; }
            public double AfterLevelComplete { get; }
        }
    }
}