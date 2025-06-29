using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFT.UI.Ragfair;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

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
                FastSoldInFleaPlugin.logSource.LogWarning("DEBUG avg catch -> " + result.avg);
                FastSoldInFleaPlugin.LastCatchedPrice = result.avg - 1;
                FastSoldInFleaPlugin.logSource.LogWarning("DEBUG catch id -> " + FastSoldInFleaPlugin.LastCatchedItemID);

                if (FastSoldInFleaPlugin.LastCachedItem == null || FastSoldInFleaPlugin.LastCatchedItemID == null)
                {
                    FastSoldInFleaPlugin.logSource.LogError("Data is null. Not create offer");
                    return true;
                }
                FastSoldInFleaPlugin.TryAddOfferToFlea(FastSoldInFleaPlugin.LastCachedItem, FastSoldInFleaPlugin.LastCatchedPrice);
                return false;
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
            FastSoldInFleaPlugin.TryGetPrice(FastSoldInFleaPlugin.LastCachedItem, price =>
            {
	            FastSoldInFleaPlugin.logSource.LogWarning($"CATCH ITEM ID {__instance.Item.Id}");
	            FastSoldInFleaPlugin.logSource.LogWarning($"CATCH ITEM Price {price}");
	            FastSoldInFleaPlugin.logSource.LogWarning($"CATCH ITEM TemplateId {__instance.Item.TemplateId}");
            });
            
            return true;
        }
    }
    
    public class ContextMenuAddOfferPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(ContextMenuButton), "Show", null, null);
		}
		
		[PatchPostfix]
		public static void Postfix(string caption, TextMeshProUGUI ____text)
		{
			if (!FastSoldInFleaPlugin.IsKeyPressed)
			{
				return;
			}
			
			
			RagFairClass ragFair = FastSoldInFleaPlugin.Session.RagFair;
			if (ragFair != null && ragFair.Available)
			{
				int myOffersCount = ragFair.MyOffersCount;
				int maxOffersCount = ragFair.MaxOffersCount;
				string text = string.Format("AddOfferButton{0}/{1}".Localized(null), myOffersCount, maxOffersCount);
				if (caption == text)
				{
					FastSoldInFleaPlugin.logSource.LogWarning("Update button text");
					double cachePrice = 0.0;
					FastSoldInFleaPlugin.TryGetPrice(FastSoldInFleaPlugin.LastCachedItem, price =>
					{
						cachePrice = price;
					});
					____text.text = $"Sold for {cachePrice}";
				}
			}
		}
	}
}