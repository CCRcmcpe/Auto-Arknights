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
            Combat = new CombatModule(interactor);
            Infrastructure = new InfrastructureModule(interactor);
        }

        public CombatModule Combat { get; }
        public InfrastructureModule Infrastructure { get; }

        public async Task BackToMainScreen()
        {
            while (true)
            {
                await _i.Update();
                var cts = new CancellationTokenSource();
                Task clickYes = Task.Run(async () =>
                {
                    if (await _i.TestAppear("Yes"))
                    {
                        await _i.Click("Yes");
                    }
                }, cts.Token);
                if (await _i.TestAppear("Home/Settings"))
                {
                    cts.Cancel();
                    return;
                }

                await clickYes;
                _i.Back();
                await Task.Delay(500);
            }
        }

        public async Task CollectTasks()
        {
            async Task WaitForTasksCollected()
            {
                while (true)
                {
                    await _i.Update();
                    Task<bool> task = _i.TestAppear("Tasks/AllCompleted");
                    if (!await _i.TestAppear("Tasks/Receive"))
                    {
                        return;
                    }

                    if (await task)
                    {
                        return;
                    }

                    await Task.Delay(200);
                }
            }

            async Task CollectCurrentTab()
            {
                Task waitForTasksCollected = WaitForTasksCollected();
                while (!waitForTasksCollected.IsCompleted)
                {
                    await _i.Click(RelativeArea.ReceiveTaskRewardButton);
                }
            }

            await BackToMainScreen();
            await _i.Click(RelativeArea.TasksButton);

            await Task.Delay(1000);
            await _i.Update();
            if (!await _i.TestAppear("Tasks/Daily"))
            {
                throw new Exception("Tasks interface not entered");
            }

            await CollectCurrentTab();

            await _i.Update();
            await _i.Click("Tasks/Weekly");
            await Task.Delay(1000);
            await CollectCurrentTab();
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