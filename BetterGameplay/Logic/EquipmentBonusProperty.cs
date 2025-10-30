using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;

namespace BetterGameplay.Logic
{
    [TypeId("b59e204b1b2d4b899f846eb7d6eda15e")]
    public class EquipmentBonusProperty : PropertyValueGetter
    {
        public bool isArmor = true;

        public bool isShield = false;

        public ContextRankProgression scale;

        public override int GetBaseValue(UnitEntityData unit)
        {
            if(!isArmor && !isShield)
            {
                return GetMythicValue(unit);
            }

            IEnumerable<ModifiableValue.Modifier> modifiers = unit.Descriptor.Stats.AC.Modifiers;
            int maxBaseAc = 0, maxBonusAc = 0, maxFocusAc = 0;
            foreach (ModifiableValue.Modifier item in modifiers)
            {
                if ((item.ModDescriptor == ModifierDescriptor.Armor && isArmor) || (item.ModDescriptor == ModifierDescriptor.Shield && isShield))
                {
                    if (!CheckItem(item))
                    {
                        continue;
                    }
                    maxBaseAc = Math.Max(maxBaseAc, item.ModValue);
                }

                if ((item.ModDescriptor == ModifierDescriptor.ArmorEnhancement && isArmor) || (item.ModDescriptor == ModifierDescriptor.ShieldEnhancement && isShield))
                {
                    if (!CheckItem(item))
                    {
                        continue;
                    }
                    maxBonusAc = Math.Max(maxBonusAc, item.ModValue);
                }

                if ((item.ModDescriptor == ModifierDescriptor.ArmorFocus && isArmor) || (item.ModDescriptor == ModifierDescriptor.ShieldFocus && isShield))
                {
                    maxFocusAc = Math.Max(maxFocusAc, item.ModValue);
                }
            }

            return Math.Min(GetMythicValue(unit), maxBaseAc + maxBonusAc + maxFocusAc);
        }

        public bool CheckItem(ModifiableValue.Modifier modifier)
        {
            if (modifier.ItemSource == null)
            {
                return false;
            }

            return true;
        }

        public int GetMythicValue(UnitEntityData unit)
        {
            int mythicLevel = unit.Progression.MythicLevel;
            return scale switch
            {
                ContextRankProgression.Div2 => mythicLevel / 2,
                ContextRankProgression.OnePlusDivStep => 1 + Math.Max(mythicLevel / 2, 0),
                ContextRankProgression.AsIs => mythicLevel,
                ContextRankProgression.HalfMore => mythicLevel + mythicLevel / 2,
                ContextRankProgression.DivStep => mythicLevel / 4,
                _ => 0,
            };
        }

    }
}
