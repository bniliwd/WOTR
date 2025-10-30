using BetterGameplay.Logic;
using BetterGameplay.Rule;
using BetterGameplay.Util;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Modify
{
    public static class ModifyMythicFeat
    {
        public static void MultiModify()
        {
            ModifyMythicArmor();
            ModifyImprovedCritical();
            ModifyWeaponFocus();
            ModifyPointBlankShot();
        }

        public static void ModifyImprovedCritical()
        {
            Type RuleType = typeof(RuleFortificationCheck);
            if (!GameLogEventsFactory.Creators.ContainsKey(RuleType))
            {
                Type gameLogEventType = typeof(GameLogRuleEvent<>).MakeGenericType([RuleType]);
                GameLogEventsFactory.Creators.Add(RuleType, rule => (GameLogEvent)Activator.CreateInstance(gameLogEventType, [rule]));
            }

            BlueprintParametrizedFeature feature = GetBlueprint<BlueprintParametrizedFeature>("8bc0190a4ec04bd489eec290aeaa6d07");
            feature.AddComponent<RerollFortification>(c => {
                c.ConfirmedCriticals = true;
                c.SneakAttacks = false;
                c.PreciseStrike = false;
                c.RerollCount = 1;
                c.TakeBest = true;
            });
        }

        public static void ModifyWeaponFocus()
        {
            BlueprintParametrizedFeature weaponFocusGreater = GetBlueprint<BlueprintParametrizedFeature>("09c9e82965fb4334b984a1e9df3bd088");

            weaponFocusGreater.RemoveComponents<WeaponFocusParametrized>();
            weaponFocusGreater.AddComponent<WeaponFocusGreaterParametrized>(p =>
            {
                p.m_MythicFocus = GetBlueprintReference<BlueprintUnitFactReference>("74eb201774bccb9428ba5ac8440bf990");
                p.Descriptor = ModifierDescriptor.UntypedStackable;
            });
        }

        public static void ModifyPointBlankShot()
        {
            BlueprintFeature feature = GetBlueprint<BlueprintFeature>("b651baa5f6faba646b7e3082929dfaf5");
            feature.SetComponents(
                new DistanceAttackBonusRe()
                {
                    Close = 15.Feet(),
                    Range = 30.Feet(),
                    Descriptor = ModifierDescriptor.UntypedStackable,
                    AttackBonus = 1
                },
                new DistanceDamageBonusRe()
                {
                    Close = 15.Feet(),
                    Range = 30.Feet(),
                    DamageBonus = 1
                },
                new PrerequisiteFeature()
                {
                    m_Feature = GetBlueprintReference<BlueprintFeatureReference>("0da0c194d6e1d43419eb8d990b28e0ab")
                }
            );
        }

        public static void ModifyMythicArmor()
        {
            //秘银移速惩罚下调一级
            var MithralEnchantment = GetBlueprint<BlueprintArmorEnchantment>("7b95a819181574a4799d93939aa99aff");
            var MithralFeature = GetBlueprint<BlueprintFeature>("0e5e0e709a16f6240b609616a6dbe916");

            MithralFeature.AddComponent<ArmorSpeedPenaltyRemoval>();
            MithralEnchantment.AddComponent<AddUnitFactEquipment>(fa =>
            {
                fa.m_Blueprint = CreateBlueprint<BlueprintFeature>(
                    BlueprintGuid.Parse("bf6c43ae480b4b8ab40f623e7c7135e1"), "MirthralSpeedFeature",
                    bp =>
                    {
                        LocalizedString empty = new() { m_Key = "" };
                        bp.AddComponent<ArmorSpeedPenaltyRemoval>();
                        bp.m_DisplayName = empty;
                        bp.m_Description = empty;
                        bp.HideInUI = true;
                    }).ToReference<BlueprintUnitFactReference>();
            });

            //最大加值
            BlueprintUnitPropertyReference HalfBobus = CreateBlueprint<BlueprintUnitProperty>(
                BlueprintGuid.Parse("3cf427df509b41e58b87a9e44deead32"), "HalfMythicBonus",
                bp => {
                    bp.AddComponent<EquipmentBonusProperty>(p => {
                        p.scale = ContextRankProgression.Div2;
                    });
                }).ToReference<BlueprintUnitPropertyReference>();

            BlueprintUnitPropertyReference NormalBonus = CreateBlueprint<BlueprintUnitProperty>(
                BlueprintGuid.Parse("50593ff0a0d649779d8fcc868b565434"), "NormalMythicBonus",
                bp => {
                    bp.AddComponent<EquipmentBonusProperty>(p => {
                        p.scale = ContextRankProgression.AsIs;
                    });
                }).ToReference<BlueprintUnitPropertyReference>();

            BlueprintUnitPropertyReference HalfMoreBonus = CreateBlueprint<BlueprintUnitProperty>(
                BlueprintGuid.Parse("0897f17bca4846be88e52835ce1da117"), "HalfMoreMythicBonus", bp => {
                    bp.AddComponent<EquipmentBonusProperty>(p => {
                        p.scale = ContextRankProgression.HalfMore;
                    });
                }).ToReference<BlueprintUnitPropertyReference>();

            BlueprintUnitPropertyReference Step2Bonus = CreateBlueprint<BlueprintUnitProperty>(
                BlueprintGuid.Parse("2ebf9d3137504e0f98f7ee67708084f2"), "Step2Bonus", bp => {
                    bp.AddComponent<EquipmentBonusProperty>(p => {
                        p.scale = ContextRankProgression.OnePlusDivStep;
                    });
                }).ToReference<BlueprintUnitPropertyReference>();

            BlueprintUnitPropertyReference Step4Bonus = CreateBlueprint<BlueprintUnitProperty>(
                BlueprintGuid.Parse("82809f263d5d4d169a42ae82f3560814"), "Step4Bonus", bp => {
                    bp.AddComponent<EquipmentBonusProperty>(p => {
                        p.isArmor = false;
                        p.isShield = false;
                        p.scale = ContextRankProgression.DivStep;
                    });
                }).ToReference<BlueprintUnitPropertyReference>();

            Dictionary<string, BlueprintUnitPropertyReference> bonusLevel = new(){
                {"Half", HalfBobus}, {"Normal", NormalBonus}, {"HalfMore", HalfMoreBonus}, {"OneStep2", Step2Bonus }, {"Step4", Step4Bonus }
            };

            BlueprintBuff endurance = ModifyMythicHeavyArmor(bonusLevel);
            ModifyMythicMediumArmor(bonusLevel, endurance);
            ModifyMythicLightArmor(bonusLevel);
        }

        public static void ModifyMythicLightArmor(Dictionary<string, BlueprintUnitPropertyReference> bonusLevel)
        {
            //轻甲强攻
            //var LightArmorAssault = GetBlueprint<BlueprintFeature>("48168449f8ba465fab2843ba2dada063");
            //var LightArmorAssaultBuff = GetBlueprint<BlueprintBuff>("b4583316ff014878809e57fc24d19229");
            var LightArmorAssaultBuffEffect = GetBlueprint<BlueprintBuff>("dc9702301ee7464d99c95e847e4d94f6");

            //允许战士娴熟 上限1/2神话阶级
            BlueprintFeatureReference reference = GetBlueprintReference<BlueprintFeatureReference>("2f1619e253ea6a04087def71c7925715");
            WeaponParametersAttackBonus ab = LightArmorAssaultBuffEffect.GetComponent<WeaponParametersAttackBonus>();
            WeaponParametersDamageBonus db = LightArmorAssaultBuffEffect.GetComponent<WeaponParametersDamageBonus>();
            ab.CanBeUsedWithFightersFinesse = true;
            ab.m_FightersFinesse = reference;

            db.CanBeUsedWithFightersFinesse = true;
            db.m_FightersFinesse = reference;

            LightArmorAssaultBuffEffect.RemoveComponents<ContextRankConfig>();
            LightArmorAssaultBuffEffect.AddContextRankConfig(config =>
            {
                config.m_BaseValueType = ContextRankBaseValueType.CustomProperty;
                config.m_CustomProperty = bonusLevel["Half"];
            });

            //轻甲回避
            //var LightArmorAvoidance = GetBlueprint<BlueprintFeature>("40be409a44084b2998eda225dd9c544a");
            //var LightArmorAvoidanceBuff = GetBlueprint<BlueprintBuff>("b1217085b7004b11a719ea075fba8ea7");
            //var LightArmorAvoidanceBuffEffect = GetBlueprint<BlueprintBuff>("18a94772f0634cb1bd73ca41b86b8354");

            //轻甲耐受 法抗上限神话阶级
            //var LightArmorEndurance = GetBlueprint<BlueprintFeature>("3260cacb9fa64d3bba6f179cfb026abd");
            //var LightArmorEnduranceBuff = GetBlueprint<BlueprintBuff>("7c483825698f45bd8c1c1a6a9054e963");
            var LightArmorEnduranceBuffEffect = GetBlueprint<BlueprintBuff>("80bd5c6d5e0a483793a9ddff9ff42a95");
            //var LightArmorEnduranceExtraBuffEffect = GetBlueprint<BlueprintBuff>("f0079e9247e54be3a13d69f6e57cfc20");

            LightArmorEnduranceBuffEffect.RemoveComponents<ContextRankConfig>();
            LightArmorEnduranceBuffEffect.AddContextRankConfig(config =>
            {
                config.m_BaseValueType = ContextRankBaseValueType.CustomProperty;
                config.m_CustomProperty = bonusLevel["Normal"];
            });
        }

        public static void ModifyMythicMediumArmor(Dictionary<string, BlueprintUnitPropertyReference> bonusLevel, BlueprintBuff endurance)
        {
            //中甲强攻
            //var MediumArmorAssault = GetBlueprint<BlueprintFeature>("6c6b8ba0ad1141df9316ab0ee28cbb77");
            //var MediumArmorAssaultBuff = GetBlueprint<BlueprintBuff>("a9446282d567471496566a3b464559dc");
            var MediumArmorAssaultBuffEffect = GetBlueprint<BlueprintBuff>("8ddbd82754ac44c1990c212a3ed1f1c8");

            //命中1/4神话阶级
            MediumArmorAssaultBuffEffect.AddComponent<WeaponParametersAttackBonus>(
                ab =>
                {
                    ab.UseContextIstead = true;
                    ab.ContextAttackBonus = new ContextValue()
                    {
                        ValueType = ContextValueType.CasterCustomProperty,
                        m_CustomProperty = bonusLevel["Step4"]
                    };
                    ab.Descriptor = ModifierDescriptor.Armor;
                }
            );

            //挥砍伤害 上限1D6+(AC和1/2神话阶级取低)
            AdditionalDiceOnAttack MediumArmorOrigin = MediumArmorAssaultBuffEffect.GetComponent<AdditionalDiceOnAttack>();
            if (MediumArmorOrigin != null)
            {
                MediumArmorOrigin.DamageType = new DamageTypeDescription()
                {
                    Type = DamageType.Physical,
                    Physical = new DamageTypeDescription.PhysicalData()
                    {
                        Material = PhysicalDamageMaterial.Adamantite,
                        Form = PhysicalDamageForm.Slashing
                    }
                };
            }
            MediumArmorAssaultBuffEffect.RemoveComponents<ContextRankConfig>();
            MediumArmorAssaultBuffEffect.AddContextRankConfig(config =>
            {
                config.m_BaseValueType = ContextRankBaseValueType.CustomProperty;
                config.m_CustomProperty = bonusLevel["OneStep2"];
            });

            //中甲回避
            //var MediumArmorAvoidance = GetBlueprint<BlueprintFeature>("b152e7fdfc6a452c8c47dd76de83e1aa");
            var MediumArmorAvoidanceBuff = GetBlueprint<BlueprintBuff>("262ffe91ee5749c19005657c6aa5e818");
            var MediumArmorAvoidanceBuffEffect = GetBlueprint<BlueprintBuff>("61c15df2702845589fec0b77fda158fe");

            LocalizedString repair = new() { m_Key = "8fa6ae2f-8b95-426d-b22d-41a22d138c0b" };
            MediumArmorAvoidanceBuff.m_DisplayName = repair;
            MediumArmorAvoidanceBuffEffect.m_DisplayName = repair;
            

            //中甲耐受 DR 1+1/2神话阶级
            //var MediumArmorEndurance = GetBlueprint<BlueprintFeature>("4243c96836bf4b54aa4b36e1ab0c7fb1");
            //var MediumArmorEnduranceBuff = GetBlueprint<BlueprintBuff>("93e80467a5fc4e68927b99732484fbd4");
            var MediumArmorEnduranceBuffEffect = GetBlueprint<BlueprintBuff>("457d5cde294c435ea7b4ee66f4949956");

            MediumArmorEnduranceBuffEffect.AddComponent(endurance.GetComponent<AddDamageResistancePhysical>());
            MediumArmorEnduranceBuffEffect.RemoveComponents<AcAddAcBuff>();
            MediumArmorEnduranceBuffEffect.AddComponent<AcAddAcBuff>(ar =>
            {
                ar.name = "MediumArmorAcAddAcBuff";
                ar.m_BonusMod = BonusMod.Custom;
                ar.m_CustomProperty = bonusLevel["OneStep2"];
            });


            MediumArmorEnduranceBuffEffect.RemoveComponents<ContextRankConfig>();
            MediumArmorEnduranceBuffEffect.AddContextRankConfig(config =>
            {
                config.m_BaseValueType = ContextRankBaseValueType.CustomProperty;
                config.m_CustomProperty = bonusLevel["Half"];
            });
        }

        public static BlueprintBuff ModifyMythicHeavyArmor(Dictionary<string, BlueprintUnitPropertyReference> bonusLevel)
        {
            //重甲强攻
            var HeavyArmorAssault = GetBlueprint<BlueprintFeature>("037c3fde2ad14767a92768c9aa6d6de2");
            var HeavyArmorAssaultBuff = GetBlueprint<BlueprintBuff>("fcd913568e1544108eceb8db2a90cd0f");
            var HeavyArmorAssaultBuffEffect = GetBlueprint<BlueprintBuff>("5ec6fdb6fa4741798e3264c09e91c949");

            //1D6+(神话阶级和AC取低)钝击
            AdditionalDiceOnAttack HeavyArmorOrigin = HeavyArmorAssaultBuffEffect.GetComponent<AdditionalDiceOnAttack>();
            if (HeavyArmorOrigin != null)
            {
                HeavyArmorOrigin.DamageType = new DamageTypeDescription()
                {
                    Type = DamageType.Physical,
                    Physical = new DamageTypeDescription.PhysicalData()
                    {
                        Material = PhysicalDamageMaterial.Adamantite,
                        Form = PhysicalDamageForm.Bludgeoning
                    }
                };
            }
            HeavyArmorAssaultBuffEffect.RemoveComponents<ContextRankConfig>();
            HeavyArmorAssaultBuffEffect.AddContextRankConfig(config =>
            {
                config.m_BaseValueType = ContextRankBaseValueType.CustomProperty;
                config.m_CustomProperty = bonusLevel["Normal"];
            });

            //重甲回避
            //var HeavyArmorAvoidance = GetBlueprint<BlueprintFeature>("9b83bd09e7154b5f9c22a172efaaef4e");
            //var HeavyArmorAvoidanceBuff = GetBlueprint<BlueprintBuff>("95a2856c0369449db24000d2fb4e9277");
            var HeavyArmorAvoidanceBuffEffect = GetBlueprint<BlueprintBuff>("5d4e71c6f33b4f7fa41f7db0af3ac320");

            //移除1/2力代敏
            HeavyArmorAvoidanceBuffEffect.RemoveComponents<ReplaceStatBaseAttribute>();

            //重甲耐受
            //var HeavyMediumArmorEndurance = GetBlueprint<BlueprintFeature>("b28a0e962e3c40ee84411cccef31d4d0");
            var HeavyMediumArmorEnduranceBuff = GetBlueprint<BlueprintBuff>("af7a83e16d1442cb87e84f879bf2141b");
            var HeavyMediumArmorEnduranceBuffEffect = GetBlueprint<BlueprintBuff>("6610a6ded3fa4cc8bd7fc86ccbfa722f");

            LocalizedString repair = new() { m_Key = "b2c8842f-5ba6-42db-a348-cd0451038577" };
            HeavyMediumArmorEnduranceBuff.m_DisplayName = repair;
            HeavyMediumArmorEnduranceBuffEffect.m_DisplayName = repair;

            //不受移速惩罚 限制DR 上限1.5神话阶级
            HeavyMediumArmorEnduranceBuffEffect.AddComponent<ArmorSpeedPenaltyRemovalRe>();
            HeavyMediumArmorEnduranceBuffEffect.RemoveComponents<ContextRankConfig>();
            HeavyMediumArmorEnduranceBuffEffect.AddContextRankConfig(config =>
            {
                config.m_BaseValueType = ContextRankBaseValueType.CustomProperty;
                config.m_CustomProperty = bonusLevel["HalfMore"];
            });

            //提高AC 上限1+1/2神话阶级
            HeavyMediumArmorEnduranceBuffEffect.AddComponent<AcAddAcBuff>(ar => {
                ar.name = "HeavyArmorAcAddAcBuff";
                ar.m_BonusMod = BonusMod.Custom;
                ar.m_CustomProperty = bonusLevel["OneStep2"];
            });

            return HeavyMediumArmorEnduranceBuffEffect;
        }
    }
}