
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace HatDog.Train
{
    [ViewCreatorModule]
    class HatDogTrainViews
    {
        [ViewMethod("HatDogTrainBattle")]
        public static MissionView[] OpenCustomBattleMission(Mission mission) => new List<MissionView>()
    {
      ViewCreator.CreateMissionSingleplayerEscapeMenu(),
      ViewCreator.CreateMissionAgentLabelUIHandler(mission),
      ViewCreator.CreateMissionBattleScoreUIHandler(mission,  new CustomBattleScoreboardVM()),
      ViewCreator.CreateOptionsUIHandler(),
      ViewCreator.CreateMissionOrderUIHandler(),
       new OrderTroopPlacer(),
      ViewCreator.CreateMissionAgentStatusUIHandler(mission),
      ViewCreator.CreateMissionMainAgentEquipmentController(mission),
      ViewCreator.CreateMissionMainAgentCheerControllerView(mission),
      ViewCreator.CreateMissionAgentLockVisualizerView(mission),
      ViewCreator.CreateMissionBoundaryCrossingView(),
       new MissionBoundaryWallView(),
      ViewCreator.CreateMissionFormationMarkerUIHandler(mission),
      ViewCreator.CreateSingleplayerMissionKillNotificationUIHandler(),
      ViewCreator.CreateMissionSpectatorControlView(mission),
      ViewCreator.CreatePhotoModeView(),
       new MissionAgentContourControllerView(),
       new MissionCustomBattlePreloadView()
    }.ToArray();
    }
}
