using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FastSellInFlea.Patches
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
            if (__instance == FastSellInFleaPlugin.MainContextMenu)
            {
                FastSellInFleaPlugin.CachedTextButton = null;
                FastSellInFleaPlugin.CachedOriginalText = "";
                FastSellInFleaPlugin.CachedNewText = "";
                FastSellInFleaPlugin.LastCacheItem = null;
                FastSellInFleaPlugin.LastCachePrice = 0.0;
            }
        }
    }
}