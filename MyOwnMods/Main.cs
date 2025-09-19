using Epic.OnlineServices.AntiCheatCommon;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Craft;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityModManagerNet;

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
                 * 2.无超魔列表
                 * 3.原定义是整轮
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

        [HarmonyPatch(typeof(CraftRoot))]
        static class CraftRoot_Patch
        {
            [HarmonyPatch(nameof(CraftRoot.CheckCraftAvail))]
            [HarmonyPrefix]
            public static bool CheckCraftAvail(CraftRoot __instance, UnitEntityData crafter, CraftItemInfo craftInfo, bool isInProgress, ref CraftAvailInfo __result)
            {
                //不是卷轴就用原逻辑
                if (craftInfo.Item.Type != UsableItemType.Scroll) return true;

                CraftAvailInfo craftAvailInfo = new();
                BlueprintAbility blueprintAbility = craftInfo.Item?.Ability;
                if (blueprintAbility != null && TryFindAbilityInSpellbooks(crafter, blueprintAbility, out var spellbook))
                {
                    CraftRequirements[] array = (CraftRequirements[])(AccessTools.Field(typeof(CraftRoot), "m_ScrollsRequirements")?.GetValue(__instance));
                    int minSpellLevel = spellbook.GetMinSpellLevel(blueprintAbility);
                    if (minSpellLevel < 0) minSpellLevel = spellbook.GetMinSpellLevel(blueprintAbility.Parent);
                    CraftRequirements craftRequirements = ((minSpellLevel >= 0 && minSpellLevel < array.Length) ? array[minSpellLevel] : new CraftRequirements());
                    ItemsCollection inventory = Game.Instance.Player.Inventory;
                    craftAvailInfo.IsHaveFeature = craftRequirements.RequiredFeature == null || crafter.HasFact((BlueprintFeature)craftRequirements.RequiredFeature);
                    craftAvailInfo.IsHaveItem = craftRequirements.RequiredItems.Count == 0 || inventory.ContainsAtLeastOneOf(craftRequirements.RequiredItems);
                    craftAvailInfo.IsHaveResources = isInProgress || craftInfo.CraftCost.Count == 0 || CheckResources(craftInfo);
                }
                else
                {
                    craftAvailInfo.IsKnowAbility = false;
                }

                craftAvailInfo.Info = craftInfo;
                __result = craftAvailInfo;
                return false;
            }

            private static bool TryFindAbilityInSpellbooks(UnitEntityData crafter, BlueprintAbility abillity, out Spellbook spellbook)
            {
                spellbook = null;
                using (PooledHashSet<Spellbook> pooledHashSet = PooledHashSet<Spellbook>.Get())
                {
                    foreach (Spellbook spellbook2 in crafter.Spellbooks)
                    {
                        if (spellbook2.IsKnown(abillity) || (abillity.Parent != null && spellbook2.IsKnown(abillity.Parent)))
                        {
                            pooledHashSet.Add(spellbook2);
                        }
                    }
                    spellbook = pooledHashSet.MinBy((Spellbook x) => x.GetMinSpellLevel(abillity));
                }
                return spellbook != null;
            }

            private static bool CheckResources(CraftItemInfo itemInfo)
            {
                ItemsCollection inventory = Game.Instance.Player.Inventory;
                foreach (BlueprintIngredient.Reference item in itemInfo.CraftCost)
                {
                    if (!inventory.Contains((BlueprintIngredient)item))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
