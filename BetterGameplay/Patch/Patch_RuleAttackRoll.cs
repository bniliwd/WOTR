using BetterGameplay.Rule;
using HarmonyLib;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterGameplay.Patch
{
    [ClassInfoBox("改自ttt，替换攻击检定中的护命部分")]
    [HarmonyPatch(typeof(RuleAttackRoll), nameof(RuleAttackRoll.OnTrigger), [typeof(RulebookEventContext)])]
    public class Patch_RuleAttackRoll
    {
        static readonly MethodInfo get_TargetUseFortification = AccessTools.PropertyGetter(typeof(RuleAttackRoll), nameof(RuleAttackRoll.TargetUseFortification));
        static readonly MethodInfo checkFortification = AccessTools.Method(typeof(Patch_RuleAttackRoll), nameof(CheckFortification));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var target = FindInsertionTarget(codes);
            if (target.Index < 0)
            {
                return codes;
            }

            var labels = codes.GetRange(target.Start, (target.End - target.Start))
                .SelectMany(c => c.labels)
                .Where(labels => labels != null)
                .Distinct()
                .ToList();

            codes.RemoveRange(target.Start, 1 + (target.End - target.Start));
            codes.InsertRange(target.Start, [
                new (OpCodes.Ldarg_0), new (OpCodes.Call, checkFortification),
            ]);
            codes[target.Start].labels = labels;

            return codes;
        }
        private static TargetInfo FindInsertionTarget(List<CodeInstruction> codes)
        {
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(get_TargetUseFortification))
                {
                    return new TargetInfo(i, i - 12, i + 24);
                }
            }
            return new TargetInfo(-1, -1, -1);
        }
        private struct TargetInfo(int index, int start, int end)
        {
            public int Index = index;
            public int Start = start;
            public int End = end;
        }

        private static void CheckFortification(RuleAttackRoll evt)
        {
            RuleFortificationCheck beforeTrigger = new(evt, evt.Weapon);
            if (!beforeTrigger.TargetUseFortification) { return; }

            var FortificationCheck = Rulebook.Trigger<RuleFortificationCheck>(beforeTrigger);
            evt.FortificationChance = FortificationCheck.FortificationChance;
            evt.FortificationRoll = FortificationCheck.Roll;

            if (!FortificationCheck.IsPassed)
            {
                evt.FortificationNegatesSneakAttack = evt.IsSneakAttack;
                evt.FortificationNegatesCriticalHit = evt.IsCriticalConfirmed;
                evt.IsSneakAttack = false;
                evt.IsCriticalConfirmed = false;
                evt.PreciseStrike = 0;
            }
        }
    }
}