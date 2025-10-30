using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using static OnePearl.Utils.BlueprintUtils;

namespace OnePearl.Components;
internal static class BlueprintCreator
{
    public static BlueprintItem OnePearl;
    public static BlueprintAbilityResource[] PearlAbilityResources;
    public static HashSet<BlueprintGuid> PearlAbilityResourceRefs;

    internal static void CreateBlueprints()
    {
        var exquisitePearlIcon = GetBlueprint<BlueprintItem>("f682126f69da1ea479bf1ddf1d775d97").m_Icon;
        BlueprintUnitFactReference[] abilities = new BlueprintUnitFactReference[9];

        BlueprintAbilityResource[] resources = new BlueprintAbilityResource[9];

        var normalPearls = new List<BlueprintItemEquipmentUsable>() {
            GetBlueprint<BlueprintItemEquipmentUsable>("edab1a1ee8654810aca64df16dd87aae"),
            GetBlueprint<BlueprintItemEquipmentUsable>("f548bd21606344f0963c0ae374946d39"),
            GetBlueprint<BlueprintItemEquipmentUsable>("d2d4b585db9f43698d183437f20bd8de"),
            GetBlueprint<BlueprintItemEquipmentUsable>("54cbc6480de54d49b277d2686a98b4fb"),
            GetBlueprint<BlueprintItemEquipmentUsable>("3f3aa97c9a1c46d5a8b3a3c22a5727db"),
            GetBlueprint<BlueprintItemEquipmentUsable>("8103f79802eb4ec59c373829ff5907a1"),
            GetBlueprint<BlueprintItemEquipmentUsable>("778b25032e7343ba856326bf7f3dfeab"),
            GetBlueprint<BlueprintItemEquipmentUsable>("d0552e5dd29d49568cebfb8fd2335486"),
            GetBlueprint<BlueprintItemEquipmentUsable>("4e98e92f49024c529cb2afa01fc63b0e"),
        };

        // resources
        for (int i = 1; i <= 9; i++)
        {
            var resource = CreateBlueprint<BlueprintAbilityResource>($"OnePearlRestoreResource{i}", bp =>
            {
                bp.m_Min = 0;
                bp.m_Max = 99;
            });
            resources[i - 1] = resource;
        }
        PearlAbilityResources = resources;
        PearlAbilityResourceRefs = [.. resources.Select(x => x.AssetGuid)];

        var nullString = new LocalizedString();

        // restore abilities
        for (int i = 1; i <= 9; i++)
        {
            var ability = CreateBlueprint<BlueprintAbility>($"OnePearlRestoreAbility{i}", bp =>
            {
                bp.m_Icon = normalPearls[i - 1].m_Icon;
                bp.m_DisplayName = normalPearls[i - 1].m_DisplayNameText;
                bp.m_Description = normalPearls[i - 1].m_DescriptionText;
                bp.LocalizedSavingThrow = nullString;
                bp.LocalizedDuration = nullString;
                bp.SetComponents(
                    new AbilityResourceLogic() {
                        m_RequiredResource = resources[i - 1].ToReference<BlueprintAbilityResourceReference>(),
                        m_IsSpendResource = true
                    },
                    new AbilityRestoreFixedLevelSpellSlot() { SpellLevel = i },
                    new HideDCFromTooltip()
                );
                bp.ActionBarAutoFillIgnored = true;
                bp.Hidden = true;
                bp.ActionType = UnitCommand.CommandType.Standard;
                bp.Type = AbilityType.Supernatural;
                bp.Range = AbilityRange.Personal;
                bp.CanTargetSelf = true;
                bp.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            });
            abilities[i - 1] = ability.ToReference<BlueprintUnitFactReference>();
        }

        // feature granting resources and abilities
        var onePearlFeature = CreateBlueprint<BlueprintFeature>("$OnePearlFeature", bp =>
        {
            bp.AddComponent<AddFacts>(c =>
            {
                c.m_Facts = abilities;
            });
            for (int i = 0; i < resources.Length; i++)
            {
                bp.AddComponent<AddAbilityResources>(c =>
                {
                    c.RestoreAmount = false;
                    c.m_Resource = resources[i].ToReference<BlueprintAbilityResourceReference>();
                });
            }
            bp.AddComponent<AddRestTrigger>(c =>
            {
                c.Action = new()
                {
                    Actions = [
                        new UpdateOnePearlResourcesAction
                        {
                            MergePearls = true,
                            TakeMaxCharges = true
                        }
                    ]
                };
            });
            bp.AddComponent<OnePearlResourceUpdateHandler>();
            bp.HideInUI = true;
            bp.HideInCharacterSheetAndLevelUp = true;
            bp.IsClassFeature = false;
        });

        // enchnantment that grants feature above
        var ench = CreateBlueprint<BlueprintEquipmentEnchantment>("OnePearlEnchantment", bp =>
        {
            bp.m_EnchantName = nullString;
            bp.m_Description = nullString;
            bp.m_Prefix = nullString;
            bp.m_Suffix = nullString;
            bp.m_IdentifyDC = 0;
            bp.m_EnchantmentCost = 0;
            bp.AddComponent<AddOnePearlFeatureEquipment>(c =>
            {
                c.m_Feature = onePearlFeature.ToReference<BlueprintFeatureReference>();
            });
        });

        var updResources = CreateBlueprint<BlueprintAbility>($"OnePearlResourceUpdater", bp =>
        {
            bp.m_Icon = exquisitePearlIcon;
            bp.m_DisplayName = CreateLocalizedString(bp.name, "合一珍珠");
            bp.m_Description = CreateLocalizedString($"{bp.name}Desc", "激活合一珍珠，获取背包中所有法力珍珠的有效次数。");
            bp.LocalizedSavingThrow = nullString;
            bp.LocalizedDuration = nullString;
            bp.AddComponent<HideDCFromTooltip>();
            bp.AddComponent<AbilityEffectRunAction>(c =>
            {
                c.Actions = new()
                {
                    Actions = [
                        new UpdateOnePearlResourcesAction
                        {
                            TakeMaxCharges = false
                        }
                    ]
                };
            });
            bp.ActionType = UnitCommand.CommandType.Standard;
            bp.Type = AbilityType.Supernatural;
            bp.Range = AbilityRange.Personal;
            bp.CanTargetSelf = true;
            bp.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
        });

        // the item
        var item = CreateBlueprint<BlueprintItemEquipmentUsable>("OnePearlItem", bp =>
        {
            bp.m_Icon = exquisitePearlIcon;
            bp.m_DisplayNameText = CreateLocalizedString(bp.name, "合一珍珠");
            bp.m_DescriptionText = CreateLocalizedString($"{bp.name}Desc", "集合了拥有者所有法力珍珠能力和资源的合一珍珠。");
            bp.m_FlavorText = nullString;
            bp.m_NonIdentifiedDescriptionText = nullString;
            bp.m_NonIdentifiedNameText = nullString;
            bp.m_Cost = 10;
            bp.m_Weight = 0;
            bp.Charges = 1;
            bp.SpendCharges = false;
            bp.m_Ability = updResources.ToReference<BlueprintAbilityReference>();
            bp.Type = UsableItemType.Other;
            bp.m_MiscellaneousType = BlueprintItem.MiscellaneousItemType.None;
            bp.m_Destructible = false;
            bp.DC = 0;
            bp.m_Enchantments = [
                ench.ToReference<BlueprintEquipmentEnchantmentReference>()
            ];
            bp.m_BeltItemPrefab = normalPearls[0].m_BeltItemPrefab;
            bp.m_ShardItem = normalPearls[0].m_ShardItem;
            bp.m_EquipmentEntity = normalPearls[0].m_EquipmentEntity;
            bp.m_EquipmentEntityAlternatives = [];
        });

        OnePearl = item;

        var vendor = GetBlueprint<BlueprintSharedVendorTable>("5753b6f35e7db234aa44085a358c27af");
        vendor.AddComponent<LootItemsPackFixed>(
            item =>
            {
                item.m_Flags = 0;
                item.m_Item = new LootItem()
                {
                    m_Type = LootItemType.Item,
                    m_Item = OnePearl.ToReference<BlueprintItemReference>(),
                };
                item.m_Count = 1;
            }
        );
    }
}