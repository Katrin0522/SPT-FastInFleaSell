using System;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using FastSellInFlea.Models;
using FastSellInFlea.Patches;
using SPT.Reflection.Utils;
using TMPro;
using FleaRequirement = GClass2102;

namespace FastSellInFlea
{
    [BepInPlugin("katrin0522.FastSellInFlea", "Kat.FastSellInFlea", "1.0.3")]
    public class FastSellInFleaPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        private LocalizationModel _localization;
        
        public static ManualLogSource logSource;
        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        
        public static bool IsKeyHold;
        public static bool IsKeyPress;
        
        public static double LastCachePrice;
        public static Item LastCacheItem;
        public static string CachedOriginalText = "";
        public static string CachedNewText = "";
        
        public static TextMeshProUGUI CachedTextButton = null;
        public static SimpleContextMenu MainContextMenu;
        public static bool IsStashItemHovered;
        
        public static bool HasRaidStarted()
        {			
            bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
            return inRaid.HasValue && inRaid.Value;
        }
        
        public bool FleaIsAvailable()
        {
            if (Session != null)
            {
                RagFairClass rag = Session.RagFair;
                if (rag != null && rag.Available)
                {
                    return true;
                }

                return false;
            }
            return false;
        }

        public static bool CanBeSelectedAtRagfair(Item item)
        {
            if (item.Owner.OwnerType != EOwnerType.Profile &&
                item.Owner.GetType() == typeof(TraderControllerClass))
                return false;
            if (!item.CanSellOnRagfair)
                return false;
            if (Session.RagFair.MyOffersCount >= Session.RagFair.MaxOffersCount)
                return false;
            
            return true;
        }

        private void Awake()
        {
            _settings = SettingsModel.Create(Config);
            _localization = LocalizationModel.Create();
            
            new CatchAddOfferClickPatch().Enable();
            new CatchIDItemPatch().Enable();
            new ContextMenuAddOfferViewPatch().Enable();
            new ContextMenuClosePatch().Enable();
            new CatchMainMenuOpenPatch().Enable();
            new GridItemOnPointerEnterPatch().Enable();
            new GridItemOnPointerExitPatch().Enable();
            
            logSource = Logger;
            logSource.LogInfo("FastSellInFlea successful loaded!");
        }
        
        private void Update()
        {
            IsKeyHold = SettingsModel.Instance.KeyBind.Value.IsPressed();
            IsKeyPress = SettingsModel.Instance.KeyBindHover.Value.IsDown();
            
            //Updating view for button if it !null
            if (CachedTextButton)
            {
                if (FleaIsAvailable())
                {
                    if (IsKeyHold && !string.IsNullOrEmpty(CachedNewText))
                    {
                        CachedTextButton.text = $"{CachedNewText} {LastCachePrice}RUB".ToUpper();
                    }
                    else
                    {
                        CachedTextButton.text = CachedOriginalText.ToUpper();
                    }
                }
            }

            //Add offer when hover on item
            if (IsStashItemHovered)
            {
                if (FleaIsAvailable())
                {
                    if (IsKeyPress)
                    {
                        if (LastCacheItem != null && Session.Profile.Examined(LastCacheItem) && !HasRaidStarted())
                        {
                            if(!CanBeSelectedAtRagfair(LastCacheItem))
                                return;

                            TryGetPrice(LastCacheItem, price =>
                            {
                                LastCachePrice = price;
                                if(price <= 0)
                                    return;
                                
                                TryAddOfferToFlea(LastCacheItem, LastCachePrice, result =>
                                {
                                    CachedTextButton = null;
                                    CachedOriginalText = "";
                                    CachedNewText = "";
                                    LastCacheItem = null;
                                    LastCachePrice = 0.0;
                                });
                            });
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Trying to add offer to flea with Item and Adjusted price
        /// </summary>
        /// <param name="item"></param>
        /// <param name="adjustedPrice"></param>
        public static void TryAddOfferToFlea(Item item, double adjustedPrice)
        {
            //Used some code from LootValue repository
            var dataOffer = new FleaRequirement()
            {
                count = adjustedPrice,
                _tpl = "5449016a4bdc2d6f028b456f"
            };
            
            FleaRequirement[] gs = new FleaRequirement[1] { dataOffer };
            Session.RagFair.AddOffer(false, new string[1] { item.Id }, gs, null);
            NotificationManagerClass.DisplayMessageNotification(LocalizationModel.Instance.GetLocaleText(TypeText.AddOffer, dataOffer.count), ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
        }
        
        /// <summary>
        /// Trying to add offer to flea with Item and Adjusted price. With callback
        /// </summary>
        /// <param name="item"></param>
        /// <param name="adjustedPrice"></param>
        /// <param name="callback"></param>
        public static void TryAddOfferToFlea(Item item, double adjustedPrice, Action<bool> callback)
        {
            //Used some code from LootValue repository
            var dataOffer = new FleaRequirement()
            {
                count = adjustedPrice,
                _tpl = "5449016a4bdc2d6f028b456f"
            };
            
            FleaRequirement[] gs = new FleaRequirement[1] { dataOffer };
            Session.RagFair.AddOffer(false, new string[1] { item.Id }, gs, null);
            NotificationManagerClass.DisplayMessageNotification(LocalizationModel.Instance.GetLocaleText(TypeText.AddOffer, dataOffer.count), ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
            callback(true);
        }
        
        /// <summary>
        /// Get price with preset from config model 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="callback"></param>
        public static void TryGetPrice(Item item, Action<double> callback)
        {
            Session.GetMarketPrices(item.TemplateId, result =>
            {
                double price = 0;
                if (result.Value != null)
                {
                    if (SettingsModel.Instance.TypeAdjustPrice.Value == TypeMathPrice.Value)
                    {
                        price = SettingsModel.Instance.OfferPresetFlea.Value switch
                        {
                            AutoFleaPrice.Minimum => result.Value.min -
                                                     SettingsModel.Instance.SubstractPriceValue.Value,
                            AutoFleaPrice.Average => result.Value.avg -
                                                     SettingsModel.Instance.SubstractPriceValue.Value,
                            AutoFleaPrice.Maximum => result.Value.max -
                                                     SettingsModel.Instance.SubstractPriceValue.Value,
                            _ => result.Value.avg - 1
                        };

                        if (result.Value.min <= 0.0f && result.Value.avg <= 0.0f && result.Value.max <= 0.0f)
                        {
                            LastCachePrice = 0.0;
                            callback(0.0f);
                        }
                    }
                    else if (SettingsModel.Instance.TypeAdjustPrice.Value == TypeMathPrice.Percent)
                    {
                        price = SettingsModel.Instance.OfferPresetFlea.Value switch
                        {
                            AutoFleaPrice.Minimum => result.Value.min *
                                                     (1 - (SettingsModel.Instance.SubstractPricePercent.Value / 100.0)),
                            AutoFleaPrice.Average => result.Value.avg *
                                                     (1 - (SettingsModel.Instance.SubstractPricePercent.Value / 100.0)),
                            AutoFleaPrice.Maximum => result.Value.max *
                                                     (1 - (SettingsModel.Instance.SubstractPricePercent.Value / 100.0)),
                            _ => result.Value.avg * (1 - (99 / 100.0)),
                        };
                        
                        if (result.Value.min <= 0.0f && result.Value.avg <= 0.0f && result.Value.max <= 0.0f)
                        {
                            LastCachePrice = 0.0;
                            callback(0.0f);
                        }
                    }
                }
                price = Math.Max(0, (int)Math.Round(price));
                LastCachePrice = price;
                callback(price);
            });
        }

    }

    /// <summary>
    /// Presets for get and adjust price
    /// </summary>
    public enum AutoFleaPrice
    {
        Minimum,
        Average,
        Maximum
    }
    
    /// <summary>
    /// Types math for adjust price
    /// </summary>
    public enum TypeMathPrice
    {
        Value,
        Percent
    }
}
