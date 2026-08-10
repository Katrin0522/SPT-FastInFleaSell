using Comfort.Common;
using System;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using EFT.UI.Ragfair;
using EFT.UI.Screens;
using SPT.Reflection.Utils;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FastSellInFlea.Utils;

public static class PlayerHelper
{
    public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
    
    public static bool HasRaidStarted()
    {			
        bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
        return inRaid.HasValue && inRaid.Value;
    }
        
    public static bool FleaIsAvailable()
    {
        if (Session != null)
        {
            var rag = Session.RagFair;
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
        if (!CanQuickSell(item) || !(item.Owner is TraderControllerClass itemController))
            return false;

        var ragFair = Session?.RagFair;
        if (ragFair == null || !ragFair.Available)
            return false;
        if (ragFair.MyOffersCount >= ragFair.MaxOffersCount)
            return false;
        if (item.PinLockState != EItemPinLockState.Free)
            return false;

        return RagFairClass.CanBeSelectedAtRagfair(item, itemController, out _);
    }

    public static bool CanQuickSell(Item item)
    {
        if (item == null || IsTextInputFocused() || IsPostRaidScavTransferOpen())
            return false;

        var inventory = Session?.Profile?.Inventory;
        return inventory != null && inventory.GetPlayerItems(EPlayerItems.All)
            .Any(playerItem => ReferenceEquals(playerItem, item));
    }

    public static bool IsTextInputFocused()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null)
            return false;

        var tmpInput = selected.GetComponentInParent<TMP_InputField>();
        if (tmpInput != null && tmpInput.isActiveAndEnabled && tmpInput.isFocused)
            return true;

        var input = selected.GetComponentInParent<InputField>();
        return input != null && input.isActiveAndEnabled && input.isFocused;
    }

    private static bool IsPostRaidScavTransferOpen()
    {
        return CurrentScreenSingletonClass.Instance?.CheckCurrentScreen(EEftScreenType.ScavInventory) == true;
    }
}
