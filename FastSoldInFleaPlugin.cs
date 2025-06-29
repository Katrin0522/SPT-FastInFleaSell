using System;
using BepInEx;
using BepInEx.Logging;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using FastSoldInFlea.Models;
using FastSoldInFlea.Patches;
using SPT.Reflection.Utils;
using TMPro;
using FleaRequirement = GClass2102;

namespace FastSoldInFlea
{
    [BepInPlugin("katrin0522.FastSoldInFlea", "Kat.FastSoldInFlea", "1.0.0")]
    public class FastSoldInFleaPlugin : BaseUnityPlugin
    {
        public static ManualLogSource logSource;
        
        private SettingsModel _settings;
        
        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        public static bool IsKeyPressed;
        public static string LastCacheItemID;
        public static double LastCachePrice;
        public static Item LastCacheItem;
        public static TextMeshProUGUI CachedTextButton = null;
        public static string CachedOriginalText = "";
        public static string CachedNewText = "";
        public static SimpleContextMenu MainContextMenu;
        
        private void Awake()
        {
            _settings = SettingsModel.Create(Config);
            
            new CatchAddOfferClickPatch().Enable();
            new CatchIDItemPatch().Enable();
            new ContextMenuAddOfferViewPatch().Enable();
            new ContextMenuClosePatch().Enable();
            new CatchMainMenuOpenPatch().Enable();
            
            logSource = Logger;
            logSource.LogInfo("FastSoldInFlea successful loaded!");
        }
        
        private void Update()
        {
            IsKeyPressed = SettingsModel.Instance.KeyBind.Value.IsPressed();

            if (CachedTextButton)
            {
                if (IsKeyPressed && !string.IsNullOrEmpty(CachedNewText))
                {
                    CachedTextButton.text = $"{CachedNewText} {LastCachePrice}RUB";
                }
                else
                {
                    CachedTextButton.text = CachedOriginalText;
                }
            }
        }
        
        public static void TryAddOfferToFlea(Item item, double adjustedPrice)
        {
            var g = new FleaRequirement()
            {
                count = adjustedPrice,
                _tpl = "5449016a4bdc2d6f028b456f"
            };
            
            FleaRequirement[] gs = new FleaRequirement[1] { g };
            Session.RagFair.AddOffer(false, new string[1] { item.Id }, gs, null);
            NotificationManagerClass.DisplayMessageNotification($"Sell offer for {g.count}RUB", ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
        }
        
        public static void TryGetPrice(Item item, Action<double> callback)
        {
            Session.GetMarketPrices(item.TemplateId, result =>
            {
                double price = 0;
                if (result.Value != null)
                {
                    price = SettingsModel.Instance.OfferPresetFlea.Value switch
                    {
                        AutoFleaPrice.Minimum => result.Value.min - SettingsModel.Instance.AdjustPriceValue.Value,
                        AutoFleaPrice.Average => result.Value.avg - SettingsModel.Instance.AdjustPriceValue.Value,
                        AutoFleaPrice.Maximum => result.Value.max - SettingsModel.Instance.AdjustPriceValue.Value,
                        _ => result.Value.avg - 1
                    };
                }

                LastCachePrice = price;
                callback(price);
            });
        }

    }

    public enum AutoFleaPrice
    {
        Minimum,
        Average,
        Maximum
    }
}
