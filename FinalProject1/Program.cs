using FinalProject1.Enums;
using FinalProject1.Models;
using System.IO;

try
{
    string currencyRatesFilePath = WriteCurrencyRatesInFile();
    List<Card> existingCards = GetInitiatedCards();

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


#region Initiate Data

string WriteCurrencyRatesInFile()
{
    string dir = Path.Combine(AppContext.BaseDirectory, "Data");
    Directory.CreateDirectory(dir);

    string path = Path.Combine(dir, "rates.txt");
    string[] lines =
    {
        "USD=2.72",
        "EUR=3.9",
        "GEL=1.00"
    };

    File.WriteAllLines(path, lines);
    return path;
}

List<Card> GetInitiatedCards()
{
    List<Card> cards = new List<Card>();

    // Card 1
    Card card1 = new Card
    {
        CardNumber = "11111",
        PinCode = "1234",
        CVC = 123,
        ExpiryMonth = 12,
        ExpiryYear = 2026,
        Customer = new Customer
        {
            FirstName = "Goga",
            LastName = "Gogidze"
        },
        Accounts = new List<Account>
                {
                    new Account
                    {
                        CurrencyCode = CurrencyCode.USD,
                        Balance = 0m
                    },
                    new Account
                    {
                        CurrencyCode = CurrencyCode.EUR,
                        Balance = 250.75m
                    },
                    new Account
                    {
                        CurrencyCode = CurrencyCode.GEL,
                        Balance = 1025.00m
                    }
                },
        Transactons = new List<Transaction>()
    };

    // Card 2
    Card card2 = new Card
    {
        CardNumber = "22222",
        PinCode = "4321",
        CVC = 456,
        ExpiryMonth = 8,
        ExpiryYear = 2025,
        Customer = new Customer
        {
            FirstName = "Giorgi",
            LastName = "Kapanadze"
        },
        Accounts = new List<Account>
                {
                    new Account
                    {
                        CurrencyCode = CurrencyCode.USD,
                        Balance = 780.40m
                    },
                    new Account
                    {
                        CurrencyCode = CurrencyCode.EUR,
                        Balance = 0m
                    },
                    new Account
                    {
                        CurrencyCode = CurrencyCode.GEL,
                        Balance = 50.00m
                    }
                },
        Transactons = new List<Transaction>()
    };

    // Card 3
    var now = DateTime.Now;
    Card card3 = new Card
    {
        CardNumber = "33333",
        PinCode = "1111",
        CVC = 789,
        ExpiryMonth = 5,
        ExpiryYear = 2028,
        Customer = new Customer
        {
            FirstName = "Mariam",
            LastName = "Gelashvili"
        },
        Accounts = new List<Account>
                {
                    new Account
                    {
                        CurrencyCode = CurrencyCode.USD,
                        Balance = 200.00m
                    },
                    new Account
                    {
                        CurrencyCode = CurrencyCode.EUR,
                        Balance = 146.00m
                    },
                    new Account
                    {
                        CurrencyCode = CurrencyCode.GEL,
                        Balance = 260.00m
                    }
                },
        Transactons = new List<Transaction>
                {
                    new Transaction
                    {
                        TransactionDate = now.AddDays(-2),
                        TransactionType = TransactionType.Withdrawal,
                        CurrencyCode = CurrencyCode.GEL,
                        Amount = 40.00m,
                        Description = "ATM cash withdrawal"
                    },
                    new Transaction
                    {
                        TransactionDate = now.AddDays(-1),
                        TransactionType = TransactionType.BalanceTopUp,
                        CurrencyCode = CurrencyCode.USD,
                        Amount = 50.00m,
                        Description = "Cash deposit at branch"
                    },
                    new Transaction
                    {
                        TransactionDate = now,
                        TransactionType = TransactionType.CurrencyConversion,
                        CurrencyCode = CurrencyCode.EUR,
                        Amount = 46.00m,
                        Description = "Converted 50 USD to EUR (rate 0.92)"
                    }
                }
    };

    cards.Add(card1);
    cards.Add(card2);
    cards.Add(card3);

    return cards;
}

#endregion