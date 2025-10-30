using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(ContextActionProvokeAttackOfOpportunity))]
    public static class Patch_ContextActionProvokeAttackOfOpportunity
    {
        [HarmonyPatch(nameof(ContextActionProvokeAttackOfOpportunity.RunAction))]
        [HarmonyPrefix]
        public static bool Prefix(ContextActionProvokeAttackOfOpportunity __instance)
        {
            UnitEntityData attacker, target;
            if (__instance.ApplyToCaster)
            {
                attacker = __instance.Target.Unit;
                target = __instance.Context.MaybeCaster;
            }
            else
            {
                attacker = __instance.Context.MaybeCaster;
                target = __instance.Target.Unit;
            }

            if (attacker == null || target == null)
            {
                PFLog.Mods.Error("Attacker or Target is missing");
                return false;
            }

            Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(attacker, target);

            return false;
        }
    }
}