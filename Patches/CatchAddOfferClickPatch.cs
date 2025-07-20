using System.Reflection;
using EFT.Communications;
using FastSellInFlea.Models;
using FastSellInFlea.Utils;
using HarmonyLib;
using SPT.Reflection.Patching;
using UIFixesInterop;

namespace FastSellInFlea.Patches
{
    /// <summary>
    /// Patch for redirects while clicking on Add Offer.
    /// If the keybind from config is clicked - it will add quickly offer.
    /// If not, it will show the Add Offer screen.
    /// </summary>
    internal class CatchAddOfferClickPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass3468), "method_6");
        }

        [PatchPrefix]
        static bool Prefix()
        {
            if (FastSellInFleaPlugin.IsKeySellModeHold)
            {
                if (FastSellInFleaPlugin.LastCacheItem == null)
                {
                    FastSellInFleaPlugin.logSource.LogError($"LastCacheItem is NUll? -> {FastSellInFleaPlugin.LastCacheItem == null}\nLastCachePrice is NUll? -> {FastSellInFleaPlugin.LastCachePrice <= 0}\nMainContextMenu is NUll? -> {FastSellInFleaPlugin.MainContextMenu == null}");
                    NotificationManagerClass.DisplayWarningNotification(LocalizationModel.Instance.GetLocaleText(TypeText.ErrorAddOffer),
                        ENotificationDurationType.Long);
                    return true;
                }

                if (FastSellInFleaPlugin.LastCachePrice <= 0)
                {
                    return false;
                }

                if (MultiSelect.Count > 1)
                {
                    var myOffersCountNow = PlayerHelper.Session.RagFair.MyOffersCount;
                    var maxOffersCountNow = PlayerHelper.Session.RagFair.MaxOffersCount;
                    
                    int offersAdded = 0;
                    int offerLimit = maxOffersCountNow - myOffersCountNow;
                    
                    var itemsGrouped = FleaUtils.GroupSimilarItems(MultiSelect.Items);

                    foreach (var itemGrouped in itemsGrouped)
                    {
                        if (offersAdded >= offerLimit)
                            break;
                        
                        if (PlayerHelper.CanBeSelectedAtRagfair(itemGrouped[0]))
                        {
                            FleaUtils.TryGetPrice(itemGrouped[0], price =>
                            {
                                if (price <= 0 || offersAdded >= offerLimit)
                                    return;
                                
                                FastSellInFleaPlugin.LastCachePrice = price;
                                offersAdded++;
                                FleaUtils.TryAddOfferToFlea(itemGrouped, price);
                            });
                        }
                    }
                    
                    FastSellInFleaPlugin.CachedTextButton = null;
                    FastSellInFleaPlugin.CachedOriginalText = "";
                    FastSellInFleaPlugin.CachedNewText = "";
                    FastSellInFleaPlugin.LastCacheItem = null;
                    FastSellInFleaPlugin.LastCachePrice = 0.0;
                    return false;
                }
                
                
                FleaUtils.TryAddOfferToFlea(FastSellInFleaPlugin.LastCacheItem,
                    FastSellInFleaPlugin.LastCachePrice);
                return false;
            }

            return true;
        }
    }
}