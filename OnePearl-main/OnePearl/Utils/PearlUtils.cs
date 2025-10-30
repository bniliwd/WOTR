using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using OnePearl.Components;

namespace OnePearl.Utils;
internal static class PearlUtils
{
    /// <summary>
    /// Collects all pearls in the inventory
    /// </summary>
    /// <param name="inventory">inventory to look in</param>
    /// <param name="takeMaxCharges">use max charges instead of current</param>
    /// <returns></returns>
    public static IEnumerable<PearlOfPower> CollectPearls(ItemsCollection inventory, bool takeMaxCharges, bool merge)
    {
        var pearls = inventory
            .Where(x => x.Blueprint is BlueprintItemEquipmentUsable usable
                && !x.IsDisposed
                && usable.Type == UsableItemType.Other
                && usable.SpendCharges
                && usable.RestoreChargesOnRest
                && usable.Charges <= 2
                && usable.m_Ability != null
                && usable.Ability.GetComponent<AbilityRestoreSpellSlot>() != null
            );
        if (merge)
        {
            foreach (var item in pearls)
            {
                if (!item.IsDisposed)
                {
                    item.TryMergeInCollection();
                }
            }
            pearls = inventory
                .Where(x => x.Blueprint is BlueprintItemEquipmentUsable usable
                && !x.IsDisposed
                && usable.Type == UsableItemType.Other
                && usable.SpendCharges
                && usable.RestoreChargesOnRest
                && usable.Charges <= 2
                && usable.m_Ability != null
                && usable.Ability.GetComponent<AbilityRestoreSpellSlot>() != null
                );
        }
        var collected = pearls
            .Select(x =>
                {
                    var bp = x.Blueprint as BlueprintItemEquipmentUsable;
                    var restoreSpell = bp.Ability.GetComponent<AbilityRestoreSpellSlot>();
                    var charges = x.Count * (takeMaxCharges ? bp.Charges : x.Charges);
                    return new PearlOfPower(x, restoreSpell.SpellLevel, charges);
                });
        return collected;
    }

    /// <summary>
    /// Aggregates pearls charges into array, where index is spell level and
    /// value is amount of charges for that level
    /// </summary>
    /// <param name="pearls"></param>
    /// <param name="allowLowerSlots"></param>
    /// <returns></returns>
    internal static int[] PearlTotal(IEnumerable<PearlOfPower> pearls)
    {
        var arr = pearls.Aggregate(new int[10], (acc, item) =>
        {
            acc[item.MaxSpellLevel] += item.Charges;
            return acc;
        });
        return arr;
    }

    /// <summary>
    /// Spends resource of one of the pearls in inventory
    /// </summary>
    /// <param name="unit">unit</param>
    /// <param name="level">spell level</param>
    /// <param name="exactLevel">Only use pearls of exact level or pearls of higher level too</param>
    /// <param name="pearls">collection of pearls user has</param>
    /// <returns></returns>
    internal static bool TrySpendPearlResource(UnitEntityData unit, int level, out IEnumerable<PearlOfPower> pearls)
    {
        var inventory = unit.Inventory;
        pearls = CollectPearls(inventory, false, false).ToList();
        var pearl = pearls
            .Where(x => x.Charges > 0 && x.MaxSpellLevel == level)
            .OrderBy(x => x.MaxSpellLevel)
            .FirstOrDefault();
        if (pearl != default)
        {
            // if count > 1 that means it's a stack of pearls.
            // Trying to spend on it, will affect all pearls in it
            // So we split one pearl from the stack and spend charge from it
            if (pearl.ItemEntity.Count > 1)
            {
                var newPearl = pearl.ItemEntity.Split(1);
                newPearl.SpendCharges(unit);
                newPearl.TryMergeInCollection();
            }
            else
            {
                pearl.ItemEntity.SpendCharges(unit);
                pearl.ItemEntity = pearl.ItemEntity.TryMergeInCollection();
            }
            pearl.Charges--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates One Pearl resources based on pearls
    /// </summary>
    /// <param name="unit">unit</param>
    /// <param name="pearls">pearls in posession</param>
    /// <param name="restore">are we restoring or spending</param>
    /// <param name="upToLevel">affect resources only up to certain spell level</param>
    /// 
    public static void UpdateResources(UnitEntityData unit, IEnumerable<PearlOfPower> pearls, bool restore, int upToLevel = 9)
    {
        var totals = PearlTotal(pearls);
        UpdateResources(unit, totals, restore, upToLevel);
    }

    private static void UpdateResources(UnitEntityData unit, int[] amounts, bool restore, int upToLevel = 9)
    {
        for (int i = 1; i <= upToLevel; i++)
        {
            var res = BlueprintCreator.PearlAbilityResources[i - 1];
            var resource = unit.Descriptor.Resources.GetResource(res);
            if (resource != null)
            {
                var oldAmount = resource.Amount;
                if (oldAmount != amounts[i])
                {
                    if (restore)
                    {
                        resource.Amount = amounts[i];
                        EventBus.RaiseEvent(delegate (IUnitAbilityResourceHandler h)
                        {
                            h.HandleAbilityResourceChange(unit, resource, oldAmount);
                        });
                    }
                    else if (oldAmount - amounts[i] > 0)
                    {
                        unit.Descriptor.Resources.Spend(res, oldAmount - amounts[i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if unit has One Pearl equipped
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public static bool HasOnePearlEquipped(UnitEntityData unit)
    {
        if (unit == null || unit.Body == null || unit.Body.QuickSlots == null)
        {
            return false;
        }
        return unit.Body.QuickSlots.Any(x => x.Item != null && x.Item.Blueprint.AssetGuid == BlueprintCreator.OnePearl.AssetGuid);
    }

    public class PearlOfPower(ItemEntity itemEntity, int maxSpellLevel, int charges)
    {
        public int Charges = charges;
        public ItemEntity ItemEntity = itemEntity;
        public int MaxSpellLevel = maxSpellLevel;
    };
}
