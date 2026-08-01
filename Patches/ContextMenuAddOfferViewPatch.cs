using System.Reflection;
using EFT;
using EFT.UI;
using EFT.UI.Ragfair;
using FastSellInFlea.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace FastSellInFlea.Patches
{
    /// <summary>
    /// Patch to change view on the add offer button.
    /// </summary>
    public class ContextMenuAddOfferViewPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ContextMenuButton), nameof(ContextMenuButton.Show));
        }

        [PatchPostfix]
        public static void Postfix(string caption, TextMeshProUGUI ____text)
        {
            RagFair ragFair = PlayerHelper.Session?.RagFair;
            var item = FastSellInFleaPlugin.LastCacheItem;
            if (ragFair != null && ragFair.Available && PlayerHelper.CanQuickSell(item) && ____text != null)
            {
                int myOffersCount = ragFair.MyOffersCount;
                int maxOffersCount = ragFair.MaxOffersCount;
                string textWithCount =
                    string.Format("AddOfferButton{0}/{1}".Localized(), myOffersCount, maxOffersCount);

                string clearText = string.Format("ADDOFFER".Localized());
                FastSellInFleaPlugin.CachedOriginalText = textWithCount;
                FastSellInFleaPlugin.CachedNewText = clearText;
                if (caption.Contains(clearText))
                {
                    var button = ____text;
                    FastSellInFleaPlugin.CachedTextButton = button;
                    FastSellInFleaPlugin.LastCachePrice = 0;
                    FleaUtils.TryGetPrice(item, price =>
                    {
                        if (!button || FastSellInFleaPlugin.CachedTextButton != button ||
                            FastSellInFleaPlugin.LastCacheItem != item || !PlayerHelper.CanQuickSell(item))
                        {
                            return;
                        }

                        FastSellInFleaPlugin.LastCachePrice = price;
                        if (FastSellInFleaPlugin.IsKeySellModeHold)
                        {
                            button.text = $"{clearText} {price}RUB".ToUpper();
                        }
                        else
                        {
                            button.text = textWithCount.ToUpper();
                        }
                    });
                }
            }
        }
    }
}
