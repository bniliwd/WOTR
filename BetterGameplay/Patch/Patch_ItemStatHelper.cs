using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Craft;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using System;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(ItemStatHelper))]
    public static class Patch_ItemStatHelper
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
}