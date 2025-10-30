using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Craft;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(CraftRoot))]
    public static class Patch_CraftRoot
    {
        [HarmonyPatch(nameof(CraftRoot.CheckCraftAvail))]
        [HarmonyPrefix]
        public static bool CheckCraftAvail(CraftRoot __instance, UnitEntityData crafter, CraftItemInfo craftInfo, bool isInProgress, ref CraftAvailInfo __result)
        {
            CraftAvailInfo craftAvailInfo = new();
            BlueprintAbility blueprintAbility = craftInfo.Item?.Ability;

            if (blueprintAbility != null && __instance.TryFindAbilityInSpellbooks(crafter, blueprintAbility, out var spellbook))
            {
                CraftRequirements[] array = ((craftInfo.Item.Type == UsableItemType.Scroll) ? __instance.m_ScrollsRequirements : __instance.m_PotionRequirements);
                int minSpellLevel = spellbook.GetMinSpellLevel(blueprintAbility);
                if (minSpellLevel < 0) minSpellLevel = spellbook.GetMinSpellLevel(blueprintAbility.Parent);
                CraftRequirements craftRequirements = ((minSpellLevel >= 0 && minSpellLevel < array.Length) ? array[minSpellLevel] : new CraftRequirements());
                ItemsCollection inventory = Game.Instance.Player.Inventory;
                craftAvailInfo.IsHaveFeature = craftRequirements.RequiredFeature == null || crafter.HasFact((BlueprintFeature)craftRequirements.RequiredFeature);
                craftAvailInfo.IsHaveItem = craftRequirements.RequiredItems.Count == 0 || inventory.ContainsAtLeastOneOf(craftRequirements.RequiredItems);
                craftAvailInfo.IsHaveResources = isInProgress || craftInfo.CraftCost.Count == 0 || __instance.CheckResources(craftInfo);
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
}