using HarmonyLib;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(AbilityData))]
    public static class Patch_IsActionFullRound
    {
        public static bool OverwriteFullRound(AbilityData __instance)
        {
            /**
             * 1.非自发施法 2.无超魔列表 3.原定义是整轮
             * 不可以重写动作类型
             */
            if (!__instance.IsSpontaneous
                || __instance.MetamagicData?.NotEmpty != true
                || __instance.Blueprint.IsFullRoundAction)
                return false;

            UnitMechanicFeatures features = __instance.Caster?.State.Features;
            MetamagicData metamagicData = __instance.MetamagicData;
            bool favorite;

            if (metamagicData.Has(Metamagic.CompletelyNormal))
            {
                favorite = true;
            }
            else if (features == null
                || (metamagicData.Has(Metamagic.Empower) && !(bool)features.FavoriteMetamagicEmpower)
                || (metamagicData.Has(Metamagic.Maximize) && !(bool)features.FavoriteMetamagicMaximize)
                || (metamagicData.Has(Metamagic.Extend) && !(bool)features.FavoriteMetamagicExtend)
                || (metamagicData.Has(Metamagic.Heighten))
                || (metamagicData.Has(Metamagic.Reach) && !(bool)features.FavoriteMetamagicReach)
                || (metamagicData.Has(Metamagic.Persistent) && !(bool)features.FavoriteMetamagicPersistent)
                || (metamagicData.Has(Metamagic.Selective) && !(bool)features.FavoriteMetamagicSelective)
                || (metamagicData.Has(Metamagic.Bolstered) && !(bool)features.FavoriteMetamagicBolstered)
                || (metamagicData.Has(Metamagic.Piercing) && !(bool)features.FavoriteMetamagicPiercing)
                || (metamagicData.Has(Metamagic.Intensified) && !(bool)features.FavoriteMetamagicIntensified))
            {
                favorite = false;
            }
            else
            {
                favorite = true;
            }

            /**
             * 4.法术为超正常或有对应偏好超魔
             * 可以重写动作类型
             */
            if (favorite)
                return true;

            return false;
        }

        [HarmonyPatch(nameof(AbilityData.RequireFullRoundAction), MethodType.Getter)]
        [HarmonyPostfix]
        public static void RequireFullRoundAction(AbilityData __instance, ref bool __result)
        {
            //原来就不需要整轮的不判断
            if (!__result)
                return;

            if (OverwriteFullRound(__instance))
                __result = false;
        }
    }
}