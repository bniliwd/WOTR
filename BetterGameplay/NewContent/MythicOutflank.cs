using BetterGameplay.Logic;
using BetterGameplay.Util;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.NewContent
{
    public static class MythicOutflank
    {
        public static void AddMythicOutflank()
        {
            BlueprintFeatureReference outflank = GetBlueprintReference<BlueprintFeatureReference>("422dab7309e1ad343935f33a4d6e9f11");

            BlueprintBuff enemyBuff = MythicOutFlankEnemyBuff(outflank);
            BlueprintAbilityAreaEffect areaBuffEffect = MythicOutFlankAreaBuffEffect(enemyBuff.ToReference<BlueprintBuffReference>());
            BlueprintBuff areaBuff = MythicOutFlankAreaBuff(areaBuffEffect.ToReference<BlueprintAbilityAreaEffectReference>());
            BlueprintActivatableAbility ability = MythicOutFlankAbility(areaBuff.ToReference<BlueprintBuffReference>());
            BlueprintFeature mythicOutflank = MythicOutFlankFeature([ability.ToReference<BlueprintUnitFactReference>()]);

            BlueprintFeatureSelection select = GetBlueprint<BlueprintFeatureSelection>("ba0e5a900b775be4a99702f1ed08914d");
            select.m_AllFeatures = select.m_AllFeatures.AppendToArray(mythicOutflank.ToReference<BlueprintFeatureReference>());
        }

        public static BlueprintFeature MythicOutFlankFeature(BlueprintUnitFactReference[] facts)
        {
            BlueprintFeature swift = GetBlueprint<BlueprintFeature>("93e78cad499b1b54c859a970cbe4f585");

            return CreateBlueprint<BlueprintFeature>(
                BlueprintGuid.Parse("32a6e2d21e714c0089e6b62caf60b9e2"), "MythicOutflank",
                bp =>
                {
                    bp.m_DisplayName = "MythicOutflank".UseLocalizedString();
                    bp.m_Description = "MythicOutflankDesc".UseLocalizedString();
                    bp.m_Icon = swift.m_Icon;
                    bp.HideInUI = false;
                    bp.HideInCharacterSheetAndLevelUp = false;
                    bp.HideNotAvailibleInUI = false;
                    bp.Groups = [FeatureGroup.MythicAbility];
                    bp.ReapplyOnLevelUp = false;
                    bp.IsClassFeature = true;
                    bp.IsPrerequisiteFor = null;

                    bp.SetComponents(
                        new PrerequisiteFeature()
                        {
                            m_Feature = GetBlueprintReference<BlueprintFeatureReference>("422dab7309e1ad343935f33a4d6e9f11"),
                            Group = Prerequisite.GroupType.All
                        }, 
                        new PrerequisiteFeature()
                        {
                            m_Feature = GetBlueprintReference<BlueprintFeatureReference>("0da0c194d6e1d43419eb8d990b28e0ab"),
                            Group = Prerequisite.GroupType.All
                        },
                        new AddFacts()
                        {
                            m_Flags = 0,
                            m_Facts = facts,
                            DoNotRestoreMissingFacts = false,
                            HasDifficultyRequirements = false,
                            InvertDifficultyRequirements = false,
                            MinDifficulty = GameDifficultyOption.Story
                        }
                    );
                });
        }

        public static BlueprintActivatableAbility MythicOutFlankAbility(BlueprintBuffReference buff)
        {
            BlueprintFeature hammerTheGap = GetBlueprint<BlueprintFeature>("7b64641c76ff4a744a2bce7f91a20f9a");

            return CreateBlueprint<BlueprintActivatableAbility>(
                BlueprintGuid.Parse("4e298a240c6546a991189b686b1a6869"), "MythicOutflankAbility",
                bp =>
                {
                    bp.m_DisplayName = "MythicOutflankAbility".UseLocalizedString();
                    bp.m_Description = "MythicOutflankAbilityDesc".UseLocalizedString();
                    bp.m_Icon = hammerTheGap.m_Icon;
                    bp.m_Buff = buff;
                    bp.Group = ActivatableAbilityGroup.None;
                    bp.WeightInGroup = 1;
                    bp.IsOnByDefault = false;
                    bp.DeactivateIfCombatEnded = false;
                    bp.DeactivateAfterFirstRound = false;
                    bp.DeactivateImmediately = true;
                    bp.IsTargeted = false;
                    bp.DeactivateIfOwnerDisabled = false;
                    bp.DeactivateIfOwnerUnconscious = false;
                    bp.OnlyInCombat = false;
                    bp.DoNotTurnOffOnRest = false;
                    bp.ActivationType = AbilityActivationType.Immediately;
                    bp.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;
                    bp.m_ActivateOnUnitAction = AbilityActivateOnUnitActionType.Attack;
                    bp.ResourceAssetIds = [];
                });
        }

        public static BlueprintBuff MythicOutFlankAreaBuff(BlueprintAbilityAreaEffectReference effect)
        {
            return CreateBlueprint<BlueprintBuff>(
                BlueprintGuid.Parse("dd31d1c7c316455d9e237a6b053ac316"), "MythicOutflankAreaBuff",
                bp =>
                {
                    bp.m_Icon = null;
                    bp.IsClassFeature = false;
                    bp.m_Flags = BlueprintBuff.Flags.HiddenInUi;
                    bp.Stacking = StackingType.Replace;
                    bp.Ranks = 0;
                    bp.TickEachSecond = false;
                    bp.Frequency = DurationRate.Rounds;
                    bp.ResourceAssetIds = [];

                    bp.SetComponents(new AddAreaEffect{ m_AreaEffect = effect });
                });
        }

        public static BlueprintAbilityAreaEffect MythicOutFlankAreaBuffEffect(BlueprintBuffReference buff)
        {
            return CreateBlueprint<BlueprintAbilityAreaEffect>(
                BlueprintGuid.Parse("20ca54f9023f416ebc7555e8877cbf72"), "MythicOutflankAreaBuffEffect",
                bp =>
                {
                    bp.Comment = "";
                    bp.m_AllowNonContextActions = false;
                    bp.m_TargetType = BlueprintAbilityAreaEffect.TargetType.Enemy;
                    bp.SpellResistance = false;
                    bp.AffectEnemies = true;
                    bp.AggroEnemies = false;
                    bp.AffectDead = false;
                    bp.IgnoreSleepingUnits = false;
                    bp.Shape = AreaEffectShape.Cylinder;
                    bp.Size = new Feet(30);
                    bp.Fx = null;
                    bp.CanBeUsedInTacticalCombat = false;
                    bp.m_SizeInCells = 0;

                    bp.SetComponents(
                        new AbilityAreaEffectRunAction()
                        {
                            UnitEnter = CreateActionList(
                                Create<ContextActionApplyBuff>(a =>
                                {
                                    a.m_Buff = buff;
                                    a.Permanent = true;
                                    a.IsFromSpell = false;
                                    a.IsNotDispelable = true;
                                    a.ToCaster = false;
                                    a.AsChild = false;
                                })
                            ),
                            UnitExit = CreateActionList(
                                Create<ContextActionRemoveBuff>(a=> a.m_Buff = buff)
                            ),
                            UnitMove = CreateActionList(),
                            Round = CreateActionList()
                        }
                    );
                });
        }

        public static BlueprintBuff MythicOutFlankEnemyBuff(BlueprintFeatureReference checkedFact)
        {
            return CreateBlueprint<BlueprintBuff>(
                BlueprintGuid.Parse("0bd1b4081519427e8c494632df615e4b"), "MythicOutflankEnemyBuff",
                bp =>
                {
                    bp.m_Icon = null;
                    bp.IsClassFeature = false;
                    bp.m_Flags = BlueprintBuff.Flags.HiddenInUi | BlueprintBuff.Flags.RemoveOnRest;
                    bp.Stacking = StackingType.Replace;
                    bp.Ranks = 0;
                    bp.TickEachSecond = false;
                    bp.Frequency = DurationRate.Rounds;
                    bp.ResourceAssetIds = [];

                    bp.SetComponents(
                        new AddTargetAttackWithWeaponTriggerRe()
                        {
                            TriggerBeforeAttack = false,
                            OnlyMelee = true,
                            OnlyHit = true,
                            OnAttackOfOpportunity = true,
                            isFlanked = true,
                            m_CheckedFact = checkedFact,
                            Action = CreateActionList(
                                Create<ContextActionRangedAoO>()
                            )
                        }
                    );
                });
        }

    }
}