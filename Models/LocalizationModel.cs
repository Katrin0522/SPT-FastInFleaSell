using System.Collections.Generic;
using Comfort.Common;

namespace FastSellInFlea.Models
{
    /// <summary>
    /// Model for getting localization
    /// </summary>
    public class LocalizationModel
    {
        private Dictionary<string, string> LocalizationAddOffer = new Dictionary<string, string>
        {
            { "ch", "已添加报价，金额 {0}RUB" },
            { "cz", "Nabídka přidána za {0}RUB" },
            { "en", "Offer added for {0}RUB" },
            { "fr", "Offre ajoutée pour {0}RUB" },
            { "ge", "Angebot hinzugefügt für {0}RUB" },
            { "hu", "Ajánlat hozzáadva {0}RUB értékben" },
            { "it", "Offerta aggiunta per {0}RUB" },
            { "jp", "{0}RUB でオファーが追加されました" },
            { "kr", "{0}RUB로 오퍼가 추가되었습니다" },
            { "pl", "Dodano ofertę za {0}RUB" },
            { "po", "Oferta adicionada por {0}RUB" },
            { "sk", "Ponuka pridaná za {0}RUB" },
            { "es", "Oferta añadida por {0}RUB" },
            { "es-mx", "Oferta añadida por {0}RUB" },
            { "tu", "{0}RUB karşılığında teklif eklendi" },
            { "ru", "Добавлен лот за {0}RUB" },
            { "ro", "Ofertă adăugată pentru {0}RUB" }
        };
        
        private Dictionary<string, string> LocalizationErrorAddOffer = new Dictionary<string, string>
        {
            { "ch", "添加报价时出错" },
            { "cz", "Chyba při přidávání nabídky" },
            { "en", "Error while adding offer" },
            { "fr", "Erreur lors de l'ajout de l'offre" },
            { "ge", "Fehler beim Hinzufügen des Angebots" },
            { "hu", "Hiba az ajánlat hozzáadásakor" },
            { "it", "Errore durante l'aggiunta dell'offerta" },
            { "jp", "オファーの追加中にエラーが発生しました" },
            { "kr", "오퍼 추가 중 오류가 발생했습니다" },
            { "pl", "Błąd podczas dodawania oferty" },
            { "po", "Erro ao adicionar a oferta" },
            { "sk", "Chyba pri pridávaní ponuky" },
            { "es", "Error al añadir la oferta" },
            { "es-mx", "Error al añadir la oferta" },
            { "tu", "Teklif eklenirken hata oluştu" },
            { "ru", "Ошибка при добавлении лота" },
            { "ro", "Eroare la adăugarea ofertei" }
        };
        
        public static LocalizationModel Instance { get; private set; }

        private string CurrentLanguage()
        {
            if (Singleton<SharedGameSettingsClass>.Instance.Game.Settings.Language == null)
            {
                return "en";
            }
            return Singleton<SharedGameSettingsClass>.Instance.Game.Settings.Language;
        }
        
        public static LocalizationModel Create()
        {
            if (Instance != null)
            {
                return Instance;
            }
            return Instance = new LocalizationModel();
        }

        /// <summary>
        /// Get locale text for needly type. See lang in game settings.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetLocaleText(TypeText type, double price = 0.0)
        {
            if (type == TypeText.AddOffer)
            {
                if (LocalizationAddOffer.TryGetValue(CurrentLanguage(), out var addOfferLocale))
                {
                    return string.Format(addOfferLocale, price);
                }
                return string.Format(LocalizationAddOffer["en"], price); // fallback to English
            }
            else if(type == TypeText.ErrorAddOffer)
            {
                if (LocalizationErrorAddOffer.TryGetValue(CurrentLanguage(), out var addOfferLocale))
                {
                    return addOfferLocale;
                }

                return LocalizationErrorAddOffer["en"]; // fallback to English
            }

            return "Error. Can`t get locale";
        }
    }

    public enum TypeText
    {
        AddOffer,
        ErrorAddOffer
    }
}