using BetterGameplay.Modify;
using BetterGameplay.NewContent;
using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(BlueprintsCache))]
    public static class Patch_BlueprintsCache
    {
        static bool loaded = false;

        [HarmonyPatch(nameof(BlueprintsCache.Init))]
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (loaded) return;
            loaded = true;

            ModifyItem.MultiModify();
            ModifyFeature.MultiModify();

            ModifyMythicAbility.MultiModify();
            ModifyMythicFeat.MultiModify();
            ModifyResource.MultiModify();

            ModifyCompanion.MultiModify();

            MythicOutflank.AddMythicOutflank();
            FreedomGift.AddFreedomGift();
            RichEnemy.ReplaceEnemyEquipments();
            SentientFinnean.ModifyFinneanEnchantment();
        }
    }
}