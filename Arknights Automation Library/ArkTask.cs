namespace REVUnit.AutoArknights.Core
{
    public abstract class ArkTask
    {
        protected readonly Device Device;

        protected ArkTask(Device device)
        {
            Device = device;
        }

        public abstract ExecuteResult Execute();
    }
}