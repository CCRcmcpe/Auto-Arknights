using System;
using System.Threading;
using System.Threading.Tasks;

namespace REVUnit.AutoArknights.Core
{
    public class Game
    {
        private readonly Interactor _i;

        private Game(Interactor interactor)
        {
            _i = interactor;
            Combat = new CombatModule(this, interactor);
            Infrastructure = new InfrastructureModule(this, interactor);
        }

        public CombatModule Combat { get; }
        public InfrastructureModule Infrastructure { get; }

        public async Task BackToMainScreen()
        {
            while (true)
            {
                using (var cts = new CancellationTokenSource())
                {
                    Task clickYes = Task.Run(async () =>
                    {
                        if (await _i.TestAppear("Yes"))
                        {
                            await _i.ClickFor("Yes");
                        }
                    }, cts.Token);
                    if (await _i.TestAppear("Home/Settings"))
                    {
                        cts.Cancel();
                        return;
                    }

                    await clickYes;
                }

                _i.Back();
                await Task.Delay(500);
            }
        }

        public async Task ClaimTasks()
        {
            async Task TabClaim()
            {
                if (await _i.TestAppear("Tasks/ClaimAll"))
                {
                    await _i.ClickFor("Tasks/ClaimAll");
                    await Task.Delay(1000);
                    await _i.Click(RelativeArea.LowerBottom);
                }
            }

            await BackToMainScreen();
            await _i.Click(RelativeArea.TasksButton);

            await Task.Delay(1000);
            if (!await _i.TestAppear("Tasks/Daily"))
            {
                throw new Exception("Tasks interface not entered");
            }

            await TabClaim();

            await _i.ClickFor("Tasks/Weekly");
            await Task.Delay(1000);
            await TabClaim();
            await Task.Delay(200);

            _i.Back();
        }

        public static async Task<Game> FromDevice(IDevice device)
        {
            return new(await Interactor.FromDevice(device));
        }

        public async Task Recurit()
        {
            await BackToMainScreen();
        }
    }
}