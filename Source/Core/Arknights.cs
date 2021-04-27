namespace REVUnit.AutoArknights.Core
{
    public class Arknights
    {
        private readonly Interactor _interactor;

        private Arknights(Interactor interactor)
        {
            _interactor = interactor;
            Combat = new CombatModule(interactor);
        }

        public CombatModule Combat { get; }

        public static Arknights FromDevice(IDevice device)
        {
            return new(new Interactor(device));
        }
    }
}