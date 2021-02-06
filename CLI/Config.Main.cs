namespace REVUnit.AutoArknights.CLI
{
    public partial class Config
    {
        public bool ForcedSuspend => Optional("ForcedSuspend", bool.Parse);
    }
}