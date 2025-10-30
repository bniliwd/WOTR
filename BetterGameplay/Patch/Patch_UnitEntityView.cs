using HarmonyLib;
using Kingmaker.Utility;
using Kingmaker.View;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterGameplay.Patch
{
    [ClassInfoBox("感谢ttt作者")]
    [HarmonyPatch(typeof(UnitEntityView), nameof(UnitEntityView.SetupSelectionColliders), [typeof(bool)])]
    static class Patch_UnitEntityView
    {
        static readonly FieldInfo UnitEntityView_m_Corpulence = AccessTools.Field(typeof(UnitEntityView), "m_Corpulence");
        static readonly MethodInfo UnitEntityView_Corpulence = AccessTools.PropertyGetter(typeof(UnitEntityView), "Corpulence");

        //Prevent Collision radius from scaling with size adjusted corpulance 阻止模型半径随体型膨胀 from TTT
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int target = FindInsertionTarget(codes);
            if (target >= 0 && target < codes.Count)
            {
                codes[target] = new CodeInstruction(OpCodes.Ldfld, UnitEntityView_m_Corpulence);
            }
            return codes.AsEnumerable();
        }

        private static int FindInsertionTarget(List<CodeInstruction> codes)
        {
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(UnitEntityView_Corpulence))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}