using BetterGameplay.Logic;
using BetterGameplay.Util;
using Kingmaker;
using Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.NewContent
{
    public static class FreedomGift
    {
        public static List<AaCombine> ParseCombines(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<AaCombine>>(jsonContent);
            }
            catch (Exception) { PFLog.Mods.Error("读取本地文本失败"); }

            return [];
        }

        public static void AddFreedomGift()
        {
            string path = Path.Combine(Main.ModPath, "Configuration", "ArueshalaeAbilities.json");
            var data = ParseCombines(path);

            BuildMainPart(data);
            ModifyUpgrader();
        }

        public static void ModifyUpgrader()
        {
            if (ResourcesLibrary.TryGetBlueprint(BlueprintGuid.Parse("e899419ccb524c368177c2843f208025")) is BlueprintPlayerUpgrader upgrader)
            {
                CompanionInParty Arueshalae = new()
                    {
                        m_Companion = GetBlueprintReference<BlueprintUnitReference>("a352873d37ec6c54c9fa8f6da3a6b3e1"),
                        IncludeRemote = true,
                        IncludeExCompanions = true,
                        IncludeDettached = true
                    };

                upgrader.m_Actions = CreateActionList(
                    new Conditional
                    {
                        ConditionsChecker = new ConditionsChecker
                        {
                            Operation = Operation.And,
                            Conditions = [
                                new DialogSeen{
                                    Not = false,
                                    m_Dialog = GetBlueprintReference<BlueprintDialogReference>("173a2abcfc9a1574695b6759097746fa")
                                }
                            ]
                        },
                        IfTrue = CreateActionList(
                            [
                                new AddFact{
                                    Unit = Arueshalae,
                                    m_Fact = GetBlueprintReference<BlueprintUnitFactReference>("0142e98b4f244bb083ba2a8bba6a8072")
                                },
                                new AddFact{
                                    Unit = Arueshalae,
                                    m_Fact = GetBlueprintReference<BlueprintUnitFactReference>("27f5383d73e84e589ec0ee1cfc3d0e2e")
                                },
                                new RemoveFact{
                                    Unit = Arueshalae,
                                    m_Fact = GetBlueprintReference<BlueprintUnitFactReference>("fe3e941559915d542896a66ad7d4d695")
                                }
                            ]),
                        IfFalse = CreateActionList()
                    });
            }
        }

        public static BlueprintFeature BuildMainPart(List<AaCombine> combines)
        {
            BlueprintAbilityResource resource = CreateBlueprint<BlueprintAbilityResource>(
                BlueprintGuid.Parse("b708438045724165a90e6c82309ac198"), "FreedomGiftResource",
                bp =>
                {
                    bp.m_MaxAmount = new BlueprintAbilityResource.Amount {
                        BaseValue = 1,
                        IncreasedByLevel = false,
                        IncreasedByStat = false
                    };
                });

            BlueprintAbility copy = GetBlueprint<BlueprintAbility>("9af0b584f6f754045a0a79293d100ab3");

            BlueprintAbility ability = CreateBlueprint<BlueprintAbility>(
                BlueprintGuid.Parse("0bb8e9be7af245559c0b58360a01050a"), "FreedomGiftAbility",
                bp =>
                {
                    bp.m_DisplayName = "FreedomGiftFeature".UseLocalizedString();
                    bp.m_Description = "FreedomGiftDesc".UseLocalizedString();
                    bp.m_Icon = copy.m_Icon;
                    bp.Type = AbilityType.Special;
                    bp.Range = AbilityRange.Touch;
                    bp.CanTargetFriends = true;
                    bp.CanTargetSelf = false;
                    bp.SpellResistance = false;
                    bp.EffectOnAlly = AbilityEffectOnUnit.Helpful;
                    bp.ActionType = UnitCommand.CommandType.Standard;
                    bp.m_IsFullRoundAction = true;
                    bp.LocalizedDuration = new LocalizedString() { Key = "0b5bb39b-9e2e-4841-9f1c-5c20c306553b" };
                    bp.LocalizedSavingThrow = new LocalizedString() { Key = "" };
                });

            ability.SetComponents(
                new AbilityVariants()
                {
                    m_Variants = GetChildBlueprintAbilities(combines, ability.ToReference<BlueprintAbilityReference>())
                },
                new AbilityResourceLogic()
                {
                    m_RequiredResource = resource.ToReference<BlueprintAbilityResourceReference>(),
                    m_IsSpendResource = true,
                    Amount = 1
                },
                new HideDCFromTooltip()
            );

            return CreateBlueprint<BlueprintFeature>(
                BlueprintGuid.Parse("27f5383d73e84e589ec0ee1cfc3d0e2e"), "FreedomGiftFeature",
                bp =>
                {
                    bp.m_DisplayName = "FreedomGiftFeature".UseLocalizedString();
                    bp.m_Description = "FreedomGiftFeatureDesc".UseLocalizedString();
                    bp.m_Icon = null;
                    bp.HideInUI = false;
                    bp.HideInCharacterSheetAndLevelUp = true;
                    bp.HideNotAvailibleInUI = false;
                    bp.ReapplyOnLevelUp = false;
                    bp.IsClassFeature = false;

                    bp.SetComponents(
                        new AddFacts() {m_Facts = [ability.ToReference<BlueprintUnitFactReference>()] },
                        new AddAbilityResources() { m_Resource = resource.ToReference<BlueprintAbilityResourceReference>(), RestoreAmount = true }
                    );
                    
                });
        }

        public static BlueprintAbilityReference[] GetChildBlueprintAbilities(List<AaCombine> combines, BlueprintAbilityReference parent)
        {
            BlueprintAbilityReference[] result = new BlueprintAbilityReference[combines.Count];
            BlueprintBuff icon = GetBlueprint<BlueprintBuff>("7ad9d9982302e2244a7dd73fee6c381b");
            for (int i = 0; i < result.Length; i++)
            {
                HashSet<BlueprintGuid> conflict = [.. combines.Where(combine => combine != combines[i]).Select(combine => combine.BuffGuid)];
                result[i] = BuildChildBlueprintAbility(combines[i], conflict, parent, icon).ToReference<BlueprintAbilityReference>();
            }

            return result;
        }

        public static BlueprintAbility BuildChildBlueprintAbility(AaCombine combine, HashSet<BlueprintGuid> toRemove, BlueprintAbilityReference parent, BlueprintBuff icon)
        {
            BlueprintBuff buff = CreateBlueprint<BlueprintBuff>(
                combine.BuffGuid, combine.DisplayName + "Buff",
                bp =>
                {
                    bp.m_DisplayName = "FreedomGift".UseLocalizedString();
                    bp.m_Description = "FreedomGiftDesc".UseLocalizedString();
                    bp.m_Icon = icon?.m_Icon;
                    bp.IsClassFeature = false;
                    bp.m_Flags = BlueprintBuff.Flags.StayOnDeath;
                    bp.Stacking = StackingType.Replace;
                    bp.TickEachSecond = false;
                    bp.Frequency = DurationRate.Rounds;

                    bp.SetComponents(
                        new AddStatBonus()
                        {
                            Descriptor = ModifierDescriptor.Sacred,
                            Stat = combine.StatType,
                            Value = 2,
                            ScaleByBasicAttackBonus = false
                        }
                    );
                });

            BlueprintAbility copy = GetBlueprint<BlueprintAbility>(combine.CopyGuid);

            return CreateBlueprint<BlueprintAbility>(
                combine.BpGuid, combine.DisplayName,
                bp =>
                {
                    bp.m_DisplayName = combine.DisplayName.UseLocalizedString();
                    bp.m_Description = new LocalizedString() { Key = "" };
                    bp.m_Icon = copy?.Icon;
                    bp.Type = AbilityType.Special;
                    bp.Range = AbilityRange.Touch;
                    bp.CanTargetFriends = true;
                    bp.CanTargetSelf = false;
                    bp.EffectOnAlly = AbilityEffectOnUnit.Helpful;
                    bp.m_Parent = parent;
                    bp.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
                    bp.ActionType = UnitCommand.CommandType.Standard;
                    bp.m_IsFullRoundAction = true;
                    bp.LocalizedDuration = new LocalizedString() { Key = "" };
                    bp.LocalizedSavingThrow = new LocalizedString() { Key = "" };
                    bp.ResourceAssetIds = [];

                    bp.SetComponents(
                        new AbilityEffectRunAction()
                        {
                            Actions = CreateActionList(
                                new ContextActionApplyBuff()
                                {
                                    m_Buff = buff.ToReference<BlueprintBuffReference>(),
                                    Permanent = true,
                                    IsFromSpell = false,
                                    IsNotDispelable = true,
                                    ToCaster = false,
                                    AsChild = false,
                                    SameDuration = false
                                },
                                new ContextActionRemoveBuffs() { Buffs = toRemove }
                            )
                        },
                        new AbilitySpawnFx
                        {
                            PrefabLink = new PrefabLink{ AssetId = "e23fec8d2024a8c48a8b4a57693e31a7" },
                            Time = AbilitySpawnFxTime.OnApplyEffect,
                            Anchor = AbilitySpawnFxAnchor.SelectedTarget,
                            OrientationMode = AbilitySpawnFxOrientation.Copy
                        },
                        new HideDCFromTooltip()
                    );
                });
        }
        
    }

    public class AaCombine
    {
        public string BpId { get; set; }

        public BlueprintGuid BpGuid { get; }

        public string BuffId { get; set; }

        public BlueprintGuid BuffGuid { get; }

        public string CopyId { get; set; }

        public BlueprintGuid CopyGuid { get; }

        public StatType StatType { get; set; }

        public string DisplayName { get; set; }

        public AaCombine() {}

        [JsonConstructor]
        public AaCombine(string bpId, string buffId, string copyId, StatType statType, string displayName)
        {
            BpId = bpId;
            BpGuid = BlueprintGuid.Parse(bpId);
            BuffId = buffId;
            BuffGuid = BlueprintGuid.Parse(buffId);
            CopyId = copyId;
            CopyGuid = BlueprintGuid.Parse(copyId);
            StatType = statType;
            DisplayName = displayName;
        }
    }
}