using BetterGameplay.Logic;
using BetterGameplay.Util;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using SpellTurningRedone.Logic;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Modify
{
    public static class ModifyFeature
    {
        public static void MultiModify()
        {
            ModifyFavoriteEnemy();
            ModifyDisableElectricityImmunity();
            ModifySpellTurning();
            ModifyFreedomOfMovement();
            ModifyTwoHandFighter();
            ModifyRage();
        }

        public static void ModifyFavoriteEnemy()
        {
            var FavoriteEnemyOutsider = GetBlueprint<BlueprintFeature>("f643b38acc23e8e42a3ed577daeb6949");
            var FavoriteEnemyDemonOfMagic = GetBlueprint<BlueprintFeature>("21328361091fd2c44a3909fcae0dd598");
            var FavoriteEnemyDemonOfSlaughter = GetBlueprint<BlueprintFeature>("6c450765555b1554294b5556f50d304e");
            var FavoriteEnemyDemonOfStrength = GetBlueprint<BlueprintFeature>("48e9e7ecca39c4a438d9262a20ab5066");

            var OutsiderType = GetBlueprintReference<BlueprintUnitFactReference>("9054d3988d491d944ac144e27b6bc318");
            var AasimarRace = GetBlueprintReference<BlueprintUnitFactReference>("b7f02ba92b363064fb873963bec275ee");
            var InstantEnemyBuff = GetBlueprintReference<BlueprintUnitFactReference>("82574f7d14a28e64fab8867fbaa17715");

            PatchFavoriteEnemy(FavoriteEnemyOutsider);
            PatchFavoriteEnemy(FavoriteEnemyDemonOfMagic);
            PatchFavoriteEnemy(FavoriteEnemyDemonOfSlaughter);
            PatchFavoriteEnemy(FavoriteEnemyDemonOfStrength);

            void PatchFavoriteEnemy(BlueprintFeature FavoriteEnemy)
            {
                var favoredEnemy = FavoriteEnemy.GetComponent<FavoredEnemy>();
                favoredEnemy.m_CheckedFacts = [OutsiderType, AasimarRace, InstantEnemyBuff];
                FavoriteEnemy.m_Icon = FavoriteEnemyDemonOfStrength.Icon;
            }
        }

        public static void ModifyDisableElectricityImmunity()
        {
            var DisableElectricityImmunity = GetBlueprint<BlueprintFeature>("02808184b77c4ba9b1cdac80a666b5e6");
            DisableElectricityImmunity.SetComponents(
                new CombatStateTrigger()
                {
                    CombatStartActions = new ActionList()
                    {
                        Actions = [
                            new ContextActionRemoveFact()
                            { m_Fact = GetBlueprintReference<BlueprintUnitFactReference>("cd1e5ab641a833c49994aff99db98952") }
                        ]
                    },
                    CombatEndActions = new ActionList()
                }
            );
        }

        public static void ModifySpellTurning()
        {
            var ShieldArmorReflect = GetBlueprint<BlueprintBuff>("94a13a46d2e744b6bccf85f443557643");
            var RazmirMaskFeature = GetBlueprint<BlueprintFeature>("13d5818737694021b001641437a4ba29");
            var BeltOfArodenFeature = GetBlueprint<BlueprintFeature>("255c71644a79475b950a1270a882ff65");
            var SliverDragonFeature = GetBlueprint<BlueprintFeature>("4d67d955ede14c9b824bed14af7d8b18");
            var ShadowBalorFeature = GetBlueprint<BlueprintFeature>("32bf05e17a1c422b8aeed93055275a26");
            var ValmallosFeature = GetBlueprint<BlueprintFeature>("2021295715794845806aeec7ae26641a");

            //RazmirMaskFeature.m_Description = "RazmirMaskFeatureDesc".CreateString();
            //RazmirMaskFeature.m_Icon = GetBlueprint<BlueprintItemEquipmentGlasses>("9338510a8d5c4b43b1284470b706128d")?.m_Icon;
            //RazmirMaskFeature.HideInUI = false;
            //RazmirMaskFeature.HideInCharacterSheetAndLevelUp = false;

            var RazmirMaskBuff = GetBlueprint<BlueprintBuff>("40d4548a8b1f42ba92f70fcc1c50b491");
            AddAbilityUseTrigger trigger = RazmirMaskBuff.GetComponent<AddAbilityUseTrigger>();
            if (trigger != null)
            {
                trigger.CheckAbilityType = true;
            }

            PatchSpellTurning(RazmirMaskFeature);
            PatchSpellTurning(ShieldArmorReflect);
            PatchSpellTurning(BeltOfArodenFeature);
            PatchSpellTurning(SliverDragonFeature);
            PatchSpellTurning(ShadowBalorFeature);
            PatchSpellTurning(ValmallosFeature);

            static void PatchSpellTurning(BlueprintFact fact)
            {
                fact.RemoveComponents<SpellTurning>();
                fact.AddComponent<SpellTurningRe>(
                    st => {
                        st.m_SpellResistanceOnly = false;
                        st.m_SpellDescriptorOnly = SpellDescriptor.None;
                        st.m_SpecificSpellsOnly = [];
                    });
            }
        }

        public static void ModifyFreedomOfMovement()
        {
            var Staggered = GetBlueprint<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            Staggered.GetComponent<SpellDescriptorComponent>().Descriptor = SpellDescriptor.Staggered;

            var Nauseated = GetBlueprint<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            Nauseated.GetComponent<SpellDescriptorComponent>().Descriptor = SpellDescriptor.Nauseated;

            var SeamantleBuff = GetBlueprint<BlueprintBuff>("1c05dd3a1c78b0e4e9f7438a43e7a9fd");
            var FreedomOfMovementBuff = GetBlueprint<BlueprintBuff>("1533e782fca42b84ea370fc1dcbf4fc1");
            var FreedomOfMovementBuffPermanent = GetBlueprint<BlueprintBuff>("235533b62159790499ced35860636bb2");
            var FreedomOfMovementBuff_FD = GetBlueprint<BlueprintBuff>("60906dd9e4ddec14c8ac9a0f4e47f54c");
            var DLC3_FreedomOfMovementBuff = GetBlueprint<BlueprintBuff>("d6fb42ec153f4d699e57891522d7f4c9");
            var FreedomOfMovementLinnorm = GetBlueprint<BlueprintBuff>("67519ff6ba615c045afca2347608bfe3");
            var BootsOfFreeReinBuff = GetBlueprint<BlueprintBuff>("7ac8effd6341443d98da735b965b0176");
            var BootsOfFreeReinBuff2 = GetBlueprint<BlueprintBuff>("e24e9c0d77144663815c69e969ac4fdb");

            RemoveStaggerImmunity(FreedomOfMovementBuff);
            RemoveStaggerImmunity(FreedomOfMovementBuffPermanent);
            RemoveStaggerImmunity(FreedomOfMovementBuff_FD);
            RemoveStaggerImmunity(DLC3_FreedomOfMovementBuff);
            RemoveStaggerImmunity(FreedomOfMovementLinnorm);
            RemoveStaggerImmunity(BootsOfFreeReinBuff);
            RemoveStaggerImmunity(BootsOfFreeReinBuff2);

            SeamantleBuff.TemporaryContext(bp => {
                bp.GetComponent<ACBonusUnlessFactMultiple>()?.TemporaryContext(c => {
                    c.m_Facts = [
                            FreedomOfMovementBuff.ToReference<BlueprintUnitFactReference>(),
                            FreedomOfMovementBuffPermanent.ToReference<BlueprintUnitFactReference>(),
                            FreedomOfMovementBuff_FD.ToReference<BlueprintUnitFactReference>(),
                            DLC3_FreedomOfMovementBuff.ToReference<BlueprintUnitFactReference>(),
                            FreedomOfMovementLinnorm.ToReference<BlueprintUnitFactReference>(),
                            BootsOfFreeReinBuff.ToReference<BlueprintUnitFactReference>(),
                            BootsOfFreeReinBuff2.ToReference<BlueprintUnitFactReference>(),
                        ];
                });
            });

            static void RemoveStaggerImmunity(BlueprintBuff buff)
            {
                buff.RemoveComponents<AddConditionImmunity>(p => p.Condition == UnitCondition.Staggered);
                buff.GetComponents<BuffDescriptorImmunity>().ForEach(c => {
                    c.Descriptor &= ~SpellDescriptor.Staggered;
                });
            }
        }

        public static void ModifyTwoHandFighter()
        {
            var TwoHandedFighterWeaponTraining = GetBlueprint<BlueprintFeature>("88da2a5dfc505054f933bb81014e864f");
            var WeaponTrainingSelection = GetBlueprint<BlueprintFeature>("b8cecf4e5e464ad41b79d5b42b76b399");

            var AdvancedWeaponTraining1 = GetBlueprint<BlueprintFeature>("3aa4cbdd4af5ba54888b0dc7f07f80c4");
            PrerequisiteFeature feature = AdvancedWeaponTraining1.GetComponent<PrerequisiteFeature>();
            if (feature != null)
            {
                feature.Group = Prerequisite.GroupType.Any;
            }
            AdvancedWeaponTraining1.AddComponent(
                Create<PrerequisiteFeature>(c => {
                    c.m_Feature = TwoHandedFighterWeaponTraining.ToReference<BlueprintFeatureReference>();
                    c.Group = Prerequisite.GroupType.Any;
            }));
        }

        public static void ModifyRage()
        {
            var rage = GetBlueprint<BlueprintAbility>("97b991256e43bb140b263c326f690ce2");

            AbilityTargetsAround around = rage.GetComponent<AbilityTargetsAround>();
            if (around != null)
            {
                around.m_Radius = 5.Feet();
            }
        }
    }
}