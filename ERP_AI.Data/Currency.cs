using ERP_AI.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP_AI.Data
{
    public class Currency : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // USD, EUR, GBP, etc.
        public string Name { get; set; } = string.Empty; // US Dollar, Euro, British Pound
        public string Symbol { get; set; } = string.Empty; // $, €, £
        public decimal ExchangeRate { get; set; } = 1.0m; // Rate to base currency
        public bool IsBaseCurrency { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int DecimalPlaces { get; set; } = 2;
        public string Format { get; set; } = "{0:C}"; // Currency format string
    }

    public class ExchangeRate : BaseEntity
    {
        public string FromCurrencyCode { get; set; } = string.Empty;
        public string ToCurrencyCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Source { get; set; } = string.Empty; // Manual, API, etc.
    }

    public static class CurrencyHelper
    {
        public static List<Currency> GetDefaultCurrencies()
        {
            return new List<Currency>
            {
                new() { Code = "USD", Name = "US Dollar", Symbol = "$", IsBaseCurrency = true, ExchangeRate = 1.0m, DecimalPlaces = 2, Format = "{0:C}" },
                new() { Code = "EUR", Name = "Euro", Symbol = "€", ExchangeRate = 0.85m, DecimalPlaces = 2, Format = "€{0:N2}" },
                new() { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRate = 0.73m, DecimalPlaces = 2, Format = "£{0:N2}" },
                new() { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", ExchangeRate = 110.0m, DecimalPlaces = 0, Format = "¥{0:N0}" },
                new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", ExchangeRate = 1.25m, DecimalPlaces = 2, Format = "C${0:N2}" },
                new() { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", ExchangeRate = 1.35m, DecimalPlaces = 2, Format = "A${0:N2}" },
                new() { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", ExchangeRate = 0.92m, DecimalPlaces = 2, Format = "CHF {0:N2}" },
                new() { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", ExchangeRate = 6.45m, DecimalPlaces = 2, Format = "¥{0:N2}" },
                new() { Code = "INR", Name = "Indian Rupee", Symbol = "₹", ExchangeRate = 74.0m, DecimalPlaces = 2, Format = "₹{0:N2}" },
                new() { Code = "BRL", Name = "Brazilian Real", Symbol = "R$", ExchangeRate = 5.2m, DecimalPlaces = 2, Format = "R${0:N2}" }
            };
        }

        public static string FormatCurrency(decimal amount, Currency currency)
        {
            if (currency == null) return amount.ToString("C");
            
            var formattedAmount = amount.ToString($"N{currency.DecimalPlaces}");
            return string.Format(currency.Format, formattedAmount);
        }

        public static decimal ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (fromCurrency == null || toCurrency == null) return amount;
            if (fromCurrency.Code == toCurrency.Code) return amount;

            // Convert to base currency first, then to target currency
            var baseAmount = amount / fromCurrency.ExchangeRate;
            return baseAmount * toCurrency.ExchangeRate;
        }

        public static decimal GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, List<ExchangeRate> rates)
        {
            if (fromCurrencyCode == toCurrencyCode) return 1.0m;

            var rate = rates.FirstOrDefault(r => 
                r.FromCurrencyCode == fromCurrencyCode && 
                r.ToCurrencyCode == toCurrencyCode &&
                r.EffectiveDate <= DateTime.Now &&
                (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now));

            return rate?.Rate ?? 1.0m;
        }
    }
}
