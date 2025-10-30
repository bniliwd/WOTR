using BetterGameplay.NewContent;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using static BetterGameplay.NewContent.SentientFinnean;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Logic
{

    [TypeId("5a962b3a58244a008cd9d80982343b42")]
    public class ContextActionWeaponEnchant : ContextAction
    {
        public BlueprintItemEnchantmentReference m_Enchant;

        public BlueprintGuid m_AbilityGuid;

        public BlueprintItemEnchantment Enchant => m_Enchant?.Get();

        public override string GetCaption()
        {
            return "Add enchants to Finnean";
        }

        public override void RunAction()
        {
            UnitEntityData unit = base.Target.Unit;
            if (unit == null)
            {
                return;
            }

            ItemEntityWeapon weapon = base.Target.Unit.Body.PrimaryHand.MaybeWeapon;
            if (!IsFinnean(weapon))
            {
                if (!IsFinnean(base.Target.Unit.Body.SecondaryHand.MaybeWeapon)) return;
                weapon = base.Target.Unit.Body.SecondaryHand.MaybeWeapon;
            }

            if (weapon == null)
            {
                return;
            }

            if (weapon.Blueprint.Charges < Enchant.EnchantmentCost)
            {
                PFLog.Mods.Error("附魔点数比总可用 {0} 大", weapon.Blueprint.Charges);
                throw new Exception("附魔点数比总可用大");
            }

            if (checkActived)
            {
                InitEnchantmentToPool();
                checkActived = false;
            }

            bool listContains = ContainsValue(m_AbilityGuid);
            var attached = weapon.GetEnchantment(Enchant);

            bool permanent = attached?.IsTemporary == false;

            if (listContains && !permanent)
            {
                listContains = false;
                RemoveEnchantment(weapon, attached);
            }
            else if (!listContains && permanent)
            {
                AddValue(new EnchantCombine(m_AbilityGuid, Enchant.AssetGuid, Enchant.EnchantmentCost));
                listContains = true;
            }

            if (listContains)
            {
                return;
            }

            EnlargeCapacity(weapon.Blueprint.Charges);

            AddEnchantment(weapon);
        }

        public bool IsFinnean(ItemEntityWeapon entity)
        {
            return entity?.NameForAcronym.StartsWith("Finnean", StringComparison.Ordinal) ?? false;
        }

        public void EnlargeCapacity(int maxEnchantmentPoint)
        {
            int max = 0;
            while (activedAbilities.Count > 0 && (activedAbilities.Sum(e => e.cost) + Enchant.EnchantmentCost) > maxEnchantmentPoint && max < 10)
            {
                TurnOffOldestActivatableAbility();
                max++;
            }
        }

        public void TurnOffOldestActivatableAbility()
        {
            BlueprintActivatableAbility ability = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>(FindFirst().abilityGuid);
            if (ability != null)
            {
                ActivatableAbility activatableAbility = null;
                foreach (ActivatableAbility item in base.Target.Unit.ActivatableAbilities)
                {
                    if (item.Blueprint == ability)
                    {
                        activatableAbility = item;
                        break;
                    }
                }

                if (activatableAbility == null || !activatableAbility.IsOn)
                {
                    InitEnchantmentToPool();
                } else {
                    activatableAbility.SetIsOn(false, null);
                }
            }
        }

        public void AddEnchantment(ItemEntityWeapon weapon)
        {
            weapon.AddEnchantmentInternal(Enchant, base.Context, null);
            AddValue(new EnchantCombine(m_AbilityGuid, Enchant.AssetGuid, Enchant.EnchantmentCost));
        }

        public virtual void RemoveEnchantment(ItemEntityWeapon weapon, ItemEnchantment enchantment)
        {
            weapon.EnchantmentsCollection?.RemoveFact(enchantment);
            weapon.OnEnchantmentRemoved(enchantment);
            RemoveByEnchant(Enchant.AssetGuid);
        }

        public void InitEnchantmentToPool()
        {
            ClearCache();
            List<BlueprintActivatableAbility> toCheck = [
                GetBlueprint<BlueprintActivatableAbility>("1f29fcd85f7c4ff585033375fd997bbc"), 
                GetBlueprint<BlueprintActivatableAbility>("e93455e1b563424997fb701a04e46555"), 
                GetBlueprint<BlueprintActivatableAbility>("b27af38ec8714361964f262e12ff80c9")
            ];

            BlueprintActivatableAbility realToCheck = null;
            foreach (ActivatableAbility item in base.Target.Unit.ActivatableAbilities)
            {
                if (toCheck.Contains(item.Blueprint))
                {
                    realToCheck = item.Blueprint;
                    break;
                }
            }

            if (realToCheck == null) return;

            BlueprintActivatableAbilityReference[] variants = realToCheck.GetComponent<ActivatableAbilityVariants>().m_Variants;
            HashSet<BlueprintGuid> set = [.. variants.Select(re => re.deserializedGuid)];

            foreach (ActivatableAbility aa in base.Target.Unit.ActivatableAbilities)
            {
                if (set.Count <= 0) break;
                if (set.Contains(aa.Blueprint.AssetGuid))
                {
                    if (aa.IsOn && !(aa.Blueprint.AssetGuid == m_AbilityGuid))
                    {
                        if (aa.Blueprint is BlueprintActivatableAbilityRe re)
                        {
                            AddValue(new(re.AssetGuid, re.Enchant.AssetGuid, re.Enchant.EnchantmentCost));
                        }
                    }
                    set.Remove(aa.Blueprint.AssetGuid);
                }
            }
        }

    }
}