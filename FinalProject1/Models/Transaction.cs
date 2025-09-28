using FinalProject1.Enums;

namespace FinalProject1.Models
{
    public class Transaction
    {
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public CurrencyCode? CurrencyCode { get; set; }
        public decimal? Amount { get; set; }
        public string Description { get; set; }
    }
}
