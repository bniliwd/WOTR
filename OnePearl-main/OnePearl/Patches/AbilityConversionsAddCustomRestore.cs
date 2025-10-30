using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using OnePearl.Components;

namespace OnePearl.Patches;

/// <summary>
/// Patches ability conversions collection method to include 
/// new AbilityRestoreFixedLevelSpellSlot
/// </summary>
[HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetConversions))]
internal static class AbilityConversionsAddCustomRestore
{
    [HarmonyPostfix]
    public static void Postfix(AbilityData __instance, ref IEnumerable<AbilityData> __result)
    {
        var spellbook = __instance.Spellbook;
        var spellLevel = __instance.SpellLevel;
        if (spellbook == null || spellbook.Blueprint.IsAlchemist || !(spellLevel > 0))
        {
            return;
        }
        List<AbilityData> tmpList = [];
        var spellSlot = __instance.SpellSlot;
        foreach (var ability in __instance.Caster.Abilities)
        {
            var restoreComponent = ability.Blueprint.GetComponent<AbilityRestoreFixedLevelSpellSlot>();
            if (restoreComponent != null && restoreComponent.SpellLevel == spellLevel)
            {
                AbilityData.AddAbilityUnique(ref tmpList, new AbilityData(ability)
                {
                    ParamSpellbook = spellbook,
                    ParamSpellLevel = spellLevel,
                    ParamSpellSlot = spellSlot
                });
            }
        }
        if (tmpList.Count > 0)
        {
            if (__result is List<AbilityData> list)
            {
                list.AddRange(tmpList);
            }
            else
            {
                __result = __result == null ? tmpList : [.. Enumerable.Concat(__result, tmpList)];
            }
        }
    }
}