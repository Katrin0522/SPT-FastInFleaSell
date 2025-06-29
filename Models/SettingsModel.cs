using BepInEx.Configuration;
using UnityEngine;

namespace FastSoldInFlea.Models
{
	/// <summary>
	/// Model with config fields
	/// </summary>
	public class SettingsModel
	{
		public static SettingsModel Instance { get; private set; }
		
		public ConfigEntry<KeyboardShortcut> KeyBind;
		public ConfigEntry<AutoFleaPrice> OfferPresetFlea;
		public ConfigEntry<int> AdjustPriceValue;

		private SettingsModel(ConfigFile configFile)
		{
			KeyBind = configFile.Bind(
				"Settings", 
				"Quick Offer Hold Key", 
				new KeyboardShortcut(KeyCode.LeftShift), 
				"Hold this key to show quick offer adding when opened context menu"); 
			OfferPresetFlea = configFile.Bind(
				"Settings", 
				"Flea Market Price Preset",
				AutoFleaPrice.Average,
				"Choose how your offer price will be based on market values");
			AdjustPriceValue = configFile.Bind(
				"Settings", 
				"Price Subtract Value",
				1,
				"The amount to subtract from the fetched flea price before add offer.");
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