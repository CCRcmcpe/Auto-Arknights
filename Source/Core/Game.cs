﻿using System;
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
            while (!await _i.TestAppear("Home/Settings"))
            {
                _i.Back();
                if (await _i.TestAppear("Yes"))
                {
                    await _i.Click("Yes");
                }
            }
        }

        public async Task CollectTasks()
        {
            async Task CollectCurrentTab()
            {
                while (!await _i.TestAppear("Tasks/AllCompleted") && !await _i.TestAppear("Tasks/Receive"))
                {
                    _i.Click(RelativeArea.ReceiveTaskRewardButton);
                    _i.Back();
                }
            }

            await BackToMainScreen();
            _i.Click(RelativeArea.TasksButton);
            await Task.Delay(2000);

            if (!await _i.TestAppear("Tasks/Daily"))
            {
                throw new Exception();
            }

            await CollectCurrentTab();

            await _i.Click("Tasks/Weekly");
            await Task.Delay(2000);
            await CollectCurrentTab();

            await BackToMainScreen();
        }

        public static async Task<Game> FromDevice(IDevice device)
        {
            return new(await Interactor.FromDevice(device));
        }
    }
}