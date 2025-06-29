using System.Reflection;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;

namespace FastSellInFlea.Patches
{
    /// <summary>
    /// Patch to get item data when clicking on an item
    /// </summary>
    internal class CatchIDItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GridItemView).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix(GridItemView __instance)
        {
            if (__instance.Item == null)
            {
                return true;
            }

            FastSellInFleaPlugin.LastCacheItem = __instance.Item;
            return true;
        }
    }
}