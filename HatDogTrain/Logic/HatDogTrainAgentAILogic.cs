using HatDog.Train.Logic.Component;
using System;
using TaleWorlds.MountAndBlade;

namespace HatDog.Train.Logic
{
    class HatDogTrainAgentAILogic : MissionLogic
    {
        private void AddComponents(Agent agent)
        {
            agent.AddComponent(new HatDogTrainAgentPollingComponent(agent));
            agent.AddComponent(new AgentAIStateFlagComponent(agent));
            if (agent.IsHuman)
            {
                agent.AddComponent(new UseObjectAgentComponent(agent));
                agent.AddComponent(new ItemPickupAgentComponent(agent));
            }
        }

        private void RemoveComponents(Agent agent)
        {
            agent.RemoveComponent(agent.GetComponent<AgentAIStateFlagComponent>());
            if (agent.IsHuman)
            {
                agent.RemoveComponent(agent.GetComponent<ItemPickupAgentComponent>());
                agent.RemoveComponent(agent.GetComponent<UseObjectAgentComponent>());
            }
        }

        public override void OnAgentCreated(Agent agent)
        {
            if(agent.IsAIControlled)
            {
                AddComponents(agent);
            }
        }

        protected override void OnAgentControllerChanged(Agent agent)
        {
            if (agent.Controller == Agent.ControllerType.AI)
            {
                AddComponents(agent);
            }
            else if (agent.Controller == Agent.ControllerType.Player)
            {
                RemoveComponents(agent);
            }
        }
    }
}
