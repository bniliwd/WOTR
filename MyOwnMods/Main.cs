using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Craft;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityModManagerNet;

namespace BetterGameplay
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

                object[] parameters = { crafter, blueprintAbility, null };
                object result = AccessTools.DeclaredMethod(typeof(CraftRoot), "TryFindAbilityInSpellbooks")?.Invoke(__instance, parameters);
                if (blueprintAbility != null && (bool)result)
                {
                    CraftRequirements[] array = (CraftRequirements[])(AccessTools.Field(typeof(CraftRoot), "m_ScrollsRequirements")?.GetValue(__instance));
                    Spellbook spellbook = (Spellbook)parameters[2];
                    int minSpellLevel = spellbook.GetMinSpellLevel(blueprintAbility);
                    if (minSpellLevel < 0) minSpellLevel = spellbook.GetMinSpellLevel(blueprintAbility.Parent);
                    CraftRequirements craftRequirements = ((minSpellLevel >= 0 && minSpellLevel < array.Length) ? array[minSpellLevel] : new CraftRequirements());
                    ItemsCollection inventory = Game.Instance.Player.Inventory;
                    craftAvailInfo.IsHaveFeature = craftRequirements.RequiredFeature == null || crafter.HasFact((BlueprintFeature)craftRequirements.RequiredFeature);
                    craftAvailInfo.IsHaveItem = craftRequirements.RequiredItems.Count == 0 || inventory.ContainsAtLeastOneOf(craftRequirements.RequiredItems);
                    craftAvailInfo.IsHaveResources = isInProgress || craftInfo.CraftCost.Count == 0 || (bool)(AccessTools.DeclaredMethod(typeof(CraftRoot), "CheckResources")?.Invoke(__instance, [craftInfo]));
                }
                else
                {
                    craftAvailInfo.IsKnowAbility = false;
                }

                craftAvailInfo.Info = craftInfo;
                __result = craftAvailInfo;
                return false;
            }

        }

        [HarmonyPatch(typeof(BlueprintsCache))]
        static class BlueprintsCache_Patch
        {
            static bool loaded = false;

            //单位id，装备id...
            private static readonly List<List<string>> UNITS = [
                    ["d6301782519486e4aacdca6fa3140452", "5e94d9ac3448c774db72a51007aab1df"], //坎娜布利死灵师-次等完美要素腰带
                    ["082edd80ca45af84ab602fbe06e53f55", "841d3d5dfb09f6c468f4fae3567937ee"], //贤希尔-高等完美要素
                    ["b77c75af5cbea5a4db030f3e0331c693", "8762d9897514dd241b19e32ee9cf3f54", "bc195fd6225959c439bee1088a83aa6b", "e3acbfd13b857ed41a1ba8c330824f7d"], //百面-脱身弩，完美要素腰带，风暴眼
                    ["aedf061b223b6d54e8ffae6175515b29", "459cff3f67aea3846a228e0119f5e522"], //伊吉菲列斯-梦想家的微笑
                    ["d8fe12b59bb7e6b4cacb2045a36595a8", "bfa671a8f5e9f8549ba0abc86f550c9f", "e82d860e41a13534ea605fa0b00a9bb0"], //艾栗契诺-魔鬼束腰，狙杀
                    ["dcd200c627536c449bc8258eada65c9f", "d9f0df00399b0f246ac75a6339342719"], //索寇贝诺-灭迹
                    ["91cd42858e372f845ae2c82554412101", "e9c38a2a8d25ddf44a12e57ed6f003f7"], //卑微的怪似魔-祛除者
                    ["6711486b5e2ea36489dc5b97aa608c08", "7188029fda598f749a9dfbf23bec9bdc"], //雯朵格-猎人的帽子
                    ["c114bf22097668b459767f0b20c995cc", "b3977451f2d9e64498b3208bd78a5394"], //霍乱扎德-完璧
                    ["43fed636cbf213640a6953011b9e7cd7", "5bd6fafea3ee43c42abb7bec222afa11"], //卵石蹄-高等平衡
                    ["1b5fc80bcedff624b805b950a2244165", "7981ac4c52f4dbe4fabb19884aaacea3"], //永恒守卫-刺蜥兽革甲
                    ["4b7ba40dd891acf4bbb0582d65b6f506", "c76341cfbc0d0f14ea51ceb30efee432"], //黑水-卡菈酋长-装置法则
                    ["eda9ddab61fd61e4eb2a860315d317c5", "2a45458f776442e43bba57de65f9b738"], //普通食魂厉魔-+1匕首
                    ["5579ed7312bd77d43a7b97a4112d569f", "acda88693ce4a2b46bcb634fcc4e0a95"]  //高级食魂厉魔-+2匕首
                ];

            private static readonly Dictionary<string, FieldInfo> _fieldCache = [];

            [HarmonyPatch(nameof(BlueprintsCache.Init))]
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (loaded) return;
                loaded = true;

                PFLog.Mods.Log("加载掉落补丁开始...");

                foreach (List<string> combinations in UNITS)
                {
                    BlueprintUnit unit = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>(combinations[0]);

                    foreach (string id in combinations.Skip(1))
                    {
                        BlueprintItemEquipment unknown = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipment>(id);
                        if (unknown == null) continue;
                        FieldInfo partField;
                        switch(unknown)
                        {
                            //暂不支持同类型多部位(主副手，替换装，戒指，快捷物品)
                            case BlueprintItemEquipmentHand m_PrimaryHand://主手
                                partField = GetCachedField("m_PrimaryHand");
                                partField.SetValue(unit.Body, m_PrimaryHand.ToReference<BlueprintItemEquipmentHandReference>());
                                break;
                            case BlueprintItemArmor m_Armor:
                                partField = GetCachedField("m_Armor");
                                partField.SetValue(unit.Body, m_Armor.ToReference<BlueprintItemArmorReference>());
                                break;
                            case BlueprintItemEquipmentShirt m_Shirt:
                                partField = GetCachedField("m_Shirt");
                                partField.SetValue(unit.Body, m_Shirt.ToReference<BlueprintItemEquipmentShirtReference>());
                                break;
                            case BlueprintItemEquipmentBelt m_Belt:
                                partField = GetCachedField("m_Belt");
                                partField.SetValue(unit.Body, m_Belt.ToReference<BlueprintItemEquipmentBeltReference>());
                                break;
                            case BlueprintItemEquipmentHead m_Head:
                                partField = GetCachedField("m_Head");
                                partField.SetValue(unit.Body, m_Head.ToReference<BlueprintItemEquipmentHeadReference>());
                                break;
                            case BlueprintItemEquipmentGlasses m_Glasses:
                                partField = GetCachedField("m_Glasses");
                                partField.SetValue(unit.Body, m_Glasses.ToReference<BlueprintItemEquipmentGlassesReference>());
                                break;
                            case BlueprintItemEquipmentFeet m_Feet:
                                partField = GetCachedField("m_Feet");
                                partField.SetValue(unit.Body, m_Feet.ToReference<BlueprintItemEquipmentFeetReference>());
                                break;
                            case BlueprintItemEquipmentGloves m_Gloves:
                                partField = GetCachedField("m_Gloves");
                                partField.SetValue(unit.Body, m_Gloves.ToReference<BlueprintItemEquipmentGlovesReference>());
                                break;
                            case BlueprintItemEquipmentNeck m_Neck:
                                partField = GetCachedField("m_Neck");
                                partField.SetValue(unit.Body, m_Neck.ToReference<BlueprintItemEquipmentNeckReference>());
                                break;
                            case BlueprintItemEquipmentRing m_Ring1:
                                //临时方案
                                partField = GetCachedField(combinations[0].StartsWith("91cd4") ? "m_Ring2" : "m_Ring1");
                                partField.SetValue(unit.Body, m_Ring1.ToReference<BlueprintItemEquipmentRingReference>());
                                break;
                            case BlueprintItemEquipmentWrist m_Wrist:
                                partField = GetCachedField("m_Wrist");
                                partField.SetValue(unit.Body, m_Wrist.ToReference<BlueprintItemEquipmentWristReference>());
                                break;
                            case BlueprintItemEquipmentShoulders m_Shoulders:
                                partField = GetCachedField("m_Shoulders");
                                partField.SetValue(unit.Body, m_Shoulders.ToReference<BlueprintItemEquipmentShouldersReference>());
                                break;
                            default:
                                PFLog.Mods.Error("Unkownd Item Type: " + unknown.ToString());
                                break;
                                //case BlueprintItemEquipmentUsable[] m_QuickSlots;
                        }
                    }

                }

                PFLog.Mods.Log("加载掉落补丁完成，移除缓存内容");
                UNITS.Clear();
                _fieldCache.Clear();
            }

            public static FieldInfo GetCachedField(string fieldName)
            {
                if (!_fieldCache.TryGetValue(fieldName, out var field))
                {
                    field = typeof(BlueprintUnit.UnitBody).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                    _fieldCache[fieldName] = field;
                }
                return field;
            }

        }

        [HarmonyPatch(typeof(ItemStatHelper))]
        static class ItemStatHelper_Patch
        {
            [HarmonyPatch(nameof(ItemStatHelper.GetDC), [typeof(ItemEntity), typeof(UnitEntityData)])]
            [HarmonyPrefix]
            public static bool GetDC(ItemEntity item, UnitEntityData caster, ref int __result)
            {
                if (item.Blueprint is not BlueprintItemEquipment blueprintItemEquipment)
                {
                    PFLog.Default.Error(item.Name + " is not usable and not have DC");
                    __result = 0;
                    return false;
                }

                CraftedItemPart craftedItemPart = item.Get<CraftedItemPart>();
                UsableItemType? usableItemType = (blueprintItemEquipment is BlueprintItemEquipmentUsable blueprintItemEquipmentUsable)
                    ? new UsableItemType?(blueprintItemEquipmentUsable.Type) : null;

                int potentialDC = 0;
                if (caster.State.Features.WandMastery && usableItemType == UsableItemType.Wand)
                {//魔杖大师
                    potentialDC = caster.Stats.Intelligence.Bonus + item.GetSpellLevel() + 10;
                }
                else if (caster.State.Features.EldritchWandMastery && usableItemType == UsableItemType.Wand)
                {//魔杖大师变体
                    potentialDC = caster.Stats.Charisma.Bonus + item.GetSpellLevel() + 10;
                }
                else if (caster.State.Features.ScrollMastery && usableItemType == UsableItemType.Scroll)
                {//卷轴掌控
                    potentialDC = caster.Stats.Intelligence.Bonus + item.GetSpellLevel() + 10;
                }

                //获取使用者最高DC和物品DC取高
                __result = Math.Max(potentialDC, craftedItemPart?.AbilityDC ?? blueprintItemEquipment.DC);
                return false;
            }
        }

        [HarmonyPatch(typeof(WeaponFocusParametrized))]
        static class WeaponFocusParametrized_Patch
        {
            private static readonly Dictionary<string, PropertyInfo> _propertyCache = [];
        
            [HarmonyPatch(nameof(WeaponFocusParametrized.OnEventAboutToTrigger))]
            [HarmonyPrefix]
            public static bool OnEventAboutToTrigger(WeaponFocusParametrized __instance, RuleCalculateAttackBonusWithoutTarget evt)
            {
                FeatureParam baseParam = (FeatureParam)(GetCachedProperty("Param")?.GetValue(__instance));
                UnitFact baseFact = (UnitFact)(GetCachedProperty("Fact")?.GetValue(__instance));
                if (baseFact == null) return true;
        
                bool hasMythicFocus = evt.Initiator.Progression.Features.Enumerable.Any((Feature p) => p.Param == baseParam && p.Blueprint == __instance.MythicFocus);
                int bonus = hasMythicFocus ? baseFact.NameForAcronym.EndsWith("Greater", StringComparison.Ordinal) ? 3 : 2 : 1;
                if (evt.Weapon.Blueprint.Type.Category == baseParam)
                {
                    evt.AddModifier(bonus, baseFact, __instance.Descriptor);
                }
                return false;
            }
        
            public static PropertyInfo GetCachedProperty(string name)
            {
                if (!_propertyCache.TryGetValue(name, out var field))
                {
                    field = typeof(UnitFactComponentDelegate<EmptyComponentData>).GetProperty(name, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
                    _propertyCache[name] = field;
                }
                return field;
            }
        
        }

        [HarmonyPatch(typeof(Logger))]
        static class Logger_Patch
        {
            [HarmonyPatch(nameof(Logger.Log), [typeof(LogChannel), typeof(object), typeof(LogSeverity), typeof(Exception), typeof(object), typeof(object[])])]
            [HarmonyPrefix]
            public static bool LogPrefix()
            {
                return false;
            }
        }
    }
}


