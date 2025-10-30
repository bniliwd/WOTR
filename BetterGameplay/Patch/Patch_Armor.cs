using HarmonyLib;
using Kingmaker.Items;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BetterGameplay.Patch
{
    [HarmonyPatch]
    public static class Patch_Armor
    {
        [HarmonyPatch(typeof(ItemEntityArmor), nameof(ItemEntityArmor.IsMithral))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return [new(OpCodes.Ldc_I4_0), new(OpCodes.Ret)];
        }
    }
}