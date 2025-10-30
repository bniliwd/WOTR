using BetterGameplay.Util;
using Kingmaker.Assets.UnitLogic.Mechanics.Properties;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System.Linq;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Modify
{
    public static class ModifyItem
    {
        public static void MultiModify()
        {
            ModifyUnthinkableTruthBuff();
            FixItemBugs();
            ModifyDisplayName();
            ModifyPiercingRodBuff();
            ModifyMR10();

            AddMagusGloves();
        }

        public static void ModifyUnthinkableTruthBuff()
        {
            //套装buff
            BlueprintBuff setBuff = GetBlueprint<BlueprintBuff>("4141bea0f4f043308237cfbd6fad87ce");

            BlueprintUnitPropertyReference FearAuraDC = CreateBlueprint<BlueprintUnitProperty>(
                BlueprintGuid.Parse("e0a60108767e4339858516242475729e"), "FearAuraDC",
                bp => {
                    bp.AddComponent<ComplexPropertyGetter>(p => {
                        p.Bonus = 25;
                        p.Property = UnitProperty.StatBonusCharisma;
                    });
                }).ToReference<BlueprintUnitPropertyReference>();

            ContextActionApplyBuff OneRoundBuff(BlueprintBuffReference buff)
            {
                return Create<ContextActionApplyBuff>(b =>
                {
                    b.m_Buff = buff;
                    b.Permanent = false;
                    b.UseDurationSeconds = false;
                    b.DurationValue = new ContextDurationValue
                    {
                        Rate = DurationRate.Rounds,
                        DiceType = 0,
                        DiceCountValue = new ContextValue(),
                        BonusValue = new ContextValue
                        {
                            ValueType = ContextValueType.Simple,
                            Value = 1
                        },
                        m_IsExtendable = false
                    };
                    b.IsFromSpell = true;
                    b.AsChild = false;
                    b.IgnoreParentContext = true;
                });
            }

            BlueprintAbilityAreaEffect fearAura = GetBlueprint<BlueprintAbilityAreaEffect>("3b0073d0b992438db2380551dfcb894e");
            fearAura.SetComponents(
                Create<AbilityAreaEffectRunAction>(ef =>
                {
                    ef.UnitEnter = CreateActionList();
                    ef.UnitExit = CreateActionList();
                    ef.UnitMove = CreateActionList();
                    ef.Round = CreateActionList(
                        Create<ContextActionSavingThrow>(c =>
                        {
                            c.Type = SavingThrowType.Will;
                            c.UseDCFromContextSavingThrow = false;
                            c.HasCustomDC = true;
                            c.CustomDC = new ContextValue
                            {
                                ValueType = ContextValueType.CasterCustomProperty,
                                m_CustomProperty = FearAuraDC
                            };
                            c.Actions = CreateActionList(
                                Create<ContextActionConditionalSaved>(pass =>
                                {
                                    pass.Succeed = CreateActionList(
                                        OneRoundBuff(GetBlueprintReference<BlueprintBuffReference>("25ec6cb6ab1845c48a95f9c20b034220"))
                                    );
                                    pass.Failed = CreateActionList(
                                        OneRoundBuff(GetBlueprintReference<BlueprintBuffReference>("f08a7239aa961f34c8301518e71d4cdf"))
                                    );
                                })
                            );
                        })
                    );
                })
            );
            //fearAura.Fx = new PrefabLink { AssetId = "227289a95d22b60408755b46e9b03d5f" };

            BlueprintBuff fearBuff = CreateBlueprint<BlueprintBuff>(
                BlueprintGuid.Parse("3d80b60db1444e69a3885b3adc6bbd73"), "FearAuraBuff",
                bp =>
                {
                    bp.m_DisplayName = "UnthinkableTruthAbility".UseLocalizedString();
                    bp.m_Description = "UnthinkableTruthAbilityDesc".UseLocalizedString();
                    bp.m_Icon = setBuff.Icon;
                    bp.IsClassFeature = false;
                    bp.m_Flags = BlueprintBuff.Flags.HiddenInUi;
                    bp.Stacking = StackingType.Replace;
                    bp.Ranks = 0;
                    bp.TickEachSecond = false;
                    bp.Frequency = DurationRate.Rounds;
                    bp.ResourceAssetIds = [];

                    bp.SetComponents(
                        new AddAreaEffect()
                        {
                            m_Flags = 0,
                            m_AreaEffect = fearAura.ToReference<BlueprintAbilityAreaEffectReference>()
                        }
                    );
                }
            );

            //可选的恐惧光环
            BlueprintActivatableAbility fear = CreateBlueprint<BlueprintActivatableAbility>(
                BlueprintGuid.Parse("078d5651e0354c0eb8350ca74d2d7b30"), "UnthinkableTruthAbility",
                bp =>
                {
                    bp.m_DisplayName = "UnthinkableTruthAbility".UseLocalizedString();
                    bp.m_Description = "UnthinkableTruthAbilityDesc".UseLocalizedString();
                    bp.m_Icon = setBuff.Icon;
                    bp.m_Buff = fearBuff.ToReference<BlueprintBuffReference>();
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
                    bp.ActionBarAutoFillIgnored = false;
                    bp.IsRuntimeOnly = false;
                    bp.HiddenInUI = false;
                    bp.ActivationType = AbilityActivationType.Immediately;
                    bp.m_ActivateWithUnitCommand = UnitCommand.CommandType.Free;
                    bp.m_ActivateOnUnitAction = AbilityActivateOnUnitActionType.Attack;
                    bp.m_SelectTargetAbility = null;
                    bp.ResourceAssetIds = [];
                }
            );

            //套装光环变成可选能力
            setBuff.SetComponents(
                new AddStatBonus()
                {
                    Descriptor = ModifierDescriptor.Competence,
                    Stat = StatType.Intelligence,
                    Value = 2,
                    ScaleByBasicAttackBonus = false
                },
                new AddStatBonus()
                {
                    Descriptor = ModifierDescriptor.Competence,
                    Stat = StatType.Wisdom,
                    Value = 2,
                    ScaleByBasicAttackBonus = false
                },
                new AddFacts()
                {
                    m_Flags = 0,
                    m_Facts = [fear.ToReference<BlueprintUnitFactReference>()],
                    DoNotRestoreMissingFacts = false,
                    HasDifficultyRequirements = false,
                    InvertDifficultyRequirements = false,
                    MinDifficulty = GameDifficultyOption.Story
                }
            );

            BlueprintFeature glass = GetBlueprint<BlueprintFeature>("9dcd9dabe32c4a298c3c3582263358b7");
            //移除眼镜的真知效果和重计算，添加恒定真知术buff
            glass.RemoveComponents<AddCondition>();
            glass.RemoveComponents<RecalculateOnStatChange>();

            glass.AddComponent(Create<AddFacts>(fact =>
            {
                fact.m_Facts = [GetBlueprintReference<BlueprintUnitFactReference>("09b4b69169304474296484c74aa12027")];
                fact.HasDifficultyRequirements = false;
            }));
        }

        public static void FixItemBugs()
        {
            //嗅血斗篷
            BlueprintFeature bloodScent = GetBlueprint<BlueprintFeature>("9db109eee03524f4aa89881c4539a14d");
            bloodScent.AddComponent<BuffExtraEffects>(bp =>
            {
                bp.m_CheckedBuff = GetBlueprintReference<BlueprintBuffReference>("3513326cd64f475781799685c57fa452");
                bp.m_ExtraEffectBuff = GetBlueprintReference<BlueprintBuffReference>("4e850d3b0c68a6045abd10e386d07af5");
            });

            BlueprintFeature morta = GetBlueprint<BlueprintFeature>("1d96c608b7a24b2ca10a0cb1dbf37813");
            morta.HideInUI = true;
            //TODO 去除此修改 各上buff代码没有不覆盖永久buff的逻辑
            morta.SetComponents(CreateAddFacts(af => af.m_Facts = [GetBlueprintReference<BlueprintUnitFactReference>("1533e782fca42b84ea370fc1dcbf4fc1")]));
        }

        public static void ModifyDisplayName()
        {
            BlueprintBuff buff = GetBlueprint<BlueprintBuff>("4e850d3b0c68a6045abd10e386d07af5");
            buff.m_DisplayName = new LocalizedString() { m_Key = "bf1ee98c-0eaf-4d30-8948-73df34b9c29a" };

            BlueprintFeature powerless5 = GetBlueprint<BlueprintFeature>("abfd1deaddbf58140b8ba0e0bbb51e46");
            powerless5.m_DisplayName = new LocalizedString() { m_Key = "a8d22745-b7ca-4029-bbbe-fdeace2ade96" };
        }

        public static void ModifyPiercingRodBuff()
        {
            PatchPiercingMetamagic("41a7f08a27e04909aea1c43cd1895260");
            PatchPiercingMetamagic("a644f678adbf4d25aae4f5b1b9d06e86");
            PatchPiercingMetamagic("84b6421e9e1b4b3e8963ff12c35aab23");

            static void PatchPiercingMetamagic(string buffId)
            {
                BlueprintBuff buff = GetBlueprint<BlueprintBuff>(buffId);
                MetamagicRodMechanics temp = buff.GetComponent<MetamagicRodMechanics>();

                if (temp != null)
                {
                    temp.Metamagic = Metamagic.Piercing;
                }
            }
        }

        public static void ModifyMR10()
        {
            //神9判断
            Condition mythicLevel9 = new IsUnitMythicLevel { 
                Not = false, 
                CheckMinLevel = true,
                MinLevel = new IntConstant { Value = 9 },
                CheckMaxLevel = true,
                MaxLevel = new IntConstant { Value = 9 },
                Unit = new PlayerCharacter() 
            };

            //原神话10补充判断
            var mr10Cue = GetBlueprint<BlueprintCue>("83fceef22bce2474a881b44ab2fb9ef2");
            GameAction[] oldActions = mr10Cue.OnStop.Actions;
            if (oldActions.ElementAtOrDefault(1) is Conditional { ConditionsChecker.Conditions: not null } old)
            {
                old.ConditionsChecker.Conditions = [.. old.ConditionsChecker.Conditions, mythicLevel9];
            }

            //能不能升10
            Conditional mythicLevelCondition = new()
            {
                ConditionsChecker = new ConditionsChecker()
                {
                    Operation = Operation.And,
                    Conditions = [mythicLevel9]
                },
                IfTrue = CreateActionList([new GainMythicLevel { Levels = 1 }]),
                IfFalse = new ActionList()
            };

            //德斯卡瑞神话道途
            var deskariAnswerList = GetBlueprint<BlueprintAnswersList>("0487039270cde774e942660950581edf");
            foreach (var answerRef in deskariAnswerList.Answers)
            {
                var answer = (BlueprintAnswer)answerRef.Get();
                if (answer == null || answer.MythicRequirement == Mythic.None || answer.MythicRequirement == Mythic.PlayerIsLegend)
                {
                    continue;
                }
                answer.OnSelect.Actions = [..answer.OnSelect.Actions, mythicLevelCondition];
            }
        }

        public static void AddMagusGloves()
        {
            BlueprintItemEquipmentGloves copy = GetBlueprint<BlueprintItemEquipmentGloves>("c3b0886a52aa5b849bf74c36f426f4b7");

            BlueprintFeature feature = CreateBlueprint<BlueprintFeature>(
                    BlueprintGuid.Parse("29774f2156c848638491722c26052e7f"), "EnduringMagusFeature",
                    bp => {
                        bp.HideInUI = true;
                        bp.SetComponents(
                            new AutoMetamagic()
                            {
                                m_Flags = 0,
                                m_AllowedAbilities = AutoMetamagic.AllowedType.Any,
                                Metamagic = Metamagic.Extend,
                                Abilities = [
                                    GetBlueprintReference<BlueprintAbilityReference>("3c89dfc82c2a3f646808ea250eb91b91"),   //奥法武器增强
                                    GetBlueprintReference<BlueprintAbilityReference>("1b7fb8120390ca24c9da98ce87780b7f"),   //奥法精准
                                    GetBlueprintReference<BlueprintAbilityReference>("98a5a678a1b69b248b672d106553ad9a"),   //奥法精准变体
                                    GetBlueprintReference<BlueprintAbilityReference>("fa12d155c229c134dbbbebf0d7b980f0"),   //远见打击
                                    GetBlueprintReference<BlueprintAbilityReference>("cf7c4eaa2b47d7242b2c734df567cefb"),   //空间斩
                                    GetBlueprintReference<BlueprintAbilityReference>("8b425e230a6224448bc30682ee596ae3")    //极速突击
                                ],
                                Descriptor = SpellDescriptor.None
                            }
                        );
                    }
                );

            BlueprintEquipmentEnchantment enchantment = CreateBlueprint<BlueprintEquipmentEnchantment>(
                    BlueprintGuid.Parse("92584bcd0d9e40c58d38ceca187af1a2"), "EnduringMagusEnchantment",
                    bp => {
                        bp.m_EnchantmentCost = 0;
                        bp.m_IdentifyDC = 5;
                        bp.SetComponents(
                            new AddUnitFeatureEquipment() { m_Feature = feature.ToReference<BlueprintFeatureReference>() }
                        );
                    }
                );

            CreateBlueprint<BlueprintItemEquipmentGloves>(
                    BlueprintGuid.Parse("09c9307b378b4a028fd4065d7035dfb2"), "GlovesOfEnduringMagusItem",
                    bp => {
                        bp.m_DisplayNameText = "GlovesOfEnduringMagusItem".UseLocalizedString();
                        bp.m_DescriptionText = "GlovesOfEnduringMagusItemDesc".UseLocalizedString();
                        bp.m_Icon = copy.m_Icon;
                        bp.m_Cost = 62000;
                        bp.m_Weight = 1.0f;
                        bp.m_IsNotable = false;
                        bp.m_ForceStackable = false;
                        bp.m_Destructible = true;
                        bp.m_ShardItem = copy.m_ShardItem;
                        bp.m_MiscellaneousType = copy.m_MiscellaneousType;
                        bp.m_InventoryPutSound = copy.m_InventoryPutSound;
                        bp.m_InventoryTakeSound = copy.m_InventoryTakeSound;
                        bp.TrashLootTypes = [];
                        bp.CR = 13;
                        bp.m_EquipmentEntity = copy.m_EquipmentEntity;
                        bp.m_EquipmentEntityAlternatives = [];
                        bp.m_ForcedRampColorPresetIndex = 0;
                        bp.m_Enchantments = [enchantment.ToReference<BlueprintEquipmentEnchantmentReference>()];
                        bp.m_InventoryEquipSound = copy.m_InventoryEquipSound;

                        bp.SetComponents(
                            new EquipmentRestrictionClass() { m_Class = GetBlueprintReference<BlueprintCharacterClassReference>("45a4607686d96a1498891b3286121780"), Not = false }
                        );
                    }
                );
        }
    }

}
