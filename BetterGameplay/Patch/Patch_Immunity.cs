using BetterGameplay.Logic;
using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static BetterGameplay.Logic.UnitPartCustomMechanicsFeatures;

namespace BetterGameplay.Patch
{
    [ClassInfoBox("改自ttt")]
    [HarmonyPatch]
    static class Patch_MoreImmunityIngore
    {
        private static readonly MethodInfo bypassCheckMethod = AccessTools.Method(typeof(CustomMechanicsFeaturesExtentions),
                nameof(CustomMechanicsFeaturesExtentions.CustomMechanicsFeature),
                [typeof(UnitEntityData), typeof(CustomMechanicsFeature)]);
        private static readonly FieldInfo initiatorField = AccessTools.Field(typeof(RuleCalculateDamage), nameof(RuleCalculateDamage.Initiator));
        private static readonly MethodInfo getValueMethod = AccessTools.PropertyGetter(typeof(CountableFlag), nameof(CountableFlag.Value));

        static List<CodeInstruction> CreateBypassMethod(ILGenerator generator, CustomMechanicsFeature feature)
        {
            var skipCheckLabel = generator.DefineLabel();

            // 创建要插入的条件检查逻辑
            var newCodes = new List<CodeInstruction>()
            {
                new(OpCodes.Ldarg_1),                   //访问evt.Initiator
                new(OpCodes.Ldfld, initiatorField),
                new(OpCodes.Dup),                       //检查是否为null
                new(OpCodes.Brfalse_S, skipCheckLabel), //跳转点1
                new(OpCodes.Ldarg_1),                   //访问evt.Initiator
                new(OpCodes.Ldfld, initiatorField),
                new(OpCodes.Ldc_I4, (int)feature),      //加载BypassSneakAttackImmunity枚举值
                new(OpCodes.Call, bypassCheckMethod),   //调用CustomMechanicsFeature方法
                new(OpCodes.Callvirt, getValueMethod),  //检查结果是否为true
                new(OpCodes.Brfalse_S, skipCheckLabel), //跳转点2
                new(OpCodes.Pop),                       //如果条件为true，清理栈并return 
                new(OpCodes.Ret),
                new(new CodeInstruction(OpCodes.Pop) { labels = [skipCheckLabel] })// 标记跳转位置
            };

            return newCodes;
        }

        //计算精准，代码较多
        [HarmonyPatch(typeof(AddImmunityToPrecisionDamage), nameof(AddImmunityToPrecisionDamage.OnEventAboutToTrigger), [typeof(RuleCalculateDamage)])]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilerCalculate(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (codes.Count > 30)
            {
                return codes;
            }

            var newCodes = CreateBypassMethod(generator, CustomMechanicsFeature.BypassSneakAttackImmunity);

            codes.InsertRange(0, newCodes);

            return codes;
        }

        //免疫精准
        [HarmonyPatch(typeof(AddImmunityToPrecisionDamage), nameof(AddImmunityToPrecisionDamage.OnEventAboutToTrigger), [typeof(RuleAttackRoll)])]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilerAttack(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (codes.Count > 5)
            {
                return codes;
            }

            var newCodes = CreateBypassMethod(generator, CustomMechanicsFeature.BypassSneakAttackImmunity);

            codes.InsertRange(0, newCodes);

            return codes;
        }

        //计算重击，免疫重击
        [HarmonyPatch(typeof(AddImmunityToCriticalHits), nameof(AddImmunityToCriticalHits.OnEventAboutToTrigger), [typeof(RuleCalculateDamage)])]
        [HarmonyPatch(typeof(AddImmunityToCriticalHits), nameof(AddImmunityToCriticalHits.OnEventAboutToTrigger), [typeof(RuleDealStatDamage)])]
        [HarmonyPatch(typeof(AddImmunityToCriticalHits), nameof(AddImmunityToCriticalHits.OnEventAboutToTrigger), [typeof(RuleAttackRoll)])]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilerCriticalImmunity(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (codes.Count > 5)
            {
                return codes;
            }

            var newCodes = CreateBypassMethod(generator, CustomMechanicsFeature.BypassCriticalHitImmunity);

            codes.InsertRange(0, newCodes);

            return codes;
        }
    }
}