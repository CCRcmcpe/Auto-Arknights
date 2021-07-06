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

        public async Task ClaimCreditPoints()
        {
            await _game.BackToMainScreen();

            await _i.ClickFor("Home/Friends", RegistrationType.FeatureMatching);
            await Task.Delay(5000);

            await _i.ClickFor("Profile/FriendsList");
            await Task.Delay(2000);

            await _i.ClickFor("Profile/Visit");
            await Task.Delay(5000);

            if (!await _i.TestAppear("Infra/VisitNext")) return;

            async Task WaitForAllCpClaimed()
            {
                while (!await _i.TestAppear("Infra/VisitNextGrey"))
                {
                    await Task.Delay(2000);
                }
            }

            Task waitForCpCollected = WaitForAllCpClaimed();
            while (!waitForCpCollected.IsCompleted)
            {
                await _i.Click(RelativeArea.VisitNextButton);
            }

            await _game.BackToMainScreen();
        }

        public void ClaimProducts()
        {
            throw new NotImplementedException();
        }
    }
}