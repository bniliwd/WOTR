using BetterGameplay.NewContent;
using HarmonyLib;
using Kingmaker;

namespace BetterGameplay.Patch
{
    [HarmonyPatch(typeof(Game))]
    public static class Patch_LoadGame
    {
        [HarmonyPatch(nameof(Game.LoadGame))]
        [HarmonyPostfix]
        public static void LoadGame()
        {
            SentientFinnean.checkActived = true;
        }
    }
}