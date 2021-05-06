
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace HatDog.Train.Logic
{
    class HatDogTrainAgentLogic : CustomBattleAgentLogic
    {
        public override void OnAgentHit(
          Agent affectedAgent,
          Agent affectorAgent,
          int damage,
          in MissionWeapon affectorWeapon)
        {
            if (affectedAgent.Character == null || (affectorAgent?.Character == null || affectedAgent.State != AgentState.Active))
                return;
            bool isFatal = (double)affectedAgent.Health - (double)damage < 1.0;
            bool isTeamKill = affectedAgent.Team.Side == affectorAgent.Team.Side;
            affectorAgent.Origin.OnScoreHit(affectedAgent.Character, affectorAgent?.Formation?.Captain?.Character, damage, isFatal, isTeamKill, affectorWeapon.CurrentUsageItem);
        }

        public override void OnAgentRemoved(
          Agent affectedAgent,
          Agent affectorAgent,
          AgentState agentState,
          KillingBlow killingBlow)
        {
            if (affectorAgent == null && affectedAgent.IsMount && agentState == AgentState.Routed || affectedAgent.Origin == null)
                return;
            switch (agentState)
            {
                case AgentState.Unconscious:
                    affectedAgent.Origin.SetWounded();
                    if (affectedAgent == this.Mission.MainAgent)
                        this.BecomeGhost();
                    break;
                case AgentState.Killed:
                    affectedAgent.Origin.SetKilled();
                    break;
                default:
                    affectedAgent.Origin.SetRouted();
                    break;
            }
        }

        private void BecomeGhost()
        {
            Agent leader = this.Mission.PlayerEnemyTeam.Leader;
            if (leader != null)
                leader.Controller = Agent.ControllerType.AI;
            this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
        }
    }
}
