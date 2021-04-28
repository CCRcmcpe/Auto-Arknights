using System;
using REVUnit.AutoArknights.Core.CV;

namespace REVUnit.AutoArknights.Core
{
    public class Arknights
    {
        private readonly Interactor _i;

        private Arknights(Interactor interactor)
        {
            _i = interactor;
            Combat = new CombatModule(interactor);
            Infrastructure = new InfrastructureModule(interactor);
        }

        public CombatModule Combat { get; }
        public InfrastructureModule Infrastructure { get; }

        public void BackToMainScreen()
        {
            while (!_i.TestAppear("Home/Settings"))
            {
                _i.Back();
                if (_i.TestAppear("Yes", out RegistrationResult result))
                {
                    _i.Click(result.CircumRect);
                }
            }
        }

        public void CollectTasks()
        {
            void CollectCurrentTab()
            {
                while (_i.TestAppear("Tasks/Receive") && !_i.TestAppear("Tasks/AllCompleted"))
                {
                    _i.Click(RelativeArea.ReceiveTaskRewardButton);
                    _i.Back();
                }
            }

            BackToMainScreen();

            _i.Click(RelativeArea.TasksButton);
            Utils.Sleep(1);

            if (!_i.TestAppear("Tasks/Daily"))
            {
                throw new Exception();
            }

            CollectCurrentTab();

            _i.Click("Tasks/Weekly");
            Utils.Sleep(0.5);

            CollectCurrentTab();

            BackToMainScreen();
        }

        public static Arknights FromDevice(IDevice device)
        {
            return new(new Interactor(device));
        }
    }
}