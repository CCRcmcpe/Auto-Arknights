using System;
using System.Threading.Tasks;

namespace REVUnit.AutoArknights.Core
{
    public class InfrastructureModule
    {
        private readonly Game _game;
        private readonly Interactor _i;

        internal InfrastructureModule(Game game, Interactor interactor)
        {
            _game = game;
            _i = interactor;
        }

        public void Collect()
        {
            throw new NotImplementedException();
        }

        public async Task CollectCreditPoints()
        {
            await _game.BackToMainScreen();

            await _i.ClickFor("Home/Friends", RegistrationType.FeatureMatching);
            await Task.Delay(5000);

            await _i.ClickFor("Home/FriendsList");
            await Task.Delay(2000);

            await _i.ClickFor("Home/Visit");
            await Task.Delay(5000);

            async Task WaitForCpCollected()
            {
                while (true)
                {
                    if (!await _i.TestAppear("Infra/VisitNext"))
                    {
                        return;
                    }

                    await Task.Delay(1000);
                }
            }

            Task waitForCpCollected = WaitForCpCollected();
            while (!waitForCpCollected.IsCompleted)
            {
                await _i.Click(RelativeArea.VisitNextButton);
            }

            await _game.BackToMainScreen();
        }
    }
}