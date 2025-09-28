using FinalProject1.Abstractions;
using FinalProject1.Enums;
using System.Globalization;
using System.IO;

namespace FinalProject1.Models
{
    public class Card : ICard
    {
        public string CardNumber { get; set; }
        public string PinCode { get; set; }
        public int CVC { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public Customer Customer { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Transaction> Transactons { get; set; } = new List<Transaction>();


        #region Operation Methods

        public List<BalanceAmount> GetBalances()
        {
            List<BalanceAmount> balancesByCurrency = new List<BalanceAmount>();

            foreach (var account in this.Accounts)
            {
                BalanceAmount balance = new BalanceAmount(account.Balance, account.CurrencyCode);
                balancesByCurrency.Add(balance);
            }

            this.Transactons.Add(new Transaction
            {
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.DepositCheck,
                Description = "Get balances operation performed."
            });
            return balancesByCurrency;
        }

        public List<Transaction> GetTransactions(int count = 5)
        {
            List<Transaction> transactions = this.Transactons
                .OrderByDescending(x => x.TransactionDate)
                .Take(count)
                .ToList();

            return transactions;
        }

        public List<BalanceAmount> TopUpBalance(decimal amount, CurrencyCode currencyCode)
        {
            if (amount <= 0m)
            {
                throw new Exception("Top up amount must be greater then 0");
            }

            var account = this.Accounts.FirstOrDefault(x => x.CurrencyCode == currencyCode);
            if (account == null)
            {
                account = new Account
                {
                    CurrencyCode = currencyCode,
                    Balance = 0m
                };
                this.Accounts.Add(account);
            }

            account.Balance += amount;

            this.Transactons.Add(new Transaction
            {
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.BalanceTopUp,
                CurrencyCode = currencyCode,
                Amount = amount,
                Description = $"Top-up {amount} {currencyCode}"
            });

            return this.GetBalances();
        }

        public List<BalanceAmount> Withdraw(decimal amount, CurrencyCode currencyCode)
        {
            if (amount <= 0m)
            {
                throw new Exception("Withdraw amount must be greater then 0");
            }

            var account = this.Accounts.FirstOrDefault(x => x.CurrencyCode == currencyCode);
            if (account == null || account.Balance < amount)
            {
                throw new Exception($"Not enough funds or no account found by currency code: {currencyCode}");
            }

            account.Balance -= amount;

            this.Transactons.Add(new Transaction
            {
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.Withdrawal,
                CurrencyCode = currencyCode,
                Amount = amount,
                Description = $"Withdrawal {amount} {currencyCode}"
            });

            return this.GetBalances();
        }

        public void ChangePinCode(string newPinCode)
        {
            if (string.IsNullOrEmpty(newPinCode))
            {
                throw new Exception("New pin code can not be null or empty");
            }

            if (newPinCode.Length != 4)
            {
                throw new Exception("New pin code length must iclude only 4 digits");
            }

            if (!newPinCode.All(char.IsDigit))
            {
                throw new Exception("New pin must contain only digits");
            }

            this.PinCode = newPinCode;

            Transactons.Add(new Transaction
            {
                TransactionDate = DateTime.UtcNow,
                TransactionType = TransactionType.ChangePin,
                Description = "PIN changed"
            });
        }

        public List<BalanceAmount> ExchangeMoney(decimal exchangeAmount, CurrencyCode targetCurrencyCode, CurrencyCode sourceCurrencyCode, string exchangeRateFilePath)
        {
            if (exchangeAmount <= 0m)
            {
                throw new Exception("Exchange amount must be greater than 0");
            }

            if (targetCurrencyCode == sourceCurrencyCode)
            {
                throw new Exception("Source and target currencies must be different");
            }

            Account? sourceAccount = this.Accounts.FirstOrDefault(a => a.CurrencyCode == sourceCurrencyCode);
            if (sourceAccount == null)
            {
                throw new Exception($"Account for currency {sourceCurrencyCode} does not exist");
            }

            Dictionary<CurrencyCode, decimal> rates = ReadExchangeRates(exchangeRateFilePath);
            if (rates.Count == 0)
            {
                throw new Exception("Exchange rates file not found or empty");
            }

            if (!rates.ContainsKey(sourceCurrencyCode))
            {
                throw new Exception($"Missing exchange rate for {sourceCurrencyCode}");
            }

            if (!rates.ContainsKey(targetCurrencyCode))
            {
                throw new Exception($"Missing exchange rate for {targetCurrencyCode}");
            }

            decimal convertedAmount = ConvertAmount(exchangeAmount, sourceCurrencyCode, targetCurrencyCode, rates);

            if (sourceAccount.Balance < exchangeAmount)
            {
                throw new InvalidOperationException("Insufficient funds for conversion");
            }

            sourceAccount.Balance -= exchangeAmount;

            Account? target = this.Accounts.FirstOrDefault(a => a.CurrencyCode == targetCurrencyCode);
            if (target == null)
            {
                target = new Account
                {
                    CurrencyCode = targetCurrencyCode,
                    Balance = 0m
                };
                Accounts.Add(target);
            }
            target.Balance += convertedAmount;

            DateTime now = DateTime.UtcNow;

            Transactons.Add(new Transaction
            {
                TransactionDate = now,
                TransactionType = TransactionType.CurrencyConversion,
                CurrencyCode = sourceCurrencyCode,
                Amount = -exchangeAmount,
                Description = $"Converted -{exchangeAmount} {sourceCurrencyCode}"
            });

            Transactons.Add(new Transaction
            {
                TransactionDate = now,
                TransactionType = TransactionType.CurrencyConversion,
                CurrencyCode = targetCurrencyCode,
                Amount = convertedAmount,
                Description = $"Converted +{convertedAmount} {targetCurrencyCode}"
            });

            return GetBalances();
        }

        private Dictionary<CurrencyCode, decimal> ReadExchangeRates(string filePath)
        {
            Dictionary<CurrencyCode, decimal> rates = new Dictionary<CurrencyCode, decimal>();
            if (!File.Exists(filePath))
            {
                throw new Exception("Exchange rates not found");
            }

            foreach (var raw in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string[] parts = raw.Split('=', 2);
                CurrencyCode code = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), parts[0].Trim(), ignoreCase: true);
                decimal rate = decimal.Parse(parts[1].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture);

                rates[code] = rate;
            }

            return rates;
        }

        private decimal ConvertAmount(decimal amount, CurrencyCode from, CurrencyCode to, IReadOnlyDictionary<CurrencyCode, decimal> rates)
        {
            if (from == to) return amount;

            if (!rates.TryGetValue(from, out var fromRate))
            {
                throw new Exception($"Exchange rate not found for {from}");
            }

            if (!rates.TryGetValue(to, out var toRate))
            {
                throw new Exception($"Exchange rate not found for {to}");
            }

            decimal amountInBase = amount / fromRate;
            decimal target = Math.Round(amountInBase * toRate, 2, MidpointRounding.AwayFromZero);
            return target;
        }

        #endregion
    }
}
