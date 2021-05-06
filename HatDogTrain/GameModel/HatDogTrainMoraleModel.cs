using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace HatDog.Train.GameModel
{
    class HatDogTrainMoraleModel : BattleMoraleModel
    {
        public override (float killedSideMoraleChange, float killerSideMoraleChange) CalculateMoraleChangeAfterAgentKilled(Agent killedAgent, Agent killerAgent, SkillObject killerWeaponRelevantSkill)
        {
            return (0, 0);
        }

        public override (float panickedSideMoraleChange, float affectorSideMoraleChange) CalculateMoraleChangeAfterAgentPanicked(Agent agent)
        {
            return (0, 0);
        }

        public override float CalculateMoraleChangeToCharacter(Agent agent, float moraleChange, float distance)
        {
            return moraleChange;
        }

        public override float GetEffectiveInitialMorale(Agent agent, float baseMorale)
        {
            return baseMorale;
        }

        public override bool CanPanicDueToMorale(Agent agent)
        {
            return false;
        }

        public override float GetImportance(Agent agent)
        {
            return 1f;
        }
    }
}
