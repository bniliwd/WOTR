using HarmonyLib;
using Kingmaker.UnitLogic.Mechanics.Actions;
using OnePearl.Utils;

namespace OnePearl.Patches;

/// <summary>
/// Restoring resources by itself causes pearl-given resources to be zero
/// This patch recalculates 
/// </summary>
[HarmonyPatch(typeof(ContextRestoreResource), nameof(ContextRestoreResource.RunAction))]
static class ContextRestoreResourceAfter
{
    [HarmonyPostfix]
    static void AfterRestore(ContextRestoreResource __instance)
    {
        if (!__instance.m_IsFullRestoreAllResources)
        {
            return;
        }
        var unit = __instance.Target.Unit;
        if (unit == null || unit.Inventory == null || !PearlUtils.HasOnePearlEquipped(unit))
        {
            return;
        }
        var pearls = PearlUtils.CollectPearls(unit.Inventory, false, false);
        PearlUtils.UpdateResources(unit, pearls, true);
    }
}
