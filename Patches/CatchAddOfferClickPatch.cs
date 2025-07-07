using System.Reflection;
using EFT.Communications;
using FastSellInFlea.Models;
using HarmonyLib;
using SPT.Reflection.Patching;

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
            if (FastSellInFleaPlugin.IsKeyHold)
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
                
                FastSellInFleaPlugin.TryAddOfferToFlea(FastSellInFleaPlugin.LastCacheItem,
                    FastSellInFleaPlugin.LastCachePrice);
                return false;
            }

            return true;
        }
    }
}