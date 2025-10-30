using BetterGameplay.Logic;
using BetterGameplay.Util;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.NewContent
{
    public static class SentientFinnean
    {
        //能力蓝图id，附魔蓝图id，消耗点数
        public static LinkedList<EnchantCombine> activedAbilities = [];
        public static bool checkActived = true;

        //附魔id，特性id，主能力id
        public static readonly BpCombine Level1IdGroup = new("828e8ef9cb7b474e89303b41444c22bf", "89b20c6b03994907a33f262441eab9a5", "1f29fcd85f7c4ff585033375fd997bbc");
        public static readonly BpCombine Level2IdGroup = new("af53320839af4b5b9e9bf2e4be918561", "2ca1e54b4941467cade83e8067fa24e6", "e93455e1b563424997fb701a04e46555");
        public static readonly BpCombine Level3IdGroup = new("f40486877051449f811b0ba17104ff53", "b6be1521123b449b8e8ad9791bc6979c", "b27af38ec8714361964f262e12ff80c9");

        public static Dictionary<string, List<BpCombine>> ParseLevels(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var result = new Dictionary<string, List<BpCombine>>();

                JObject jsonObj = JObject.Parse(json);

                foreach (var property in jsonObj.Properties())
                {
                    if (property.Value is JArray array)
                    {
                        result[property.Name] = array.ToObject<List<BpCombine>>();
                    }
                }
                return result;
            }
            catch (Exception) { PFLog.Mods.Error("读取本地文本失败"); }

            return [];
        }

        public static void ModifyFinneanEnchantment()
        {
            string path = Path.Combine(Main.ModPath, "Configuration", "FinneanAbilities.json");
            var data = ParseLevels(path);

            data.TryGetValue("AbilityVariantsLevel1", out List<BpCombine> AbilityVariantsLevel1);
            data.TryGetValue("AbilityVariantsLevel2", out List<BpCombine> level2Temp);
            data.TryGetValue("AbilityVariantsLevel3", out List<BpCombine> level3Temp);

            AbilityVariantsLevel1 ??= [];
            level2Temp ??= [];
            level3Temp ??= [];

            var AbilityVariantsLevel2 = AbilityVariantsLevel1.Concat(level2Temp).ToList();
            var AbilityVariantsLevel3 = AbilityVariantsLevel2.Concat(level3Temp).ToList();

            BlueprintHiddenItem Finnean = GetBlueprint<BlueprintHiddenItem>("95c126deb99ba054aa5b84710520c035");

            BlueprintWeaponEnchantmentReference Enchantment1 = GetBlueprintReference<BlueprintWeaponEnchantmentReference>("d42fc23b92c640846ac137dc26e000d4");
            BlueprintWeaponEnchantmentReference Enchantment3 = GetBlueprintReference<BlueprintWeaponEnchantmentReference>("80bb8a737579e35498177e1e3c75899b");
            BlueprintWeaponEnchantmentReference Enchantment5 = GetBlueprintReference<BlueprintWeaponEnchantmentReference>("bdba267e951851449af552aa9f9e3992");

            BlueprintWeaponEnchantmentReference ColdIron = GetBlueprintReference<BlueprintWeaponEnchantmentReference>("e5990dc76d2a613409916071c898eee8"); 
            BlueprintWeaponEnchantmentReference ForceDamage = GetBlueprintReference<BlueprintWeaponEnchantmentReference>("b183bd491793d194c9e4c96cd11769b1");

            BlueprintGuid stage1FlagGuid = BlueprintGuid.Parse("e50ce85131b64acbb14dc1cade2434d0");
            BlueprintGuid stage2FlagGuid = BlueprintGuid.Parse("10f9d67c98bf4796831ea5aa99e580b3");
            BlueprintGuid stage3FlagGuid = BlueprintGuid.Parse("dce45f9c5e23496284a6b32a5b3f8a7f");

            ItemPolymorph stage1Item = Finnean.GetComponent<ItemPolymorph>(ip => { return ip.m_FlagToCheck.Guid.Equals(stage1FlagGuid); });
            ItemPolymorph stage2Item = Finnean.GetComponent<ItemPolymorph>(ip => { return ip.m_FlagToCheck.Guid.Equals(stage2FlagGuid); });
            ItemPolymorph stage3Item = Finnean.GetComponent<ItemPolymorph>(ip => { return ip.m_FlagToCheck.Guid.Equals(stage3FlagGuid); });

            BlueprintWeaponEnchantmentReference FinneanEch1 = BuildCommonEnchantment(Level1IdGroup, AbilityVariantsLevel1)?.ToReference<BlueprintWeaponEnchantmentReference>();
            BlueprintWeaponEnchantmentReference FinneanEch2 = BuildCommonEnchantment(Level2IdGroup, AbilityVariantsLevel2)?.ToReference<BlueprintWeaponEnchantmentReference>();
            BlueprintWeaponEnchantmentReference FinneanEch3 = BuildCommonEnchantment(Level3IdGroup, AbilityVariantsLevel2)?.ToReference<BlueprintWeaponEnchantmentReference>();

            PatchDefaultEnchantments(stage1Item.m_PolymorphItems, [Enchantment1, ColdIron, FinneanEch1], 2, false);
            PatchDefaultEnchantments(stage2Item.m_PolymorphItems, [Enchantment3, ForceDamage, FinneanEch2], 4, false);
            PatchDefaultEnchantments(stage3Item.m_PolymorphItems, [Enchantment5, ForceDamage, FinneanEch3], 6, false);
        }

        public static BlueprintWeaponEnchantment BuildCommonEnchantment(BpCombine idGroup, List<BpCombine> AbilityVariants)
        {
            if (AbilityVariants.Count <= 0) return null;

            return CreateBlueprint<BlueprintWeaponEnchantment>(
                BlueprintGuid.Parse(idGroup.bpId), "FinneanWeaponEnchantment",
                bp =>
                {
                    bp.m_EnchantName = null;
                    bp.m_Description = null;
                    bp.m_EnchantmentCost = 1;
                    bp.m_HiddenInUI = true;
                    bp.m_IdentifyDC = 5;
                    
                    bp.SetComponents(new AddUnitFeatureEquipment()
                    {
                        m_Feature = CreateBlueprint<BlueprintFeature>(
                            BlueprintGuid.Parse(idGroup.enchantId), "$FinneanWeaponFeature$" + idGroup.enchantId,
                            bp =>
                            {
                                bp.HideInUI = true;
                                bp.IsClassFeature = false;
                                bp.ReapplyOnLevelUp = false;
                                bp.SetComponents(
                                    new AddFacts()
                                    {
                                        m_Flags = 0,
                                        m_Facts = BuildBlueprintAbilities(idGroup.sourceId, AbilityVariants),
                                        DoNotRestoreMissingFacts = false,
                                        HasDifficultyRequirements = false,
                                        InvertDifficultyRequirements = false,
                                        MinDifficulty = GameDifficultyOption.Story
                                    });
                            })?.ToReference<BlueprintFeatureReference>()
                    });
                    
                });
        }

        public static BlueprintUnitFactReference[] BuildBlueprintAbilities(string mainId, List<BpCombine> AbilityVariants)
        {
            var main = BuildMainBlueprintAbility(new(mainId, null, null, false, "FinneanMainAbility", "FinneanMainAbilityDesc"));
            var childs = AbilityVariants?.Select(group => BuildChildBlueprintAbility(group)).ToList() ?? throw new ArgumentNullException();
            main.AddComponent<ActivatableAbilityVariants>(bp =>
            {
                bp.m_Flags = 0;
                bp.m_Variants = [.. childs.Select(ability => ability.ToReference<BlueprintActivatableAbilityReference>())];
            });

            return [.. childs.AddItem(main).Select(ability => ability.ToReference<BlueprintUnitFactReference>())];
        }

        public static BlueprintActivatableAbility BuildMainBlueprintAbility(BpCombine main)
        {
            BlueprintAbility shamaAblity = GetBlueprint<BlueprintAbility>("0b1e0b7f61ca8874b8639a62b6b9a84e");

            return CreateBlueprint<BlueprintActivatableAbility>(
                BlueprintGuid.Parse(main.bpId), "$FinneanWeaponAbility$" + main.bpId,
                bp =>
                {
                    bp.m_DisplayName = main.displayName.UseLocalizedString() ?? shamaAblity.m_DisplayName; ;
                    bp.m_Description = main.displayDesc.UseLocalizedString() ?? shamaAblity.m_Description;
                    bp.m_Icon = shamaAblity.Icon;
                    bp.m_Buff = null;
                    bp.Group = ActivatableAbilityGroup.None;
                    bp.WeightInGroup = 1;
                    bp.IsOnByDefault = false;
                    bp.DeactivateIfCombatEnded = false;
                    bp.DeactivateAfterFirstRound = false;
                    bp.DeactivateImmediately = false;
                    bp.IsTargeted = false;
                    bp.DeactivateIfOwnerDisabled = false;
                    bp.DeactivateIfOwnerUnconscious = false;
                    bp.OnlyInCombat = false;
                    bp.DoNotTurnOffOnRest = false;
                    bp.ActionBarAutoFillIgnored = false;
                    bp.IsRuntimeOnly = false;
                    bp.HiddenInUI = false;
                    bp.ActivationType = AbilityActivationType.Immediately;
                    bp.m_ActivateWithUnitCommand = UnitCommand.CommandType.Swift;
                    bp.m_ActivateOnUnitAction = AbilityActivateOnUnitActionType.Attack;
                    bp.m_SelectTargetAbility = null;
                    bp.ResourceAssetIds = [];

                    bp.SetComponents(
                        new ActivationDisable() { m_Flags = 0 }
                    );
                });
        }

        public static BlueprintActivatableAbility BuildChildBlueprintAbility(BpCombine combine)
        {
            var copy = string.IsNullOrEmpty(combine.sourceId) ? null : GetBlueprint<BlueprintUnitFact>(combine.sourceId);
            var enchantment = GetBlueprint<BlueprintItemEnchantment>(combine.enchantId);
            var reference = enchantment.ToReference<BlueprintItemEnchantmentReference>();

            BlueprintGuid blueprintGuid = BlueprintGuid.Parse(combine.bpId);

            return CreateBlueprint<BlueprintActivatableAbilityRe>(
                blueprintGuid, "$FinneanWeaponChildAbility$" + combine.bpId,
                bp =>
                {
                    bp.m_DisplayName = combine.displayName.UseLocalizedString()??enchantment.m_Prefix;
                    bp.m_Description = combine.displayDesc.UseLocalizedString() ?? enchantment.m_Description;
                    bp.m_Icon = copy?.Icon;
                    bp.m_Buff = null;
                    bp.m_Enchant = reference;
                    bp.Group = ActivatableAbilityGroup.None;
                    bp.WeightInGroup = enchantment.EnchantmentCost;
                    bp.IsOnByDefault = combine.isOn;
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
                    bp.HiddenInUI = true;
                    bp.ActivationType = AbilityActivationType.WithUnitCommand;
                    bp.m_ActivateWithUnitCommand = UnitCommand.CommandType.Standard;
                    bp.m_ActivateOnUnitAction = AbilityActivateOnUnitActionType.Attack;
                    bp.m_SelectTargetAbility = null;
                    bp.ResourceAssetIds = [];

                    bp.SetComponents(
                        new AddTriggerOnActivationChanged()
                        {
                            m_Stage = AddTriggerOnActivationChanged.Stage.OnSwitchOn,
                            m_ActionList = CreateActionList(
                                Create<ContextActionSpawnFx>(sf => sf.PrefabLink = new PrefabLink() { AssetId = "81fdc9b3d09cfc5479248bef6e985f4a" }),
                                Create<ContextActionWeaponEnchant>(en => {
                                    en.m_Enchant = reference;
                                    en.m_AbilityGuid = blueprintGuid;
                                })
                            )
                        },
                        new AddTriggerOnActivationChanged()
                        {
                            m_Stage = AddTriggerOnActivationChanged.Stage.OnSwitchOff,
                            m_ActionList = CreateActionList(
                                Create<ContextActionWeaponDeEnchant>(en => {
                                    en.m_Enchant = reference;
                                })
                            )
                        }
                    );
                });
        }

        public static void ClearCache() => activedAbilities.Clear();

        public static EnchantCombine FindFirst() => activedAbilities.FirstOrDefault();

        public static void AddValue(EnchantCombine combine)
        {
            if (!ContainsValue(combine.abilityGuid)) activedAbilities.AddLast(combine);
        }

        public static void RemoveByEnchant(BlueprintGuid enchantId)
        {
            var item = FindByEnchant(enchantId);
            if (item != null) activedAbilities.Remove(item);
        }

        public static bool ContainsValue(BlueprintGuid guid) => activedAbilities.Any(c => guid == c.abilityGuid);

        public static EnchantCombine FindByEnchant(BlueprintGuid enchantId) =>
            activedAbilities.FirstOrDefault(c => enchantId == c.enchantGuid);
    }

    public class EnchantCombine(BlueprintGuid abilityGuid, BlueprintGuid enchantGuid, int cost)
    {
        public BlueprintGuid abilityGuid = abilityGuid;
        public BlueprintGuid enchantGuid = enchantGuid;
        public int cost = cost;
    }

    public class BpCombine(string bpId, string enchantId, string sourceId, bool isOn = false, string displayName = null, string displayDesc = null)
    {
        public string bpId = bpId;
        public string enchantId = enchantId;
        public string sourceId = sourceId;
        public bool isOn = isOn;
        public string displayName = displayName;
        public string displayDesc = displayDesc;
    }
}