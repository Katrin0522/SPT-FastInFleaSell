using System.Reflection;
using EFT.UI;
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
            return AccessTools.Method(typeof(ContextMenuButton), "Show");
        }

        [PatchPostfix]
        public static void Postfix(string caption, TextMeshProUGUI ____text)
        {
            RagFairClass ragFair = PlayerHelper.Session.RagFair;
            if (ragFair != null && ragFair.Available)
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
                    FastSellInFleaPlugin.CachedTextButton = ____text;
                    FleaUtils.TryGetPrice(FastSellInFleaPlugin.LastCacheItem, price =>
                    {
                        if (FastSellInFleaPlugin.IsKeySellModeHold)
                        {
                            FastSellInFleaPlugin.CachedTextButton.text = $"{clearText} {price}RUB".ToUpper();
                        }
                        else
                        {
                            FastSellInFleaPlugin.CachedTextButton.text = textWithCount.ToUpper();
                        }
                    });
                }
            }
        }
    }
}