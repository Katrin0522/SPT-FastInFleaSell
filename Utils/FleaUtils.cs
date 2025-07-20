using System;
using System.Collections.Generic;
using System.Linq;
using EFT.Communications;
using EFT.InventoryLogic;
using FastSellInFlea.Models;
using FleaRequirement = GClass2102;

namespace FastSellInFlea.Utils
{
    public static class FleaUtils
    {
        /// <summary>
        /// Trying to add offer to flea with Item and Adjusted price
        /// </summary>
        /// <param name="items"></param>
        /// <param name="adjustedPrice"></param>
        public static void TryAddOfferToFlea(IEnumerable<Item> items, double adjustedPrice)
        {
            //Used some code from LootValue repository
            var dataOffer = new FleaRequirement()
            {
                count = adjustedPrice,
                _tpl = "5449016a4bdc2d6f028b456f"
            };
            string[] allIds = items
                .Select(item => item.Id)
                .ToArray();
            FleaRequirement[] gs = new FleaRequirement[1] { dataOffer };
            PlayerHelper.Session.RagFair.AddOffer(false, allIds, gs, null);
            NotificationManagerClass.DisplayMessageNotification(LocalizationModel.Instance.GetLocaleText(TypeText.AddOffer, dataOffer.count), ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
        }
    
        /// <summary>
        /// Overload for single item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="adjustedPrice"></param>
        public static void TryAddOfferToFlea(Item item, double adjustedPrice)
        {
            TryAddOfferToFlea(new[] { item }, adjustedPrice);
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
            PlayerHelper.Session.RagFair.AddOffer(false, new string[1] { item.Id }, gs, null);
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
            PlayerHelper.Session.GetMarketPrices(item.TemplateId, result =>
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
                            FastSellInFleaPlugin.LastCachePrice = 0.0;
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
                            FastSellInFleaPlugin.LastCachePrice = 0.0;
                            callback(0.0f);
                        }
                    }
                }
                price = Math.Max(0, (int)Math.Round(price));
                FastSellInFleaPlugin.LastCachePrice = price;
                callback(price);
            });
        }

        /// <summary>
        /// Groups items (same template/type/condition)
        /// </summary>
        /// <param name="items"></param>
        public static List<List<Item>> GroupSimilarItems(IEnumerable<Item> items)
        {
            var itemGroups = new List<List<Item>>();
            foreach (var item in items)
            {
                var matchingGroup = itemGroups.FirstOrDefault(group =>
                    group.Any(groupItem => groupItem.Compare(item)));

                if (matchingGroup != null)
                {
                    matchingGroup.Add(item);
                }
                else
                {
                    itemGroups.Add(new List<Item> { item });
                }
            }
            return itemGroups;
        }
    }
}