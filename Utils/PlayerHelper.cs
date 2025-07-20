using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Utils;

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
        if (item.Owner.OwnerType != EOwnerType.Profile && item.Owner.GetType() == typeof(TraderControllerClass))
            return false;
        if (!item.CanSellOnRagfair)
            return false;
        if (Session.RagFair.MyOffersCount >= Session.RagFair.MaxOffersCount)
            return false;
        if (item.PinLockState != EItemPinLockState.Free)
            return false;
        if (item.IsNotEmpty())
            return false;
            
                
        return true;
    }
}