using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;

namespace BetterGameplay
{
    public class Main
    {
        public static bool Enabled;

        public static string ModPath;

        //public static ModLogger logger;

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
            //logger = modEntry.Logger;
            ModPath = modEntry.Path;
            modEntry.OnToggle = OnToggle;
            new Harmony(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

    }
}