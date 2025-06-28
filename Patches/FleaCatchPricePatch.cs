using System.Reflection;
using EFT.UI.DragAndDrop;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FastSoldInFlea.Patches
{
    internal class FleaCatchPricePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemMarketPricesPanel), "method_1", null, null);
        }

        [PatchPrefix]
        static bool Prefix(ItemMarketPrices result)
        {
            if (FastSoldInFleaPlugin.IsKeyPressed)
            {
                FastSoldInFleaPlugin.logSource.LogWarning("DEBUG avg перехват -> " + result.avg);
                FastSoldInFleaPlugin.LastCatchedPrice = result.avg - 1;
                FastSoldInFleaPlugin.logSource.LogWarning("DEBUG перехваченный id -> " + FastSoldInFleaPlugin.LastCatchedItemID);

                if (FastSoldInFleaPlugin.LastCachedItem == null || FastSoldInFleaPlugin.LastCatchedItemID == null)
                {
                    FastSoldInFleaPlugin.logSource.LogError("DEBUG Что-то пустенькое. Не создаём оффер");
                    return true;
                }
                FastSoldInFleaPlugin.TryAddOfferToFlea(FastSoldInFleaPlugin.LastCachedItem, FastSoldInFleaPlugin.LastCatchedPrice);
            }
            return true;
        }
    }
    
    internal class FleaCatchItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GridItemView).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix(GridItemView __instance)
        {
            if (__instance.Item == null)
            {
                return true;
            }
            FastSoldInFleaPlugin.LastCachedItem = __instance.Item;
            FastSoldInFleaPlugin.LastCatchedItemID = __instance.Item.TemplateId;
            FastSoldInFleaPlugin.logSource.LogWarning($"Перехват ITEM ID {__instance.Item.Id}");
            FastSoldInFleaPlugin.logSource.LogWarning($"Перехват ITEM TemplateId {__instance.Item.TemplateId}");
            return true;
        }
    }
}