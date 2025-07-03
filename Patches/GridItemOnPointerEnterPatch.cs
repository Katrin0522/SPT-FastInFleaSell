using System.Reflection;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;

namespace FastSellInFlea.Patches
{
    internal class GridItemOnPointerEnterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GridItemView).GetMethod("OnPointerEnter", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static void Prefix(GridItemView __instance)
        {
            if (__instance.Item != null)
            {
                FastSellInFleaPlugin.LastCacheItem = __instance.Item;
                FastSellInFleaPlugin.IsStashItemHovered = true;
            }
        }
    }
    
    internal class GridItemOnPointerExitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GridItemView).GetMethod("OnPointerExit", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static void Prefix(GridItemView __instance)
        {
            if (FastSellInFleaPlugin.MainContextMenu == null)
            {
                FastSellInFleaPlugin.IsStashItemHovered = false;
                FastSellInFleaPlugin.LastCacheItem = null;
            }
        }
    }
}