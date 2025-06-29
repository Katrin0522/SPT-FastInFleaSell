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
                if (FastSoldInFleaPlugin.LastCacheItem == null || FastSoldInFleaPlugin.LastCacheItemID == null)
                {
	                NotificationManagerClass.DisplayWarningNotification($"Maybe not have price", ENotificationDurationType.Long);
                    return true;
                }
                FastSoldInFleaPlugin.TryAddOfferToFlea(FastSoldInFleaPlugin.LastCacheItem, FastSoldInFleaPlugin.LastCachePrice);
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
            FastSoldInFleaPlugin.LastCacheItem = __instance.Item;
            FastSoldInFleaPlugin.LastCacheItemID = __instance.Item.TemplateId;
            
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
				string textWithCount = string.Format("AddOfferButton{0}/{1}".Localized(), myOffersCount, maxOffersCount);
				
				string clearText = string.Format("ADDOFFER".Localized());
				FastSoldInFleaPlugin.CachedOriginalText = textWithCount;
				FastSoldInFleaPlugin.CachedNewText = clearText;
				if (caption.Contains(clearText))
				{
					FastSoldInFleaPlugin.CachedTextButton = ____text; 
					FastSoldInFleaPlugin.logSource.LogWarning("Update button text");
					FastSoldInFleaPlugin.TryGetPrice(FastSoldInFleaPlugin.LastCacheItem, price =>
					{
						if (FastSoldInFleaPlugin.IsKeyPressed)
						{
							FastSoldInFleaPlugin.CachedTextButton.text = $"{clearText} {price}RUB";
						}
						else
						{
							FastSoldInFleaPlugin.CachedTextButton.text = textWithCount;
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