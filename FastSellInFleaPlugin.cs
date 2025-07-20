using BepInEx;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI;
using FastSellInFlea.Models;
using FastSellInFlea.Patches;
using FastSellInFlea.Utils;
using TMPro;


namespace FastSellInFlea
{
    [BepInPlugin("katrin0522.FastSellInFlea", "Kat.FastSellInFlea", "1.1.0")]
    public class FastSellInFleaPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        private LocalizationModel _localization;
        
        public static ManualLogSource logSource;
        
        public static bool IsKeySellModeHold;
        public static bool IsKeySellPress;
        
        public static double LastCachePrice;
        public static Item LastCacheItem;
        public static string CachedOriginalText = "";
        public static string CachedNewText = "";
        
        public static TextMeshProUGUI CachedTextButton = null;
        public static SimpleContextMenu MainContextMenu;
        public static bool IsStashItemHovered;
        
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
            IsKeySellModeHold = SettingsModel.Instance.KeyBind.Value.IsPressed();
            IsKeySellPress = SettingsModel.Instance.KeyBindHover.Value.IsDown();
            
            //Updating view for button if it != null
            if (CachedTextButton)
            {
                if (PlayerHelper.FleaIsAvailable())
                {
                    if (IsKeySellModeHold && !string.IsNullOrEmpty(CachedNewText))
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
            if (IsStashItemHovered && PlayerHelper.FleaIsAvailable())
            {
                if (IsKeySellPress)
                {
                    if (LastCacheItem != null && PlayerHelper.Session.Profile.Examined(LastCacheItem) && !PlayerHelper.HasRaidStarted())
                    {
                        if(!PlayerHelper.CanBeSelectedAtRagfair(LastCacheItem))
                            return;

                        FleaUtils.TryGetPrice(LastCacheItem, price =>
                        {
                            LastCachePrice = price;
                            if(price <= 0)
                                return;
                                
                            FleaUtils.TryAddOfferToFlea(LastCacheItem, LastCachePrice, _ =>
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
