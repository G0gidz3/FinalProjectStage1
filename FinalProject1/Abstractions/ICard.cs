using FinalProject1.Enums;
using FinalProject1.Models;

namespace FinalProject1.Abstractions
{
    public interface ICard
    {
        List<BalanceAmount> GetBalances();
        List<BalanceAmount> TopUpBalance(decimal amount, CurrencyCode currencyCode);
        List<BalanceAmount> Withdraw(decimal amount, CurrencyCode currencyCode);
        void ChangePinCode(string newPinCode);
        List<BalanceAmount> ExchangeMoney(decimal exchangeAmount, CurrencyCode targetCurrencyCode, CurrencyCode sourceCurrencyCode, string exchangeRateFilePath);
        List<Transaction> GetTransactions(int count = 5);
    }
}
