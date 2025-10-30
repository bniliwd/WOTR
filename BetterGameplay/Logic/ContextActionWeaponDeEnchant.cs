using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using static BetterGameplay.NewContent.SentientFinnean;

namespace BetterGameplay.Logic
{
    [TypeId("0c7758e95c2b4d8cba5cd4b0ae75eb0b")]
    public class ContextActionWeaponDeEnchant : ContextAction
    {
        public BlueprintItemEnchantmentReference m_Enchant;

        public BlueprintItemEnchantment Enchant => m_Enchant?.Get();

        public override string GetCaption()
        {
            return "Remove enchants from Finnean";
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

            var exists = weapon.GetEnchantment(Enchant);

            if (exists != null)
            {
                RemoveEnchantment(weapon, exists);
            }

            RemoveByEnchant(Enchant.AssetGuid);
        }

        public bool IsFinnean(ItemEntityWeapon entity)
        {
            return entity?.NameForAcronym.StartsWith("Finnean", StringComparison.Ordinal) ?? false;
        }

        public virtual void RemoveEnchantment(ItemEntityWeapon weapon, ItemEnchantment enchantment)
        {
            weapon.EnchantmentsCollection?.RemoveFact(enchantment);
            weapon.OnEnchantmentRemoved(enchantment);
        }
    }
}