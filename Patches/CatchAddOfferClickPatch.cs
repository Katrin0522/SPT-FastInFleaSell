using System.Reflection;
using EFT.Communications;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FastSoldInFlea.Patches
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
            if (FastSoldInFleaPlugin.IsKeyPressed)
            {
                if (FastSoldInFleaPlugin.LastCacheItem == null)
                {
                    NotificationManagerClass.DisplayWarningNotification("Maybe not have price",
                        ENotificationDurationType.Long);
                    return true;
                }

                FastSoldInFleaPlugin.TryAddOfferToFlea(FastSoldInFleaPlugin.LastCacheItem,
                    FastSoldInFleaPlugin.LastCachePrice);
                return false;
            }

            return true;
        }
    }
}