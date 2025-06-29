using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Communications;
using EFT.InventoryLogic;
using FastSoldInFlea.Patches;
using SPT.Reflection.Utils;
using TMPro;
using UnityEngine;
using FleaRequirement = GClass2102;

namespace FastSoldInFlea
{
    [BepInPlugin("katrin0522.FastSoldInFlea", "Kat.FastSoldInFlea", "1.0.0")]
    [BepInDependency("com.kmyuhkyuk.KmyTarkovApi", "1.4.0")]
    public class FastSoldInFleaPlugin : BaseUnityPlugin
    {
        public static ManualLogSource logSource;

        private ConfigEntry<KeyboardShortcut> keyBind;
        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        public static bool IsKeyPressed;
        public static string LastCacheItemID;
        public static double LastCachePrice;
        public static Item LastCacheItem;
        public static TextMeshProUGUI CachedTextButton = null;
        public static string CachedOriginalText = "";
        public static string CachedNewText = "";

        private void Update()
        {
            IsKeyPressed = keyBind.Value.IsPressed();

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
        
        private void Awake()
        {
            keyBind = Config.Bind("1. Settings", "KeyBindSell", new KeyboardShortcut(KeyCode.LeftShift)); 
            
            new CatchAddOfferPatch().Enable();
            new FleaCatchItemPatch().Enable();
            new ContextMenuAddOfferPatch().Enable();
            new ContextMenuClosePatch().Enable();
            
            logSource = Logger;
            logSource.LogInfo("FastSoldInFlea successful loaded!");
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
                    price = result.Value.avg - 1;
                }

                // NotificationManagerClass.DisplayMessageNotification(
                //     $"[TryGetPrice] Get price {price}", 
                //     ENotificationDurationType.Default, 
                //     ENotificationIconType.EntryPoint);

                LastCachePrice = price;
                callback(price);
            });
        }

    }
}
