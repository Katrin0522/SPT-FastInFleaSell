using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using TMPro;

namespace FastSoldInFlea.Patches
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
            RagFairClass ragFair = FastSoldInFleaPlugin.Session.RagFair;
            if (ragFair != null && ragFair.Available)
            {
                int myOffersCount = ragFair.MyOffersCount;
                int maxOffersCount = ragFair.MaxOffersCount;
                string textWithCount =
                    string.Format("AddOfferButton{0}/{1}".Localized(), myOffersCount, maxOffersCount);

                string clearText = string.Format("ADDOFFER".Localized());
                FastSoldInFleaPlugin.CachedOriginalText = textWithCount;
                FastSoldInFleaPlugin.CachedNewText = clearText;
                if (caption.Contains(clearText))
                {
                    FastSoldInFleaPlugin.CachedTextButton = ____text;
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
}