using BepInEx.Configuration;
using UnityEngine;

namespace FastSellInFlea.Models
{
	/// <summary>
	/// Model with config fields
	/// </summary>
	public class SettingsModel
	{
		public static SettingsModel Instance { get; private set; }
		
		public ConfigEntry<KeyboardShortcut> KeyBind;
		public ConfigEntry<KeyboardShortcut> KeyBindHover;
		public ConfigEntry<AutoFleaPrice> OfferPresetFlea;
		public ConfigEntry<TypeMathPrice> TypeAdjustPrice;
		public ConfigEntry<int> SubstractPriceValue;
		public ConfigEntry<int> SubstractPricePercent;

		private SettingsModel(ConfigFile configFile)
		{
			KeyBind = configFile.Bind(
				"Settings", 
				"Quick Offer Hold Key", 
				new KeyboardShortcut(KeyCode.LeftShift), 
				new ConfigDescription(
					"Hold this key to show quick offer adding when opened context menu",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 5
					})); 
			
			KeyBindHover = configFile.Bind(
				"Settings", 
				"Quick Offer Press Key When Hover", 
				new KeyboardShortcut(KeyCode.RightShift), 
				new ConfigDescription(
					"Press keybind when hover on item",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 4
					}));
			
			OfferPresetFlea = configFile.Bind(
				"Settings", 
				"Flea Market Price Preset",
				AutoFleaPrice.Average,
				new ConfigDescription(
					"Choose how your offer price will be based on market values",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 3
					}));
			
			TypeAdjustPrice = configFile.Bind(
				"Settings", 
				"Type Adjust Price",
				TypeMathPrice.Value,
				new ConfigDescription(
					"TypeValue -> RawPrice - Value\nTypePercent -> RawPrice - percent%(RawPrice)",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 2
					}));
			
			SubstractPriceValue = configFile.Bind(
				"Settings", 
				"Price Subtract Value",
				1,
				new ConfigDescription(
					"The amount to subtract from the fetched flea price before add offer.",
					new AcceptableValueRange<int>(0, 9999999), 
					new ConfigurationManagerAttributes
					{
						Order = 1
					}
				));
			
			SubstractPricePercent = configFile.Bind(
				"Settings", 
				"Price Subtract Percent %",
				1,
				new ConfigDescription(
					"The percent% to subtract from the fetched flea price before add offer.",
					new AcceptableValueRange<int>(0, 99), 
					new ConfigurationManagerAttributes
					{
						Order = 0
					}
				));
		}
		
		/// <summary>
		/// Init configs model
		/// </summary>
		/// <param name="configFile"></param>
		/// <returns></returns>
		public static SettingsModel Create(ConfigFile configFile)
		{
			if (Instance != null)
			{
				return Instance;
			}
			return Instance = new SettingsModel(configFile);
		}
	}
}