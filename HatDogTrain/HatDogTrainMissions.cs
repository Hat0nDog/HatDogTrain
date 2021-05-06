using HatDog.Train.Logic;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;

namespace HatDog.Train
{
    public static class HatDogTrainMissions
    {
        private static AtmosphereInfo CreateAtmosphereInfoForMission()
        {
            return new AtmosphereInfo
            {
                AtmosphereName = "TOD_04_00_SemiCloudy",
                TimeInfo = new TimeInformation
                {
                    Season = 1
                }
            };
        }

        [MissionMethod]
        public static Mission OpenCustomBattleMission(string scene, BasicCharacterObject character, CustomBattleCombatant playerParty, CustomBattleCombatant enemyParty, bool isPlayerGeneral, string sceneLevels = "")
        {
            BattleSideEnum playerSide = playerParty.Side;
            bool isPlayerAttacker = playerSide == BattleSideEnum.Attacker;
            IMissionTroopSupplier[] troopSuppliers = new IMissionTroopSupplier[2];
            troopSuppliers[(int)playerParty.Side] = new CustomBattleTroopSupplier(playerParty, isPlayerSide: true);
            troopSuppliers[(int)enemyParty.Side] = new CustomBattleTroopSupplier(enemyParty, isPlayerSide: false);
            bool isPlayerSergeant = !isPlayerGeneral;
            return MissionState.OpenNew("HatDogTrainBattle", new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = CreateAtmosphereInfoForMission(),
                SceneLevels = sceneLevels,
                TimeOfDay = 6f
            }, (Mission missionController) => new MissionBehaviour[]
            {
                new MissionOptionsComponent(),
                new BattleEndLogic(),
                new MissionCombatantsLogic(null, playerParty, (!isPlayerAttacker) ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty, Mission.MissionTeamAITypeEnum.FieldBattle, isPlayerSergeant),
                new BattleObserverMissionLogic(),
                new HatDogTrainAgentLogic(),
                new MissionAgentSpawnLogic(troopSuppliers, playerSide),
                new CustomBattleMissionSpawnHandler((!isPlayerAttacker) ? playerParty : enemyParty, isPlayerAttacker ? playerParty : enemyParty),
                new HatDogMissionDeployReSetLogic(),
                new HatDogTrainAgentAILogic(),
                new AgentVictoryLogic(),
                new MissionAgentPanicHandler(),
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new BattleMissionAgentInteractionLogic(),
                new AgentFadeOutLogic(),
                new AgentMoraleInteractionLogic(),
                new AssignPlayerRoleInTeamMissionController(isPlayerGeneral, isPlayerSergeant, isPlayerInArmy: false, isPlayerSergeant ? Enumerable.Repeat(character.StringId, 1).ToList() : new List<string>()),
                new HighlightsController(),
                new BattleHighlightsController()
            });
        }

    }
}
