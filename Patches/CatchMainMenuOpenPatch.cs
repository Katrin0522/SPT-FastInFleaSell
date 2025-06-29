using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FastSellInFlea.Patches
{
    /// <summary>
    /// Patch for catching the main context menu object
    /// </summary>
    public class CatchMainMenuOpenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemUiContext), "ShowContextMenu");
        }

        [PatchPostfix]
        public static void Postfix(ItemUiContext __instance)
        {
            var context = __instance.ContextMenu;
            if (context != null)
            {
                FastSellInFleaPlugin.MainContextMenu = context;
            }
        }
    }
}