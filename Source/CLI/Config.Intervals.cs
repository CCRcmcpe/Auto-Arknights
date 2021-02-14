using REVUnit.AutoArknights.Core;

namespace REVUnit.AutoArknights.CLI
{
    public partial class Config
    {
        public ISettings.IIntervals Intervals { get; }

        private class IntervalsConfigImpl : ISettings.IIntervals
        {
            private readonly Config _config;

            public IntervalsConfigImpl(Config config) => _config = config;

            public double BeforeVerifyInLevel => _config.Optional("Intervals:BeforeVerifyInLevel", double.Parse, 20);
            public double AfterLevelComplete => _config.Optional("Intervals:BeforeVerifyInLevel", double.Parse, 8);
        }
    }
}