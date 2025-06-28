using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Communications;
using EFT.InventoryLogic;
using FastSoldInFlea.Patches;
using SPT.Reflection.Utils;
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
        public static string LastCatchedItemID;
        public static double LastCatchedPrice;
        public static Item LastCachedItem;

        private void Update()
        {
            IsKeyPressed = keyBind.Value.IsPressed();
        }
        
        private void Awake()
        {
            keyBind = Config.Bind("1. Settings", "KeyBindSell", new KeyboardShortcut(KeyCode.LeftShift)); 
            
            logSource = Logger;
            logSource.LogInfo("FastSoldInFlea successful loaded!");

            new FleaCatchPricePatch().Enable();
            new FleaCatchItemPatch().Enable();
        }
        
        public static void TryAddOfferToFlea(Item item, double unadjustedPrice)
        {
            var g = new FleaRequirement()
            {
                count = unadjustedPrice - 1,
                _tpl = "5449016a4bdc2d6f028b456f"
            };
            
            FleaRequirement[] gs = new FleaRequirement[1] { g };
            Session.RagFair.AddOffer(false, new string[1] { item.Id }, gs, null);
            NotificationManagerClass.DisplayMessageNotification($"Продаём оффер за {g.count}RUB", ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
        }
    }
}
