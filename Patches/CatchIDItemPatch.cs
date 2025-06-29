using System.Reflection;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;

namespace FastSoldInFlea.Patches
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

            FastSoldInFleaPlugin.LastCacheItem = __instance.Item;
            FastSoldInFleaPlugin.LastCacheItemID = __instance.Item.TemplateId;

            return true;
        }
    }
}