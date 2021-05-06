
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace HatDog.Train.Logic.Component
{
    class HatDogTrainAgentPollingComponent : AgentComponent
    {
        private static Agent prePalyerControlAgent = null;
        private static Agent currentPlayerControlAgent = null;
        private static Agent currentAiControlAgent = null;

        public HatDogTrainAgentPollingComponent(Agent agent)
               : base(agent)
        {
        }

        protected override void OnTickAsAI(float dt)
        {
            if(currentAiControlAgent != null && currentPlayerControlAgent != null)
            {
                currentAiControlAgent.SetLookAgent(currentPlayerControlAgent);
            }
            if (Agent.Team.IsPlayerTeam)
            {
                if (currentPlayerControlAgent == null && Agent.IsActive())
                {
                    Agent.Controller = Agent.ControllerType.Player;
                    Mission.Current.MainAgent = Agent;
                    Game.Current.PlayerTroop = Agent.Character;
                    currentPlayerControlAgent = Agent;
                    ((ScoreboardVM)Mission.Current.GetMissionBehaviour<BattleObserverMissionLogic>().BattleObserver).IsMainCharacterDead = false;
                }
            }
            else if (currentAiControlAgent == null && Agent.IsActive())
            {
                currentAiControlAgent = Agent;
                Agent.SetAiBehaviorParams(AISimpleBehaviorKind.Melee, 8f, 7f, 4f, 20f, 1f);
            }

            if (prePalyerControlAgent == Agent && currentPlayerControlAgent != null)
            {
                prePalyerControlAgent.RemoveComponent(prePalyerControlAgent.GetComponent<HatDogTrainAgentPollingComponent>());
                prePalyerControlAgent.Controller = Agent.ControllerType.AI;
            }
        }

        protected override void OnAgentRemoved()
        {
            if (Agent.Team.IsPlayerTeam)
            {
                if (currentPlayerControlAgent == Agent)
                {
                    currentPlayerControlAgent = null;
                    prePalyerControlAgent = Agent;
                }
            }
            else if (currentAiControlAgent == Agent)
            {
                currentAiControlAgent = null;
            }
        }

    }
}
