using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FastSoldInFlea.Patches
{
    /// <summary>
    /// Patches to track context menu closures.
    /// If the main menu is closed - clear temporary data.
    /// </summary>
    public class ContextMenuClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SimpleContextMenu), "Close");
        }

        [PatchPostfix]
        public static void Postfix(SimpleContextMenu __instance)
        {
            if (__instance == FastSoldInFleaPlugin.MainContextMenu)
            {
                FastSoldInFleaPlugin.CachedTextButton = null;
                FastSoldInFleaPlugin.CachedOriginalText = "";
                FastSoldInFleaPlugin.CachedNewText = "";
                FastSoldInFleaPlugin.LastCacheItem = null;
                FastSoldInFleaPlugin.LastCachePrice = 0.0;
            }
        }
    }
}