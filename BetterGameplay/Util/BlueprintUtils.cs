using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using static Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite;

namespace BetterGameplay.Util
{
    internal static class BlueprintUtils
    {
        public static T CreateBlueprint<T>([NotNull] BlueprintGuid guid, [NotNull] string name, Action<T> init = null) where T : SimpleBlueprint, new()
        {
            T val = new()
            {
                AssetGuid = guid,
                name = name
            };
            AddBlueprint(val, guid);
            SetRequiredBlueprintFields(val);
            init?.Invoke(val);
            return val;
        }

        public static void AddBlueprint([NotNull] SimpleBlueprint blueprint, [NotNull] BlueprintGuid assetId)
        {
            SimpleBlueprint simpleBlueprint = ResourcesLibrary.TryGetBlueprint(assetId);
            if (simpleBlueprint == null)
            {
                ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(assetId, blueprint);
                blueprint.OnEnable();
            }
        }

        private static void SetRequiredBlueprintFields(SimpleBlueprint blueprint)
        {
            if (blueprint is not BlueprintBuff blueprintBuff)
            {
                if (blueprint is BlueprintFeature blueprintFeature)
                {
                    blueprintFeature.IsClassFeature = true;
                }
            }
            else
            {
                blueprintBuff.FxOnStart = new PrefabLink();
                blueprintBuff.FxOnRemove = new PrefabLink();
                blueprintBuff.IsClassFeature = true;
            }
        }

        public static T GetBlueprint<T>(string id) where T : BlueprintScriptableObject
        {
            return ResourcesLibrary.TryGetBlueprint<T>(id);
        }

        public static T GetBlueprint<T>(BlueprintGuid guid) where T : BlueprintScriptableObject
        {
            return ResourcesLibrary.TryGetBlueprint<T>(guid);
        }

        //事先知道蓝图类型可以用这个
        public static T GetBlueprintReference<T>(string id) where T : BlueprintReferenceBase
        {
            T val = Activator.CreateInstance<T>();
            val.deserializedGuid = BlueprintGuid.Parse(id);
            return val;
        }

        public static void SetComponents(this BlueprintScriptableObject obj, params BlueprintComponent[] components)
        {
            HashSet<string> hashSet = [];
            foreach (BlueprintComponent blueprintComponent in components)
            {
                if (string.IsNullOrEmpty(blueprintComponent.name))
                {
                    blueprintComponent.name = "$" + blueprintComponent.GetType().Name;
                }

                if (!hashSet.Add(blueprintComponent.name))
                {
                    int num = 0;
                    string name;
                    while (!hashSet.Add(name = $"{blueprintComponent.name}${num}"))
                    {
                        num++;
                    }

                    blueprintComponent.name = name;
                }
            }

            obj.ComponentsArray = components;
            obj.OnEnable();
        }

        public static void AddComponent<T>(this BlueprintScriptableObject obj, Action<T> init = null) where T : BlueprintComponent, new()
        {
            obj.SetComponents(obj.ComponentsArray.AppendToArray(Create(init)));
        }

        public static void AddComponent(this BlueprintScriptableObject obj, BlueprintComponent component)
        {
            obj.SetComponents(obj.ComponentsArray.AppendToArray(component));
        }

        //public static void RemoveComponents(this BlueprintScriptableObject obj)
        //{
        //    obj.SetComponents([]);
        //}

        public static void RemoveComponents<T>(this BlueprintScriptableObject obj) where T : BlueprintComponent
        {
            T[] array = [.. obj.GetComponents<T>()];
            foreach (T value in array)
            {
                obj.SetComponents(obj.ComponentsArray.RemoveFromArray(value));
            }
        }

        public static void RemoveComponents<T>(this BlueprintScriptableObject obj, Predicate<T> predicate) where T : BlueprintComponent
        {
            T[] array = [.. obj.GetComponents<T>()];
            foreach (T val in array)
            {
                if (predicate(val))
                {
                    obj.SetComponents(obj.ComponentsArray.RemoveFromArray(val));
                }
            }
        }

        public static T[] AppendToArray<T>(this T[] array, T value)
        {
            int num = array != null ? array.Length : 0;
            T[] array2 = new T[num + 1];
            if (num > 0)
            {
                Array.Copy(array, array2, num);
            }

            array2[num] = value;
            return array2;
        }

        public static T[] RemoveFromArray<T>(this T[] array, T value)
        {
            List<T> list = [.. array];
            if (!list.Remove(value))
            {
                return array;
            }

            return [.. list];
        }

        public static void TemporaryContext<T>(this T obj, Action<T> run)
        {
            run?.Invoke(obj);
        }

        public static T Create<T>(Action<T> init = null) where T : new()
        {
            T val = new();
            init?.Invoke(val);
            return val;
        }

        public static void AddPrerequisiteFeature(this BlueprintFeature obj, BlueprintFeature feature, GroupType group = GroupType.All)
        {
            obj.AddComponent<PrerequisiteFeature>(c => {
                c.m_Feature = feature.ToReference<BlueprintFeatureReference>();
                c.Group = group;
            });
            feature.IsPrerequisiteFor ??= [];
            if (!feature.IsPrerequisiteFor.Contains(obj.ToReference<BlueprintFeatureReference>()))
            {
                feature.IsPrerequisiteFor.Add(obj.ToReference<BlueprintFeatureReference>());
            }
        }

        public static ActionList CreateActionList(params GameAction[] actions)
        {
            if (actions == null || actions.Length == 1 && actions[0] == null) actions = [];
            return new ActionList() { Actions = actions };
        }
        public static void AddAction(this ActionList actionList, GameAction action)
        {
            if (action == null) { return; }
            actionList.Actions = actionList.Actions.AppendToArray(action);
        }
        public static void RemoveActions<T>(this ActionList actionList, Predicate<T> predicate) where T : GameAction
        {
            var actionsToRemove = actionList.Actions.OfType<T>().ToArray();
            foreach (var action in actionsToRemove)
            {
                if (predicate(action))
                {
                    actionList.Actions = actionList.Actions.RemoveFromArray(action);
                }
            }
        }

        public static void AddFacts(this BlueprintScriptableObject obj, Action<AddFacts> init = null)
        {
            obj.AddComponent(CreateAddFacts(init));
        }

        public static AddFacts CreateAddFacts(Action<AddFacts> init)
        {
            AddFacts addFacts = new()
            {
                m_Flags = 0,
                m_Facts = [],
                DoNotRestoreMissingFacts = false,
                HasDifficultyRequirements = false,
                InvertDifficultyRequirements = false,
                MinDifficulty = GameDifficultyOption.Story
            };
            init?.Invoke(addFacts);
            return addFacts;
        }

        public static void AddContextRankConfig(this BlueprintScriptableObject obj, Action<ContextRankConfig> init = null)
        {
            obj.AddComponent(CreateContextRankConfig(init));
        }

        public static ContextRankConfig CreateContextRankConfig(Action<ContextRankConfig> init)
        {
            ContextRankConfig contextRankConfig = CreateContextRankConfig();
            init?.Invoke(contextRankConfig);
            return contextRankConfig;
        }

        public static ContextRankConfig CreateContextRankConfig(ContextRankBaseValueType baseValueType = ContextRankBaseValueType.CasterLevel, ContextRankProgression progression = ContextRankProgression.AsIs, 
            AbilityRankType AbilityRankType = AbilityRankType.Default, int? min = null, int? max = null, int startLevel = 0, int stepLevel = 0, 
            bool exceptClasses = false, StatType stat = StatType.Unknown, BlueprintUnitProperty customProperty = null, 
            BlueprintCharacterClass[] classes = null, BlueprintArchetype[] archetypes = null, 
            BlueprintArchetype archetype = null, BlueprintFeature feature = null, BlueprintFeature[] featureList = null)
        {
            return new ContextRankConfig
            {
                m_Type = AbilityRankType,
                m_BaseValueType = baseValueType,
                m_Progression = progression,
                m_UseMin = min.HasValue,
                m_Min = min.GetValueOrDefault(),
                m_UseMax = max.HasValue,
                m_Max = max.GetValueOrDefault(),
                m_StartLevel = startLevel,
                m_StepLevel = stepLevel,
                m_Feature = feature.ToReference<BlueprintFeatureReference>(),
                m_ExceptClasses = exceptClasses,
                m_CustomProperty = customProperty.ToReference<BlueprintUnitPropertyReference>(),
                m_Stat = stat,
                m_Class = (classes == null) ? [] : [.. classes.Select(c => c.ToReference<BlueprintCharacterClassReference>())],
                Archetype = archetype.ToReference<BlueprintArchetypeReference>(),
                m_AdditionalArchetypes = (archetypes == null) ? [] : [.. archetypes.Select(c => c.ToReference<BlueprintArchetypeReference>())],
                m_FeatureList = (featureList == null) ? [] : [.. featureList.Select(c => c.ToReference<BlueprintFeatureReference>())]
            };
        }

        public static void PatchDefaultEnchantments(List<BlueprintItemReference> references,
            List<BlueprintWeaponEnchantmentReference> enchantments, int charge = 1, bool spendCharge = true)
        {
            var validEnchantments = enchantments.Where(ec => ec != null).ToList();
            foreach (var reference in references)
            {
                if (reference?.Get() is BlueprintItemWeapon weapon)
                {
                    weapon.SpendCharges = spendCharge;
                    weapon.Charges = charge;
                    weapon.m_Enchantments = [.. validEnchantments];
                }
            }
        }
    }
}