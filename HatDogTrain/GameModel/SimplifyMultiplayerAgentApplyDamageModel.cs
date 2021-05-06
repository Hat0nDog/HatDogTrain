using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace HatDog.Train.GameModel
{
    class SimplifyMultiplayerAgentApplyDamageModel : MultiplayerAgentApplyDamageModel
    {
        public override float CalculateShieldDamage(float baseDamage)
        {
            return baseDamage;
        }
        public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
        {
            return false;
        }
    }
}
