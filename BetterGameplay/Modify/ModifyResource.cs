using BetterGameplay.Util;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using static BetterGameplay.Util.BlueprintUtils;

namespace BetterGameplay.Modify
{
    public static class ModifyResource
    {
        public static void MultiModify()
        {
            PatchDeadlyMagic();
            PatchEclipseChill();
            PatchDemonRage();
        }

        static void PatchDeadlyMagic()
        {
            var DeadlyMagicResource = GetBlueprint<BlueprintAbilityResource>("a3441d150c5fec54bbbc04efdefaf6aa");

            DeadlyMagicResource.TemporaryContext(bp => {
                bp.m_MaxAmount = new BlueprintAbilityResource.Amount()
                {
                    m_Class = [],
                    m_Archetypes = [],
                    m_ClassDiv = [GetBlueprintReference<BlueprintCharacterClassReference>("5d501618a28bdc24c80007a5c937dcb7")],
                    BaseValue = 3,
                    IncreasedByLevelStartPlusDivStep = true,
                    LevelStep = 2,
                    PerStepIncrease = 1
                };
            });
        }

        static void PatchEclipseChill()
        {
            var EclipseChillResource = GetBlueprint<BlueprintAbilityResource>("b134e2d400adc4a49bd217a7953d6d6a");

            EclipseChillResource.TemporaryContext(bp => {
                bp.m_MaxAmount = new BlueprintAbilityResource.Amount()
                {
                    m_Class = [],
                    m_Archetypes = [],
                    m_ClassDiv = [GetBlueprintReference<BlueprintCharacterClassReference>("5d501618a28bdc24c80007a5c937dcb7")],
                    BaseValue = 3,
                    IncreasedByLevelStartPlusDivStep = true,
                    LevelStep = 2,
                    PerStepIncrease = 1
                };
            });
        }

        static void PatchDemonRage()
        {
            var DemonRageResource = GetBlueprint<BlueprintAbilityResource>("f3bf174f0f86b4f45a823e9ed6ccc7a5");

            DemonRageResource.TemporaryContext(bp => {
                bp.m_MaxAmount = new BlueprintAbilityResource.Amount()
                {
                    m_Class = [],
                    m_Archetypes = [],
                    m_ClassDiv = [GetBlueprintReference<BlueprintCharacterClassReference>("8e19495ea576a8641964102d177e34b7")],
                    BaseValue = 3,
                    IncreasedByLevelStartPlusDivStep = true,
                    LevelStep = 2,
                    PerStepIncrease = 1,
                    IncreasedByStat = true,
                    ResourceBonusStat = StatType.Constitution
                };
            });
        }
    }
}