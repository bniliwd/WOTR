using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using System;
using System.Linq;
using System.Reflection;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace MyOwnMods
{
    public class Main
    {
        public static bool Enabled;

        //private static UnityModManager.ModEntry _modEntry;

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool toggleOn)
        {
            Harmony harmonyInstance = new(modEntry.Info.Id);
            if (toggleOn)
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmonyInstance.UnpatchAll(modEntry.Info.Id);
            }
            Enabled = toggleOn;
            return true;
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            //_modEntry = modEntry;
            modEntry.OnToggle = OnToggle;
            new Harmony(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        //internal static void Log(string text)
        //{
        //    _modEntry.Logger.Log(text);
        //}

        [HarmonyPatch(typeof(AbilityData))]
        static class IsActionFullRound_Patch
        {
            public static bool OverwriteFullRound(AbilityData __instance)
            {
				/**
                 * 1.非自发施法
                 * 2.或无超魔列表
                 * 3.或原定义是整轮
                 * 不可以重写动作类型
                 */
                if (!__instance.IsSpontaneous
                    || __instance.MetamagicData?.NotEmpty != true
                    || __instance.Blueprint.IsFullRoundAction)
                    return false;
                
                UnitMechanicFeatures features = __instance.Caster?.State.Features;
                MetamagicData metamagicData = __instance.MetamagicData;
                bool favorite;

                if (metamagicData.Has(Metamagic.CompletelyNormal)) {
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
                } else {
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

            //[HarmonyPatch("GetDefaultActionType")]
            //[HarmonyPostfix]
            //public static void GetDefaultActionType(AbilityData __instance, ref UnitCommand.CommandType __result)
            //{

            //    Log("GetDefaultActionType In1：" + "---" + (j++));
            //    if (__result != UnitCommand.CommandType.Standard)
            //        return;

            //    if (OverwriteFullRound(__instance))
            //        __result = __instance.Blueprint.ActionType;
            //}
        }
    }
}
