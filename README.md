# ZabbixMeraki  
Integracja Zabbixa z Cisco Meraki API umożliwiająca automatyczne pobieranie informacji o urządzeniach, ich statusie oraz podstawowych parametrach sieciowych. Projekt powstał jako praktyczne narzędzie do monitoringu infrastruktury Meraki z poziomu Zabbixa.

## 🎯 Cel projektu
Celem aplikacji jest:
- komunikacja z **Cisco Meraki Dashboard API**,
- pobieranie danych o urządzeniach i sieciach,
- przygotowanie danych do dalszego przetwarzania w Zabbixie,
- automatyzacja monitoringu bez konieczności ręcznej konfiguracji.

Projekt pokazuje pracę z integracjami API, obsługą JSON, logiką biznesową oraz przygotowaniem danych pod systemy monitoringu.

## 🧰 Technologie
- **.NET 8**
- **C#**
- **HttpClient / HttpClientFactory**
- **Cisco Meraki Dashboard API**
- **System.Text.Json**
- **Zabbix (integracja danych)**

## 📦 Funkcjonalności
- Pobieranie listy urządzeń z organizacji Meraki  
- Pobieranie statusów urządzeń (online/offline)  
- Pobieranie podstawowych parametrów (model, MAC, public IP, networkId)  
- Serializacja i mapowanie odpowiedzi JSON na modele C#  
- Obsługa klucza API i nagłówków autoryzacyjnych  
- Przygotowanie danych do eksportu do Zabbixa  

*(Dopisz lub usuń funkcje zgodnie z Twoją implementacją.)*

## 🚀 Jak uruchomić
1. Dodaj swój klucz API Meraki do `appsettings.json`:
   ```json
   {
     "MerakiApiKey": "TWÓJ_KLUCZ_API"
   }
