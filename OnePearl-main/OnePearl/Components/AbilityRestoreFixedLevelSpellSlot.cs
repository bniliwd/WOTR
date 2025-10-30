using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using OnePearl.Utils;

namespace OnePearl.Components;

[TypeId("3fc64c9a90bc4d699618244850570727")]
public class AbilityRestoreFixedLevelSpellSlot : AbilityApplyEffect, IAbilityRequiredParameters, IAbilityRestriction
{
    public int SpellLevel;
    public AbilityParameter RequiredParameters => AbilityParameter.Spellbook | AbilityParameter.SpellLevel;

    public override void Apply(AbilityExecutionContext context, TargetWrapper target)
    {

        Spellbook paramSpellbook = context.Ability.ParamSpellbook;
        int? paramSpellLevel = context.Ability.ParamSpellLevel;

        if (paramSpellbook == null || paramSpellLevel == null || paramSpellLevel != SpellLevel)
        {
            return;
        }

        if (paramSpellbook.Blueprint.Spontaneous)
        {
            int spontaneousSlots = paramSpellbook.GetSpontaneousSlots(paramSpellLevel.Value);
            int spellsPerDay = paramSpellbook.GetSpellsPerDay(paramSpellLevel.Value);
            if (spontaneousSlots < spellsPerDay)
            {
                var resourceSpent = PearlUtils.TrySpendPearlResource(context.MaybeOwner, SpellLevel, out var pearlTotals);
                if (resourceSpent)
                {
                    paramSpellbook.RestoreSpontaneousSlots(paramSpellLevel.Value, 1);
                }
                PearlUtils.UpdateResources(context.MaybeOwner, pearlTotals, false);
            }
            return;
        }


        foreach (SpellSlot spellSlot in paramSpellbook.GetMemorizedSpellSlots(paramSpellLevel.Value))
        {
            SpellSlot paramSpellSlot = context.Ability.ParamSpellSlot;
            if (paramSpellSlot?.SpellShell == null || context.Ability.ParamSpellSlot.SpellShell == null)
            {
                break;
            }

            if (!spellSlot.Available && spellSlot.SpellShell == context.Ability.ParamSpellSlot.SpellShell)
            {
                var resourceSpent = PearlUtils.TrySpendPearlResource(context.MaybeOwner, SpellLevel, out var pearlTotals);
                if (resourceSpent)
                {
                    spellSlot.Available = true;
                }
                PearlUtils.UpdateResources(context.MaybeOwner, pearlTotals, false);
                break;
            }
        }
    }

    bool IAbilityRestriction.IsAbilityRestrictionPassed(AbilityData ability)
    {
        if (ability == null)
        {
            return false;
        }

        Spellbook paramSpellbook = ability.ParamSpellbook;
        int? paramSpellLevel = ability.ParamSpellLevel;
        if (paramSpellbook != null && paramSpellLevel != null)
        {
            if (paramSpellLevel != this.SpellLevel)
            {
                return false;
            }

            if (paramSpellbook.Blueprint.Spontaneous)
            {
                int spontaneousSlots = paramSpellbook.GetSpontaneousSlots(paramSpellLevel.Value);
                int spellsPerDay = paramSpellbook.GetSpellsPerDay(paramSpellLevel.Value);
                return spontaneousSlots < spellsPerDay;
            }
            foreach (SpellSlot spellSlot in paramSpellbook.GetMemorizedSpellSlots(paramSpellLevel.Value))
            {
                if (!spellSlot.Available)
                {
                    if (spellSlot.SpellShell == ability.ParamSpellSlot?.SpellShell)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }

    string IAbilityRestriction.GetAbilityRestrictionUIText()
    {
        return "";
    }

    public static SpellSlot GetNotAvailableSpellSlot(AbilityData ability)
    {
        if (ability.Spellbook == null)
        {
            return null;
        }

        foreach (SpellSlot memorizedSpellSlot in ability.Spellbook.GetMemorizedSpellSlots(ability.SpellLevel))
        {
            if (!memorizedSpellSlot.Available && memorizedSpellSlot.SpellShell == ability)
            {
                return memorizedSpellSlot;
            }
        }

        return null;
    }
}
