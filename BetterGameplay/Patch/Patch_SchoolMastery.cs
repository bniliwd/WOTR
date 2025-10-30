using HarmonyLib;
using Kingmaker.Designers.Mechanics.Facts;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(SchoolMasteryParametrized))]
    public static class Patch_SchoolMastery
    {
        [HarmonyPatch(nameof(SchoolMasteryParametrized.GetBonus))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return [new (OpCodes.Ldc_I4_2), new (OpCodes.Ret)];
        }
    }
}