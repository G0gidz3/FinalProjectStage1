using FinalProject1.Enums;

namespace FinalProject1.Models
{
    public class BalanceAmount
    {
        public BalanceAmount(decimal amount, CurrencyCode currencyCode)
        {
            this.Amount = amount;
            this.CurrencyCode = currencyCode;
        }
        public decimal Amount { get; set; }
        public CurrencyCode CurrencyCode { get; set; }
    }
}
