using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using OnePearl.Components;
using OnePearl.Utils;
using System.Reflection;
using UnityModManagerNet;

namespace OnePearl;

static class Main
{
    internal static Harmony HarmonyInstance;
    //internal static UnityModManager.ModEntry.ModLogger log;
    internal static Guid ModNamespaceGuid;
    internal static string ModPath;

    static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModPath = modEntry.Path;
        //log = modEntry.Logger;
        ModNamespaceGuid = NamespaceGuidUtils.CreateV5Guid(NamespaceGuidUtils.UrlNamespace, modEntry.Info.Id);
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    [HarmonyPriority(800)]
    internal static class BlueprintInitPatch
    {
        private static bool loaded;

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (loaded) return;
            loaded = true;

            BlueprintCreator.CreateBlueprints();
        }
    }
}