using TaleWorlds.MountAndBlade;

namespace HatDog.Train.Logic
{
    class HatDogMissionDeployReSetLogic : MissionLogic
    {
        public override void AfterStart()
        {
            Mission.Current.MakeDeploymentPlan();
        }
    }
}
