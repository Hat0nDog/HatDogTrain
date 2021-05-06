using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace HatDog.Train
{
    public class HatDogTrainSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("BeginTrain",
                new TextObject("{=4a14df69}Begin Train",
                    null),
                    5000,
                    () => MBGameManager.StartNewGame(new HatDogTrainGameManager()),
                    () => false,
                    null));
        }
    }
}
