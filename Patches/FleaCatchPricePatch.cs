using System.Reflection;
using EFT.Communications;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace FastSoldInFlea.Patches
{
    internal class CatchAddOfferPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass3468), "method_6", null, null);
        }

        [PatchPrefix]
        static bool Prefix()
        {
	        if (FastSoldInFleaPlugin.IsKeyPressed)
	        {
                if (FastSoldInFleaPlugin.LastCachedItem == null || FastSoldInFleaPlugin.LastCatchedItemID == null)
                {
	                NotificationManagerClass.DisplayWarningNotification($"Maybe not have price", ENotificationDurationType.Long);
                    return true;
                }
                FastSoldInFleaPlugin.TryAddOfferToFlea(FastSoldInFleaPlugin.LastCachedItem, FastSoldInFleaPlugin.LastCatchedPrice);
                return false;
            }
	        else
	        {
				return true;
	        }
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
			RagFairClass ragFair = FastSoldInFleaPlugin.Session.RagFair;
			if (ragFair != null && ragFair.Available)
			{
				int myOffersCount = ragFair.MyOffersCount;
				int maxOffersCount = ragFair.MaxOffersCount;
				string text = string.Format("AddOfferButton{0}/{1}".Localized(null), myOffersCount, maxOffersCount);
				FastSoldInFleaPlugin.CachedOriginalText = text;
				if (caption == text)
				{
					FastSoldInFleaPlugin.CachedTextButton = ____text; 
					FastSoldInFleaPlugin.logSource.LogWarning("Update button text");
					FastSoldInFleaPlugin.TryGetPrice(FastSoldInFleaPlugin.LastCachedItem, price =>
					{
						if (FastSoldInFleaPlugin.IsKeyPressed)
						{
							FastSoldInFleaPlugin.CachedTextButton.text = $"Sold for {price}";
						}
						else
						{
							FastSoldInFleaPlugin.CachedTextButton.text = text;
						}
					});
				}
			}
		}
	}
    
	public class ContextMenuClosePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.Method(typeof(SimpleContextMenu), "Close", null, null);
		}
		
		[PatchPostfix]
		public static void Postfix()
		{
			FastSoldInFleaPlugin.CachedTextButton = null;
			FastSoldInFleaPlugin.CachedOriginalText = "";
			FastSoldInFleaPlugin.logSource.LogWarning("Closed menu");
		}
	}
}