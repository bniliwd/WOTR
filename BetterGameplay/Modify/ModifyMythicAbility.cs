using BetterGameplay.Logic;
using BetterGameplay.Util;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using static BetterGameplay.Logic.UnitPartCustomMechanicsFeatures;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Modify
{
    public static class ModifyMythicAbility
    {
        public static void MultiModify()
        {
            ModifyEnduringSpells();
            ModifyDimensionalRetribution();
            ModifyElementalBarrage();
            ModifyTricksterPerception3();
            AddMythicHuntersBond();
        }

        public static void ModifyEnduringSpells()
        {
            var enduringSpells = GetBlueprint<BlueprintFeature>("2f206e6d292bdfb4d981e99dcf08153f");
            enduringSpells.SetComponents(
                new PrerequisiteFeature()
                {
                    m_Feature = GetBlueprintReference<BlueprintFeatureReference>("f180e72e4a9cbaa4da8be9bc958132ef")
                },
                new EnduringSpellsRe()
                {
                    m_Greater = GetBlueprintReference<BlueprintUnitFactReference>("13f9269b3b48ae94c896f0371ce5e23c")
                }
            );
        }

        public static void ModifyDimensionalRetribution()
        {
            var DimensionalRetribution = GetBlueprint<BlueprintFeature>("939f49ad995ee8d4fad03ad0c7f655d1");
            var Dweomerleap = GetBlueprint<BlueprintAbility>("cde8c0c172c9fa34cba7703ba4824d32");
            var MidnightFane_DimensionLock_Buff = GetBlueprint<BlueprintBuff>("4b0cd08a3cea2844dba9889c1d34d667");

            var dra = CreateBlueprint<BlueprintAbility>(
                BlueprintGuid.Parse("f8b4430be6904ee9b25320c00d0082b2"), "DimensionalRetributionAbility", 
                bp => {
                    bp.m_DisplayName = DimensionalRetribution.m_DisplayName;
                    bp.m_Description = DimensionalRetribution.m_Description;
                    bp.LocalizedDuration = LocaleStringUtils.Empty;
                    bp.LocalizedSavingThrow = LocaleStringUtils.Empty;
                    bp.m_Icon = DimensionalRetribution.Icon;
                    bp.Type = AbilityType.Supernatural;
                    bp.Range = AbilityRange.Unlimited;
                    bp.CanTargetPoint = false;
                    bp.CanTargetEnemies = true;
                    bp.CanTargetFriends = true;
                    bp.CanTargetSelf = false;
                    bp.Hidden = false;
                    bp.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
                    bp.ActionType = UnitCommand.CommandType.Swift;
                    bp.ResourceAssetIds = Dweomerleap.ResourceAssetIds;
                    var DweomerleapComponent = Dweomerleap.GetComponent<AbilityCustomDweomerLeap>();
                    bp.AddComponent<AbilityCustomDimensionalRetribution>(c => {
                        c.m_CasterDisappearProjectile = DweomerleapComponent.m_CasterDisappearProjectile;
                        c.m_CasterAppearProjectile = DweomerleapComponent.m_CasterAppearProjectile;
                        c.m_SideDisappearProjectile = DweomerleapComponent.m_SideDisappearProjectile;
                        c.m_SideAppearProjectile = DweomerleapComponent.m_SideAppearProjectile;
                        c.PortalFromPrefab = DweomerleapComponent.PortalFromPrefab;
                        c.PortalToPrefab = DweomerleapComponent.PortalToPrefab;
                        c.PortalBone = DweomerleapComponent.PortalBone;
                        c.CasterDisappearFx = DweomerleapComponent.CasterDisappearFx;
                        c.CasterAppearFx = DweomerleapComponent.CasterAppearFx;
                        c.SideDisappearFx = DweomerleapComponent.SideDisappearFx;
                        c.SideAppearFx = DweomerleapComponent.SideAppearFx;
                    });
                });

            var drb = GetBlueprint<BlueprintBuff>("eb322aef2fe82b14d8e88a5b3eea8d34");
            drb.m_Icon = dra.Icon;
            drb.m_DisplayName = dra.m_DisplayName;
            drb.m_Description = dra.m_Description;
            drb.AddComponent<DimensionalRetributionLogic>(c => {
                c.m_Ability = dra.ToReference<BlueprintAbilityReference>();
            });

            var drta = CreateBlueprint<BlueprintActivatableAbility>(
                BlueprintGuid.Parse("12686a3808b1454ca09499e9470b1d0e"), "DimensionalRetributionToggleAbility", 
                bp => {
                    bp.m_Icon = dra.Icon;
                    bp.m_DisplayName = dra.m_DisplayName;
                    bp.m_Description = dra.m_Description;
                    bp.m_Buff = drb.ToReference<BlueprintBuffReference>();
                    bp.IsOnByDefault = true;
                    bp.DoNotTurnOffOnRest = true;
                    bp.DeactivateImmediately = true;
                });

            AddFacts af = DimensionalRetribution.GetComponent<AddFacts>();
            if (af != null)
            {
                af.m_Facts = [drta.ToReference<BlueprintUnitFactReference>()];
            }

            var forbiddenSpells = MidnightFane_DimensionLock_Buff.GetComponent<ForbidSpecificSpellsCast>();
            forbiddenSpells.m_Spells = [.. forbiddenSpells.m_Spells, dra.ToReference<BlueprintAbilityReference>()];
        }

        static void ModifyElementalBarrage()
        {
            var ElementalBarrage = GetBlueprint<BlueprintFeature>("da56a1b21032a374783fdf46e1a92adb");
            var ElementalBarrageBuff = CreateBlueprint<BlueprintBuff>(
                BlueprintGuid.Parse("af5a4973e57e4ee3bb4978d65935dde0"), "ElementalBarrageBuff",
                bp =>
                {
                    bp.m_DisplayName = ElementalBarrage.m_DisplayName;
                    bp.m_Description = LocaleStringUtils.Empty;
                    bp.m_Icon = ElementalBarrage.Icon;
                    bp.m_Flags = 0;
                }
            );

            var refer = ElementalBarrageBuff.ToReference<BlueprintBuffReference>();

            ElementalBarrage.SetComponents();
            ElementalBarrage.AddComponent<ElementalBarrageOutgoingTrigger>(c =>
            {
                c.IgnoreDamageFromThisFact = true;
                c.ApplyToAreaEffectDamage = true;
                c.CheckAbilityType = true;
                c.m_AbilityType = AbilityType.Spell;
                c.CheckDamageDealt = true;
                c.CompareType = CompareOperation.Type.Greater;
                c.TargetValue = 0;
                c.m_ElementalBarrageBuff = refer;
                c.MarkDuration = new ContextDurationValue()
                {
                    m_IsExtendable = false,
                    DiceCountValue = new ContextValue(),
                    BonusValue = 3
                };
                c.TriggerActions = CreateActionList(
                    Create<ContextActionDealDamage>(a =>
                    {
                        a.DamageType = new DamageTypeDescription()
                        {
                            Type = DamageType.Energy,
                            Energy = DamageEnergyType.Divine
                        };
                        a.Duration = new ContextDurationValue()
                        {
                            DiceCountValue = new ContextValue(),
                            BonusValue = new ContextValue()
                        };
                        a.Value = new ContextDiceValue()
                        {
                            DiceType = DiceType.D6,
                            DiceCountValue = new ContextValue()
                            {
                                ValueType = ContextValueType.CasterProperty,
                                Property = UnitProperty.MythicLevel
                            },
                            BonusValue = 0
                        };
                        a.IgnoreCritical = true;
                        a.SetFactAsReason = true;
                    }),
                    Create<ContextActionDisableBonusForDamage>(a =>
                    {
                        a.DisableSneak = true;
                        a.DisableAdditionalDamage = true;
                        a.DisableFavoredEnemyDamage = true;
                        a.DisableAdditionalDice = true;
                    })
                );
            });

            ElementalBarrageBuff.AddComponent<ElementalBarrageIncomingTrigger>(c =>
            {
                c.IgnoreDamageFromThisFact = true;
                c.CheckEnergyDamageType = true;
                c.CheckDamageDealt = false;
                c.CompareType = CompareOperation.Type.Greater;
                c.TargetValue = 0;
                c.EnergyType = 0;
                c.EnergyTypes = [DamageEnergyType.Fire, DamageEnergyType.Cold, DamageEnergyType.Sonic, DamageEnergyType.Electricity, DamageEnergyType.Acid];
                c.TriggerActions = CreateActionList(
                    Create<ContextActionDealDamage>(a =>
                    {
                        a.DamageType = new DamageTypeDescription()
                        {
                            Type = DamageType.Energy,
                            Energy = DamageEnergyType.Divine
                        };
                        a.Duration = new ContextDurationValue()
                        {
                            DiceCountValue = new ContextValue(),
                            BonusValue = new ContextValue()
                        };
                        a.Value = new ContextDiceValue()
                        {
                            DiceType = DiceType.D6,
                            DiceCountValue = new ContextValue()
                            {
                                ValueType = ContextValueType.CasterProperty,
                                Property = UnitProperty.MythicLevel
                            },
                            BonusValue = 0
                        };
                        a.IgnoreCritical = true;
                        a.SetFactAsReason = true;
                    }),
                    Create<ContextActionRemoveSelf>()
                );
            });
        }

        static void ModifyTricksterPerception3()
        {
            var TricksterPerceptionTier3Feature = GetBlueprint<BlueprintFeature>("c785d2718021449f895a960c7840b4d0");
            TricksterPerceptionTier3Feature.TemporaryContext(bp => {
                bp.AddComponent<AddCustomMechanicsFeature>(c => {
                    c.Feature = CustomMechanicsFeature.BypassCriticalHitImmunity;
                });
                bp.AddComponent<AddCustomMechanicsFeature>(c => {
                    c.Feature = CustomMechanicsFeature.BypassSneakAttackImmunity;
                });
            });
        }

        static void AddMythicHuntersBond()
        {
            var HuntersBondFeature = GetBlueprint<BlueprintFeature>("6dddf5ba2291f41498df2df7f8fa2b35");
            var HuntersBondAbility = GetBlueprint<BlueprintAbility>("cd80ea8a7a07a9d4cb1a54e67a9390a5");
            var HuntersBondBuff = GetBlueprint<BlueprintBuff>("2f93cad6b132aac4e80728d7fa03a8aa");

            var MythicBondBuff = CreateBlueprint<BlueprintBuff>(
                BlueprintGuid.Parse("55abd77cc891485c9a56ca1399e6f549"), "MythicBondBuff", 
                    bp => {
                    bp.m_DisplayName = "MythicBond".UseLocalizedString();
                    bp.m_Description = "MythicBondBuffDesc".UseLocalizedString();
                    bp.m_Icon = HuntersBondAbility.Icon;
                    bp.m_Flags = HuntersBondBuff.m_Flags;
                    bp.Ranks = 1;
                    bp.IsClassFeature = true;
                    bp.AddComponent<ShareFavoredEnemies>(c => { c.Half = false; });
                });

            var MythicBondFeature = CreateBlueprint<BlueprintFeature>(
                BlueprintGuid.Parse("9f8a6494bbf14658bbfb100acf7b5ae9"), "MythicBondFeature", bp => {
                bp.m_DisplayName = "MythicBond".UseLocalizedString();
                bp.m_Description = "MythicBondDesc".UseLocalizedString();
                bp.m_Icon = HuntersBondAbility.Icon;
                bp.Ranks = 1;
                bp.IsClassFeature = true;
                bp.ReapplyOnLevelUp = true;
                bp.Groups = [FeatureGroup.MythicAbility];
                bp.AddComponent<AutoMetamagic>(c => {
                    c.m_AllowedAbilities = AutoMetamagic.AllowedType.Any;
                    c.m_IncludeClasses = [];
                    c.m_ExcludeClasses = [];
                    c.Metamagic = Metamagic.Quicken;
                    c.Abilities = [HuntersBondAbility.ToReference<BlueprintAbilityReference>()];
                });
                bp.AddPrerequisiteFeature(HuntersBondFeature);
            });

            HuntersBondAbility.TemporaryContext(bp => {
                bp.RemoveComponents<AbilityEffectRunAction>();
                bp.AddComponent<AbilityEffectRunAction>(c => {
                    c.Actions = CreateActionList(
                        new Conditional()
                        {
                            ConditionsChecker = new ConditionsChecker()
                            {
                                Conditions = new Condition[] {
                                    new ContextConditionCasterHasFact() {
                                        m_Fact = MythicBondFeature.ToReference<BlueprintUnitFactReference>()
                                    }
                                }
                            },
                            IfTrue = CreateActionList(
                                new ContextActionApplyBuff()
                                {
                                    m_Buff = MythicBondBuff.ToReference<BlueprintBuffReference>(),
                                    DurationValue = new ContextDurationValue()
                                    {
                                        DiceCountValue = 0,
                                        BonusValue = new ContextValue()
                                        {
                                            ValueType = ContextValueType.Rank
                                        }
                                    }
                                }
                            ),
                            IfFalse = CreateActionList(
                                new ContextActionApplyBuff()
                                {
                                    m_Buff = HuntersBondBuff.ToReference<BlueprintBuffReference>(),
                                    DurationValue = new ContextDurationValue()
                                    {
                                        DiceCountValue = 0,
                                        BonusValue = new ContextValue()
                                        {
                                            ValueType = ContextValueType.Rank
                                        }
                                    }
                                }
                            ),
                        }
                    );
                });
            });

            BlueprintFeatureSelection select = GetBlueprint<BlueprintFeatureSelection>("ba0e5a900b775be4a99702f1ed08914d");
            select.m_AllFeatures = select.m_AllFeatures.AppendToArray(MythicBondFeature.ToReference<BlueprintFeatureReference>());
        }
    }
}