using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace HatDog.Train
{
    class HatDogTrainGameManager : MBGameManager
    {
        protected override void DoLoadingForGameManager(
          GameManagerLoadingSteps gameManagerLoadingStep,
          out GameManagerLoadingSteps nextStep)
        {
            nextStep = GameManagerLoadingSteps.None;
            switch (gameManagerLoadingStep)
            {
                case GameManagerLoadingSteps.PreInitializeZerothStep:
                    MBGameManager.LoadModuleData(false);
                    MBGlobals.InitializeReferences();
                    Game.CreateGame(new HatDogGame(), this).DoLoading();
                    nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.FirstInitializeFirstStep:
                    bool flag = true;
                    foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
                        flag = flag && subModule.DoLoading(Game.Current);
                    nextStep = flag ? GameManagerLoadingSteps.WaitSecondStep : GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.WaitSecondStep:
                    StartNewGame();
                    nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.SecondInitializeThirdState:
                    nextStep = Game.Current.DoLoading() ? GameManagerLoadingSteps.PostInitializeFourthState : GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.PostInitializeFourthState:
                    nextStep = GameManagerLoadingSteps.FinishLoadingFifthStep;
                    break;
                case GameManagerLoadingSteps.FinishLoadingFifthStep:
                    nextStep = GameManagerLoadingSteps.None;
                    break;
            }
        }

        public override void OnLoadFinished()
        {
            base.OnLoadFinished();
            BasicCharacterObject character = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(HatDogTrainConst.AI_UNIT);
            BasicCultureObject culture = MBObjectManager.Instance.GetObject<BasicCultureObject>(HatDogTrainConst.AI_CULTURE);
            CustomBattleCombatant playerParty = new CustomBattleCombatant(new TextObject("{=!}P1"), culture, new Banner(HatDogTrainConst.BANNER_CODE[0]))
            {
                Side = BattleSideEnum.Attacker
            };
            CustomBattleCombatant enemyParty = new CustomBattleCombatant(new TextObject("{=!}P2"), culture, new Banner(HatDogTrainConst.BANNER_CODE[1]))
            {
                Side = BattleSideEnum.Defender
            };
            playerParty.AddCharacter(character, 100);
            enemyParty.AddCharacter(character, 100);
            HatDogTrainMissions.OpenCustomBattleMission(
                               "battle_terrain_a",
                               character,
                               playerParty,
                               enemyParty,
                               true,
                               ""
                            );
        }
    }
}
