using System.Collections.Generic;
using EFT.InventoryLogic;

namespace FastSellInFlea.Utils
{
    public class FleaCallbackProcessor
    {
        public void ProcessCallback()
        {
            foreach (KeyValuePair<Item, ItemAddress> keyValuePair in OfferItemDict)
            {
                keyValuePair.Deconstruct(out var item, out var itemAddress);
                itemAddress.RaiseRemoveEvent(item, itemAddress.Equals(item.CurrentAddress) ? CommandStatus.Failed : CommandStatus.Succeed, ItemController);
            }
        }
        
        public Dictionary<Item, ItemAddress> OfferItemDict = new();
        
        public ItemController ItemController;
    }
}
