using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ModuleManager;
using HatDog.Train.GameModel;

namespace HatDog.Train
{
    class HatDogGame : GameType
    {
        protected override void OnInitialize()
        {
            Game currentGame = CurrentGame;
            currentGame.FirstInitialize(false);
            this.InitializeGameTexts(currentGame.GameTextManager);
            IGameStarter gameStarter = new BasicGameStarter();
            InitializeGameModels(gameStarter);
            GameManager.OnGameStart(CurrentGame, gameStarter);
            MBObjectManager objectManager = currentGame.ObjectManager;
            currentGame.SecondInitialize(gameStarter.Models);
            currentGame.CreateGameManager();
            GameManager.BeginGameStart(CurrentGame);
            CurrentGame.ThirdInitialize();
            currentGame.CreateObjects();
            currentGame.InitializeDefaultGameObjects();
            LoadCustomGameXmls();
            objectManager.ClearEmptyObjects();
            currentGame.SetDefaultEquipments(new Dictionary<string, Equipment>());
            currentGame.CreateLists();
            objectManager.ClearEmptyObjects();
            GameManager.OnCampaignStart(CurrentGame, null);
            GameManager.OnAfterCampaignStart(CurrentGame);
            GameManager.OnGameInitializationFinished(CurrentGame);
        }

        private void InitializeGameModels(IGameStarter basicGameStarter)
        {
            //此mod在击杀单位时触发，用于判断是否使用“手术”技能, 必须
            basicGameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
            //ai相关，自定义一波
            basicGameStarter.AddModel(new HatDogTrainAIAgentStatCalculateModel());
            //天气相关
            basicGameStarter.AddModel(new DoNothingApplyWeatherEffectsModel());
            //伤害参数与破防相关 自定义Model简化了计算
            basicGameStarter.AddModel(new SimplifyMultiplayerAgentApplyDamageModel());
            //骑马计算武器加速度相关，保持原始数据，不动
            basicGameStarter.AddModel(new DefaultRidingModel());
            //技能相关，给个技能列表
            basicGameStarter.AddModel(new HatDogTrainSkillList());
            //士气相关，简化大部分计算消除士气影响
            basicGameStarter.AddModel(new HatDogTrainMoraleModel());
            //这个model,啥也不做
            basicGameStarter.AddModel(new CustomBattleInitializationModel());
            //与骑射设置有关
            basicGameStarter.AddModel(new DefaultStrikeMagnitudeModel());
        }

        private void LoadCustomGameXmls()
        {
            ObjectManager.LoadXML("Monsters");
            ObjectManager.LoadXML("SkeletonScales");
            ObjectManager.LoadXML("ItemModifiers");
            ObjectManager.LoadXML("ItemModifierGroups");
            ObjectManager.LoadXML("CraftingPieces");
            ObjectManager.LoadXML("CraftingTemplates");
            ObjectManager.LoadXML("Items");
            ObjectManager.LoadXML("NPCCharacters");
            ObjectManager.LoadXML("SPCultures");
        }

        protected override void DoLoadingForGameType(
          GameTypeLoadingStates gameTypeLoadingState,
          out GameTypeLoadingStates nextState)
        {
            nextState = GameTypeLoadingStates.None;
            switch (gameTypeLoadingState)
            {
                case GameTypeLoadingStates.InitializeFirstStep:
                    CurrentGame.Initialize();
                    nextState = GameTypeLoadingStates.WaitSecondStep;
                    break;
                case GameTypeLoadingStates.WaitSecondStep:
                    nextState = GameTypeLoadingStates.LoadVisualsThirdState;
                    break;
                case GameTypeLoadingStates.LoadVisualsThirdState:
                    nextState = GameTypeLoadingStates.PostInitializeFourthState;
                    break;
            }
        }

        public override void OnDestroy()
        {
        }

        public override void OnStateChanged(GameState oldState)
        {
        }

        protected override void BeforeRegisterTypes(MBObjectManager objectManager)
        {
        }

        protected override void OnRegisterTypes(MBObjectManager objectManager)
        {
            objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "NPCCharacters", 43U);
            objectManager.RegisterType<BasicCultureObject>("Culture", "SPCultures", 17U);
        }

        private void InitializeGameTexts(GameTextManager gameTextManager)
        {
            gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/multiplayer_strings.xml");
            gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/global_strings.xml");
            gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/module_strings.xml");
            gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/native_strings.xml");
        }
    }
}
