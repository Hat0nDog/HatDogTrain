using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HatDog.Train
{
    class HatDogTrainAIAgentStatCalculateModel : AgentStatCalculateModel
    {
        public override void InitializeMissionEquipment(Agent agent)
        {
        }

        public override float GetEffectiveMaxHealth(Agent agent)
        {
            return agent.BaseHealthLimit;
        }

        public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
        {
            agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
            if (!agent.IsHuman)
            {
                agentDrivenProperties.AiSpeciesIndex = (int)spawnEquipment[EquipmentIndex.ArmorItemEndSlot].Item.Id.InternalValue;
                agentDrivenProperties.AttributeRiding = 0.8f + ((spawnEquipment[EquipmentIndex.HorseHarness].Item != null) ? 0.2f : 0f);
                float num = 0f;
                for (int i = 1; i < 12; i++)
                {
                    if (spawnEquipment[i].Item != null)
                    {
                        num += spawnEquipment[i].GetModifiedMountBodyArmor();
                    }
                }

                agentDrivenProperties.ArmorTorso = num;
                ItemObject item = spawnEquipment[EquipmentIndex.ArmorItemEndSlot].Item;
                if (item != null)
                {
                    float num2 = 1f;
                    if (!agent.Mission.Scene.IsAtmosphereIndoor)
                    {
                        if (agent.Mission.Scene.GetRainDensity() > 0f)
                        {
                            num2 *= 0.9f;
                        }

                        if (!MBMath.IsBetween(agent.Mission.Scene.TimeOfDay, 4f, 20.01f))
                        {
                            num2 *= 0.9f;
                        }
                    }

                    _ = item.HorseComponent;
                    EquipmentElement mountElement = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
                    EquipmentElement harness = spawnEquipment[EquipmentIndex.HorseHarness];
                    agentDrivenProperties.MountManeuver = mountElement.GetModifiedMountManeuver(in harness);
                    agentDrivenProperties.MountSpeed = num2 * (mountElement.GetModifiedMountSpeed(in harness) + 1) * 0.22f;
                    agentDrivenProperties.MountChargeDamage = mountElement.GetModifiedMountCharge(in harness) * 0.01f;
                    agentDrivenProperties.MountDifficulty = mountElement.Item.Difficulty;
                    int effectiveSkill = GetEffectiveSkill(agent.RiderAgent.Character, agent.RiderAgent.Origin, agent.RiderAgent.Formation, DefaultSkills.Riding);
                    agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(in mountElement, in harness, effectiveSkill);
                    if (agent.RiderAgent != null)
                    {
                        agentDrivenProperties.MountSpeed *= 1f + effectiveSkill * 0.001f;
                        agentDrivenProperties.MountManeuver *= 1f + effectiveSkill * 0.0004f;
                    }
                }
            }
            else
            {
                agentDrivenProperties.ArmorHead = spawnEquipment.GetHeadArmorSum();
                agentDrivenProperties.ArmorTorso = spawnEquipment.GetHumanBodyArmorSum();
                agentDrivenProperties.ArmorLegs = spawnEquipment.GetLegArmorSum();
                agentDrivenProperties.ArmorArms = spawnEquipment.GetArmArmorSum();
            }

            foreach (DrivenPropertyBonusAgentComponent item2 in agent.Components.OfType<DrivenPropertyBonusAgentComponent>())
            {
                if (MBMath.IsBetween((int)item2.DrivenProperty, 0, 56))
                {
                    float value = agentDrivenProperties.GetStat(item2.DrivenProperty) + item2.DrivenPropertyBonus;
                    agentDrivenProperties.SetStat(item2.DrivenProperty, value);
                }
            }
        }

        public override float GetWeaponInaccuracy(Agent agent, WeaponComponentData weapon, int weaponSkill)
        {
            float val = 0f;
            if (weapon.IsRangedWeapon)
            {
                val = (100f - weapon.Accuracy) * (1f - 0.002f * weaponSkill) * 0.001f;
            }
            else if (weapon.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
            {
                val = 1f - weaponSkill * 0.01f;
            }

            return Math.Max(val, 0f);
        }

        public override float GetInteractionDistance(Agent agent)
        {
            return 1.5f;
        }

        public override float GetMaxCameraZoom(Agent agent)
        {
            return 1f;
        }

        public override int GetEffectiveSkill(BasicCharacterObject agentCharacter, IAgentOriginBase agentOrigin, Formation agentFormation, SkillObject skill)
        {
            return agentCharacter.GetSkillValue(skill);
        }

        public override string GetMissionDebugInfoForAgent(Agent agent)
        {
            return "Debug info not supported in this model";
        }

        public override float GetDifficultyModifier()
        {
            return 1f;
        }


        public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
        {
            if (!agent.IsHuman)
            {
                return;
            }
            MissionEquipment equipment = agent.Equipment;
            //初始参数设置
            //远程武器槽位
            agentDrivenProperties.LongestRangedWeaponSlotIndex = equipment.GetLongestRangedWeaponWithAimingError(out float inaccuracy, agent);
            //远程武器不精确度
            agentDrivenProperties.LongestRangedWeaponInaccuracy = inaccuracy;
            //武器挥动参数
            agentDrivenProperties.HandlingMultiplier = 1f;
            //盾击硬直时间倍数
            agentDrivenProperties.ShieldBashStunDurationMultiplier = 1f;
            //踢腿硬直时间倍数
            agentDrivenProperties.KickStunDurationMultiplier = 1f;
            //装弹移速惩罚因数
            agentDrivenProperties.ReloadMovementPenaltyFactor = 1f;
            //武器误差, 近战武器无效
            agentDrivenProperties.WeaponInaccuracy = 0f;

            //计算武器总重量，武器越多单位速度越慢
            float totalWeightOfWeapons = equipment.GetTotalWeightOfWeapons();
            EquipmentIndex mainHandEquipmentIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            int ridingEffectiveSkill = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, DefaultSkills.Riding);
            if (mainHandEquipmentIndex != EquipmentIndex.None)
            {
                //主武器参数设置
                ItemObject primaryItem = equipment[mainHandEquipmentIndex].Item;
                float realWeaponLength = primaryItem.WeaponComponent.PrimaryWeapon.GetRealWeaponLength();
                totalWeightOfWeapons += 1.5f * primaryItem.Weight * MathF.Sqrt(realWeaponLength);
                //武器速度参数
                agentDrivenProperties.SwingSpeedMultiplier = 0.93f + 0.0007f * GetSkillValueForItem(agent, primaryItem);
                agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = agentDrivenProperties.SwingSpeedMultiplier;
                //装弹速度
                agentDrivenProperties.ReloadSpeed = 0.93f + 0.0007f * GetSkillValueForItem(agent, primaryItem);
                //远程主武器与马上武器参数设置
                RangeWeaponAndRidingSettings(equipment[mainHandEquipmentIndex].CurrentUsageItem,
                    agent, agentDrivenProperties, ridingEffectiveSkill);
            }
            EquipmentIndex offHandEquipmentIndex = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (offHandEquipmentIndex != EquipmentIndex.None)
            {
                totalWeightOfWeapons += 1.5f * equipment[offHandEquipmentIndex].Item.Weight;
            }
            agentDrivenProperties.WeaponsEncumbrance = totalWeightOfWeapons;

            //速度设置
            SpeedSettings(agent, agentDrivenProperties, equipment);

            //盾牌大小乘数
            agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;
            //骑马参数
            agentDrivenProperties.AttributeRiding = ridingEffectiveSkill * (agent.MountAgent?.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) ?? 1f);
            //骑射参数
            agentDrivenProperties.AttributeHorseArchery = Game.Current.BasicModels.StrikeMagnitudeModel.CalculateHorseArcheryFactor(agent.Character);
            //步弓手移速参数
            agentDrivenProperties.BipedalRangedReadySpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
            //步弓手装填参数
            agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);
            SetEnhancedAiRelatedProperties(agent, agentDrivenProperties);
        }

        private void SpeedSettings(Agent agent,
            AgentDrivenProperties agentDrivenProperties,
            MissionEquipment equipment)
        {
            //移速设置
            int athleticsEffectiveSkill = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, DefaultSkills.Athletics);
            //身体本身重量????
            int weight = agent.Monster.Weight;
            //盔甲与武器的总负担
            float totalEcumbrance = agentDrivenProperties.ArmorEncumbrance + equipment.GetTotalWeightOfWeapons();
            //最高速度达到时间，那这么说步兵步行也会有一个加速度过程
            //最大可能达到6.6666
            //跑动100裸奔也需要1
            agentDrivenProperties.TopSpeedReachDuration = 2f / Math.Max((200f + athleticsEffectiveSkill) / 300f * (weight / (weight + totalEcumbrance)), 0.3f);
            //环境参数，如果是雪地下雨之类的会减速10%
            float sceneFactor = 1f;
            if (!agent.Mission.Scene.IsAtmosphereIndoor && agent.Mission.Scene.GetRainDensity() > 0f)
            {
                sceneFactor *= 0.9f;
            }
            //最大速度乘数，跑动越高装备越轻速度越大就是了
            agentDrivenProperties.MaxSpeedMultiplier = sceneFactor * Math.Min((200f + athleticsEffectiveSkill) / 300f * (weight * 2f / (weight * 2f + totalEcumbrance)), 1f);
            //步行最小速度, 这看起来是个全局配置
            float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
            //步行最大速度
            float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
            float amount = Math.Min(totalEcumbrance / weight, 1f);
            agentDrivenProperties.CombatMaxSpeedMultiplier = Math.Min(MBMath.Lerp(managedParameter2, managedParameter, amount), 1f);
        }

        private void RangeWeaponAndRidingSettings(WeaponComponentData weaponComponentData,
            Agent agent,
            AgentDrivenProperties agentDrivenProperties,
            int ridingEffectiveSkill)
        {
            //主武器不为空
            if (weaponComponentData != null)
            {
                int thrustSpeed = weaponComponentData.ThrustSpeed;
                int weaponEffectiveSkill = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, weaponComponentData.RelevantSkill);
                agentDrivenProperties.WeaponInaccuracy = GetWeaponInaccuracy(agent, weaponComponentData, weaponEffectiveSkill);
                //远程武器参数设置
                if (weaponComponentData.IsRangedWeapon)
                {
                    //精度惩罚,随技能等级提升降低
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = (500 - weaponEffectiveSkill) * 0.0002f;
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = (500 - weaponEffectiveSkill) * 0.00025f;
                    //骑马提高精度惩罚
                    if (agent.HasMount)
                    {
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= Math.Max(1f, (700 - weaponEffectiveSkill - ridingEffectiveSkill) * 0.003f);
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= Math.Max(1f, (700 - weaponEffectiveSkill - ridingEffectiveSkill) * 0.0033f);
                    }
                    //弓弩标枪精度分别设置
                    else if (weaponComponentData.RelevantSkill == DefaultSkills.Bow)
                    {
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, (thrustSpeed - 45f) / 90f);
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 6f;
                    }
                    else if (weaponComponentData.RelevantSkill == DefaultSkills.Crossbow)
                    {
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.2f;
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 2.5f;
                    }
                    else if (weaponComponentData.RelevantSkill == DefaultSkills.Throwing)
                    {
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 3.5f * MBMath.Lerp(1.5f, 0.8f, (thrustSpeed - 89f) / 13f);
                    }

                    if (weaponComponentData.WeaponClass == WeaponClass.Bow)
                    {
                        //弓最佳射击时间: 准星最小的时候
                        agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - thrustSpeed) * 0.005f;
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 0.6f + weaponEffectiveSkill * 0.01f * MBMath.Lerp(2f, 4f, (thrustSpeed - 45f) / 90f);
                        //AI精度加成
                        if (agent.IsAIControlled)
                        {
                            agentDrivenProperties.WeaponUnsteadyBeginTime *= 4f;
                        }
                        agentDrivenProperties.WeaponUnsteadyEndTime = 2f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                        //旋转精度损失
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                    }
                    //投掷
                    else if (weaponComponentData.WeaponClass == WeaponClass.Javelin || weaponComponentData.WeaponClass == WeaponClass.ThrowingAxe || weaponComponentData.WeaponClass == WeaponClass.ThrowingKnife)
                    {
                        agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.4f + (89f - thrustSpeed) * 0.03f;
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 2.5f + agent.Character.GetSkillValue(weaponComponentData.RelevantSkill) * 0.01f;
                        agentDrivenProperties.WeaponUnsteadyEndTime = 10f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                    }
                    //弩
                    else
                    {
                        agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.1f;
                        agentDrivenProperties.WeaponUnsteadyBeginTime = 0f;
                        agentDrivenProperties.WeaponUnsteadyEndTime = 0f;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                    }
                }
                //可挥砍长杆参数设置
                else if (weaponComponentData.RelevantSkill == DefaultSkills.Polearm && weaponComponentData.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
                {
                    agentDrivenProperties.WeaponUnsteadyBeginTime = 1f + weaponEffectiveSkill * 0.005f;
                    agentDrivenProperties.WeaponUnsteadyEndTime = 3f + weaponEffectiveSkill * 0.01f;
                }

                //骑马装填与旋转速度惩罚
                if (agent.HasMount)
                {
                    float num4 = 1f - Math.Max(0f, 0.2f - ridingEffectiveSkill * 0.002f);
                    agentDrivenProperties.SwingSpeedMultiplier *= num4;
                    agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier *= num4;
                    agentDrivenProperties.ReloadSpeed *= num4;
                }
            }
        }

        private int GetSkillValueForItem(Agent agent, ItemObject primaryItem)
        {
            return GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, (primaryItem != null) ? primaryItem.RelevantSkill : DefaultSkills.Athletics);
        }

        private void SetEnhancedAiRelatedProperties(
                    Agent agent,
                    AgentDrivenProperties agentDrivenProperties)
        {
            float meleeAILevel = 1;
            float rangedAILevel = 1;
            float num3 = meleeAILevel + agent.Defensiveness;

            agentDrivenProperties.AIAttackOnDecideChance = 1f;
            agentDrivenProperties.AiTryChamberAttackOnDecide = (meleeAILevel - 0.15f) * 0.1f;
            agentDrivenProperties.AIAttackOnParryChance = 0.3f - 0.1f * agent.Defensiveness;
            agentDrivenProperties.AiAttackOnParryTiming = -0.2f + 0.3f * meleeAILevel;
            agentDrivenProperties.AIDecideOnAttackChance = 0.15f * agent.Defensiveness;
            agentDrivenProperties.AIParryOnAttackAbility = MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 3.0), 0f, 1f);
            agentDrivenProperties.AiKick = -0.1f + ((meleeAILevel > 0.4f) ? 0.4f : meleeAILevel);
            agentDrivenProperties.AiAttackCalculationMaxTimeFactor = meleeAILevel;
            agentDrivenProperties.AiDecideOnAttackWhenReceiveHitTiming = -0.25f * (1f - meleeAILevel);
            agentDrivenProperties.AiDecideOnAttackContinueAction = -0.5f * (1f - meleeAILevel);
            agentDrivenProperties.AiDecideOnAttackingContinue = 0.1f * meleeAILevel;
            agentDrivenProperties.AIParryOnAttackingContinueAbility = MBMath.Lerp(0.05f, 0.95f, MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 3.0), 0f, 1f));
            agentDrivenProperties.AIDecideOnRealizeEnemyBlockingAttackAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.5) - 0.1f, 0f, 1f);
            agentDrivenProperties.AiAttackingShieldDefenseChance = 0.2f + 0.3f * meleeAILevel;
            agentDrivenProperties.AiAttackingShieldDefenseTimer = -0.3f + 0.3f * meleeAILevel;
            agentDrivenProperties.AISetNoAttackTimerAfterBeingHitAbility = MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.0), 0.05f, 0.95f);
            agentDrivenProperties.AISetNoAttackTimerAfterBeingParriedAbility = MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.0), 0.05f, 0.95f);

            agentDrivenProperties.AiRangedHorsebackMissileRange = 0.3f + 0.4f * rangedAILevel;
            agentDrivenProperties.AiFacingMissileWatch = -0.96f + meleeAILevel * 0.06f;
            agentDrivenProperties.AiFlyingMissileCheckRadius = 8f - 6f * meleeAILevel;
            agentDrivenProperties.AiShootFreq = 0.3f + 0.7f * rangedAILevel;
            agentDrivenProperties.AiWaitBeforeShootFactor = (agent._propertyModifiers.resetAiWaitBeforeShootFactor ? 0f : (1f - 0.5f * rangedAILevel));
            agentDrivenProperties.AIBlockOnDecideAbility = MBMath.Lerp(0.25f, 0.99f, MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 1.0), 0f, 1f));
            agentDrivenProperties.AIParryOnDecideAbility = MBMath.Lerp(0.01f, 0.95f, MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 1.5), 0f, 1f));
            agentDrivenProperties.AIRealizeBlockingFromIncorrectSideAbility = 0.5f * MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.5) - 0.1f, 0f, 1f);
            agentDrivenProperties.AiRandomizedDefendDirectionChance = 1f - (float)Math.Log(meleeAILevel * 7.0 + 1.0, 2.0) * 0.33333f;
            agentDrivenProperties.AISetNoDefendTimerAfterHittingAbility = MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.0), 0.05f, 0.95f);
            agentDrivenProperties.AISetNoDefendTimerAfterParryingAbility = MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.0), 0.05f, 0.95f);
            agentDrivenProperties.AIEstimateStunDurationPrecision = 1f - MBMath.ClampFloat((float)Math.Pow(meleeAILevel, 2.0), 0.05f, 0.95f);
            agentDrivenProperties.AIHoldingReadyMaxDuration = MBMath.Lerp(0.25f, 0f, Math.Min(1f, meleeAILevel * 1.2f));
            agentDrivenProperties.AIHoldingReadyVariationPercentage = meleeAILevel;
            agentDrivenProperties.AiRaiseShieldDelayTimeBase = -0.75f + 0.5f * meleeAILevel;
            agentDrivenProperties.AiUseShieldAgainstEnemyMissileProbability = 0.1f + meleeAILevel * 0.6f + num3 * 0.2f;
            agentDrivenProperties.AiCheckMovementIntervalFactor = 0.005f * (1.1f - meleeAILevel);
            agentDrivenProperties.AiMovemetDelayFactor = 4f / (3f + rangedAILevel);
            agentDrivenProperties.AiParryDecisionChangeValue = 0.05f + 0.7f * meleeAILevel;
            agentDrivenProperties.AiDefendWithShieldDecisionChanceValue = Math.Min(1f, 0.2f + 0.5f * meleeAILevel + 0.2f * num3);
            agentDrivenProperties.AiMoveEnemySideTimeValue = -2.5f + 0.5f * meleeAILevel;
            agentDrivenProperties.AiMinimumDistanceToContinueFactor = 2f + 0.3f * (3f - meleeAILevel);
            agentDrivenProperties.AiStandGroundTimerValue = 0.5f * (-1f + meleeAILevel);
            agentDrivenProperties.AiStandGroundTimerMoveAlongValue = -1f + 0.5f * meleeAILevel;
            agentDrivenProperties.AiHearingDistanceFactor = 1f + meleeAILevel;
            agentDrivenProperties.AiChargeHorsebackTargetDistFactor = 1.5f * (3f - meleeAILevel);
            float num4 = 1f - rangedAILevel;
            agentDrivenProperties.AiRangerLeadErrorMin = (0f - num4) * 0.35f;
            agentDrivenProperties.AiRangerLeadErrorMax = num4 * 0.2f;
            agentDrivenProperties.AiRangerVerticalErrorMultiplier = num4 * 0.1f;
            agentDrivenProperties.AiRangerHorizontalErrorMultiplier = num4 * ((float)Math.PI / 90f);

            agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, (agent.Controller != Agent.ControllerType.Player) ? 1f : 0f);
        }
    }
}