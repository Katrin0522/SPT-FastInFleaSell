using System.Reflection;
using EFT.Communications;
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
                    NotificationManagerClass.DisplayWarningNotification("Maybe not have price",
                        ENotificationDurationType.Long);
                    return true;
                }

                FastSellInFleaPlugin.TryAddOfferToFlea(FastSellInFleaPlugin.LastCacheItem,
                    FastSellInFleaPlugin.LastCachePrice);
                return false;
            }

            return true;
        }
    }
}