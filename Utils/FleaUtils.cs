using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.Trading;
using FastSellInFlea.Models;
using Newtonsoft.Json;
using SPT.Common.Http;
using UnityEngine;

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
            var dataOffer = new BarterTemplate()
            {
                count = adjustedPrice,
                _tpl = "5449016a4bdc2d6f028b456f"
            };

            var listItems = items.ToList();
            
            if (listItems.Count == 0 || !(listItems[0].Owner is ItemController itemController) ||
                listItems.Any(item => !ReferenceEquals(item.Owner, itemController)))
            {
                return;
            }

            foreach (var item in listItems)
            {
                RequestHandler.PutJson("/client/ragfair/offerfees", new
                {
                    id = item.Id,
                    tpl = item.TemplateId,
                    count = listItems.Count(),
                    fee = Mathf.CeilToInt((float)PriceCalculator.CalculateTaxPrice(item, item.StackObjectsCount, adjustedPrice, true))
                
                }.ToJson());
            }
            
            var allIds = listItems
                .Select(item => item.Id)
                .ToArray();
            
            var callbackProcessor = new FleaCallbackProcessor();
            
            callbackProcessor.ItemController = itemController;
            foreach (Item item in listItems)
            {
                item.Parent.RaiseRemoveEvent(item, CommandStatus.Begin, itemController);
                callbackProcessor.OfferItemDict.Add(item, item.Parent);
            }
            BarterTemplate[] gs = [dataOffer];
            PlayerHelper.Session.RagFair.AddOffer(false, allIds, gs, callbackProcessor.ProcessCallback);
            NotificationManager.DisplayMessageNotification(LocalizationModel.Instance.GetLocaleText(TypeText.AddOffer, dataOffer.count), ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
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
            if (item == null || !(item.Owner is ItemController itemController))
            {
                callback(false);
                return;
            }

            //Used some code from LootValue repository
            var dataOffer = new BarterTemplate()
            {
                count = adjustedPrice,
                _tpl = "5449016a4bdc2d6f028b456f"
            };

            RequestHandler.PutJson("/client/ragfair/offerfees", new
            {
                id = item.Id,
                tpl = item.TemplateId,
                count = 1,
                fee = Mathf.CeilToInt((float)PriceCalculator.CalculateTaxPrice(item, item.StackObjectsCount, adjustedPrice, true))
            
            }.ToJson());
            
            var callbackProcessor = new FleaCallbackProcessor();
            
            callbackProcessor.ItemController = itemController;

            item.Parent.RaiseRemoveEvent(item, CommandStatus.Begin, itemController);
            callbackProcessor.OfferItemDict.Add(item, item.Parent);
            
            BarterTemplate[] gs = [dataOffer];
            PlayerHelper.Session.RagFair.AddOffer(false, new string[1] { item.Id }, gs, callbackProcessor.ProcessCallback);
            NotificationManager.DisplayMessageNotification(LocalizationModel.Instance.GetLocaleText(TypeText.AddOffer, dataOffer.count), ENotificationDurationType.Default, ENotificationIconType.EntryPoint);
            callback(true);
        }
    
        /// <summary>
        /// Get price with preset from config model 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="callback"></param>
        public static void TryGetPrice(Item item, Action<double> callback)
        {
            if (callback == null)
                return;

            var session = PlayerHelper.Session;
            var templateId = item == null ? null : item.TemplateId.ToString();
            if (session == null || string.IsNullOrWhiteSpace(templateId) || templateId.Length != 24 ||
                !templateId.All(Uri.IsHexDigit))
            {
                callback(0.0);
                return;
            }

            session.GetMarketPrices(templateId, result =>
            {
                if (!result.Succeed || result.Value == null)
                {
                    callback(0.0);
                    return;
                }

                var prices = result.Value;
                if (float.IsNaN(prices.min) || float.IsInfinity(prices.min) ||
                    float.IsNaN(prices.avg) || float.IsInfinity(prices.avg) ||
                    float.IsNaN(prices.max) || float.IsInfinity(prices.max))
                {
                    callback(0.0);
                    return;
                }

                double price = 0;
                if (SettingsModel.Instance.TypeAdjustPrice.Value == TypeMathPrice.Value)
                {
                    price = SettingsModel.Instance.OfferPresetFlea.Value switch
                    {
                        AutoFleaPrice.Minimum => prices.min - SettingsModel.Instance.SubstractPriceValue.Value,
                        AutoFleaPrice.Average => prices.avg - SettingsModel.Instance.SubstractPriceValue.Value,
                        AutoFleaPrice.Maximum => prices.max - SettingsModel.Instance.SubstractPriceValue.Value,
                        _ => prices.avg - 1
                    };

                    if (prices.min <= 0.0f && prices.avg <= 0.0f && prices.max <= 0.0f)
                    {
                        callback(0.0);
                        return;
                    }
                }
                else if (SettingsModel.Instance.TypeAdjustPrice.Value == TypeMathPrice.Percent)
                {
                    price = SettingsModel.Instance.OfferPresetFlea.Value switch
                    {
                        AutoFleaPrice.Minimum => prices.min * (1 - (SettingsModel.Instance.SubstractPricePercent.Value / 100.0)),
                        AutoFleaPrice.Average => prices.avg * (1 - (SettingsModel.Instance.SubstractPricePercent.Value / 100.0)),
                        AutoFleaPrice.Maximum => prices.max * (1 - (SettingsModel.Instance.SubstractPricePercent.Value / 100.0)),
                        _ => prices.avg * (1 - (99 / 100.0)),
                    };

                    if (prices.min <= 0.0f && prices.avg <= 0.0f && prices.max <= 0.0f)
                    {
                        callback(0.0);
                        return;
                    }
                }

                price = Math.Max(0, (int)Math.Round(price));
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
