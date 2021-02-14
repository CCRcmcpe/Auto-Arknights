using REVUnit.AutoArknights.Core;

namespace REVUnit.AutoArknights.CLI
{
    public partial class Config
    {
        public ISettings.IRemote Remote { get; }

        private class RemoteConfigImpl : ISettings.IRemote
        {
            private readonly Config _config;

            public RemoteConfigImpl(Config config) => _config = config;

            public string AdbExecutable => _config.Required("Remote:AdbExecutable");
            public string Serial => _config.Required("Remote:Serial", "Remote:Address");
            public string? ShutdownCommand => _config.Optional("Remote:ShutdownCommand")?.Trim();
        }
    }
}