using FinalProject1.Enums;
using FinalProject1.Models;
using Serilog;

Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs"));
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt"),
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    string currencyRatesFilePath = WriteCurrencyRatesInFile();
    List<Card> existingCards = GetInitiatedCards();

    Console.WriteLine("=== ATM Simulator ===");
    Console.WriteLine("Welcome!");
    Console.WriteLine();

    // Enter Card Detils
    Console.Write("Enter Card Number: ");
    string cardNumber = (Console.ReadLine() ?? string.Empty).Trim();

    Console.Write("Enter Expiry Month (MM): ");
    int expiryMonth = ReadInt("Enter number: ");

    Console.Write("Enter Expiry Year (YYYY): ");
    int expiryYear = ReadInt("Enter number: ");

    Console.Write("Enter CVC: ");
    int cvc = ReadInt("Enter number: ");

    Card? card = existingCards.FirstOrDefault(c =>
        c.CardNumber == cardNumber &&
        c.ExpiryMonth == expiryMonth &&
        c.ExpiryYear == expiryYear &&
        c.CVC == cvc);
    if (card == null)
    {
        throw new Exception("Please provide correct data");
    }

    DateTime now = DateTime.Now;
    if (card.ExpiryYear < now.Year || (card.ExpiryYear == now.Year && card.ExpiryMonth < now.Month)) 
    {
        throw new Exception("Card expired. Please try with another card.");
    }

    Console.Write("Enter PIN: ");
    string? pin = Console.ReadLine()?.Trim();
    if (pin != card.PinCode)
    {
        throw new Exception("Please provide correct pin");
    }

    Console.WriteLine("\nLogin successful.");

    // MENU
    bool keepGoing = true;
    while (keepGoing)
    {
        Console.WriteLine();
        Console.WriteLine("Select an option:");
        Console.WriteLine("1) Check Deposit (balances) ");
        Console.WriteLine("2) Get Amount (withdraw) ");
        Console.WriteLine("3) Get Transactions ");
        Console.WriteLine("4) Add Amount (top-up) ");
        Console.WriteLine("5) Change Pin");
        Console.WriteLine("6) Change Amount (currency exchange) ");
        Console.WriteLine("0) Exit ");
        Console.Write("Your choice: ");

        int operation = ReadInt("Choose the operation ");
        Console.WriteLine();

        try
        {
            switch (operation)
            {
                case 1: // GetBalances
                    {
                        List<BalanceAmount> balances = card.GetBalances();
                        PrintBalances(balances);
                        break;
                    }
                case 2: // Withdraw
                    {
                        Console.WriteLine("Your current balances:");
                        PrintBalances(card.GetBalances());

                        CurrencyCode code = ChooseCurrency("Choose currency code to withdraw:");
                        decimal amount = ReadDecimal("Enter amount to withdraw: ");

                        List<BalanceAmount> balances = card.Withdraw(amount, code);
                        Console.WriteLine("Updated balances after withdrawal:");
                        PrintBalances(balances);
                        break;
                    }
                case 3: // GetTransactions
                    {
                        Console.WriteLine("Limit number of transactions?");
                        Console.WriteLine("1) Yes");
                        Console.WriteLine("2) No");
                        int yn = ReadInt("Choose 1-2: ");

                        if (yn == 1)
                        {
                            int count = ReadInt("Enter count: ");

                            List<Transaction> tx = card.GetTransactions(count);
                            PrintTransactions(tx);
                            break;
                        }
                        else
                        {
                            List<Transaction> tx = card.GetTransactions();
                            PrintTransactions(tx);
                            break;
                        }
                    }
                case 4: // TopUpBalance
                    {
                        Console.WriteLine("Your current balances:");
                        PrintBalances(card.GetBalances());

                        CurrencyCode code = ChooseCurrency("Choose currency code to top-up:");
                        decimal amount = ReadDecimal("Enter amount to add: ");

                        List<BalanceAmount> balances = card.TopUpBalance(amount, code);
                        Console.WriteLine("Updated balances after top-up:");
                        PrintBalances(balances);
                        break;
                    }

                case 5: // ChangePinCode
                    {
                        Console.Write("Enter new 4-digit PIN: ");
                        string newPin = Console.ReadLine()!.Trim();
                        card.ChangePinCode(newPin);
                        Console.WriteLine("PIN changed successfully.");
                        break;
                    }
                case 6: // ExchangeMoney
                    {
                        Console.WriteLine("Your current balances:");
                        PrintBalances(card.GetBalances());

                        CurrencyCode source = ChooseCurrency("Choose source currency:");
                        CurrencyCode target = ChooseCurrency("Choose target currency:");
                        decimal amount = ReadDecimal("Enter amount to exchange: ");

                        List<BalanceAmount> balances = card.ExchangeMoney(amount, target, source, currencyRatesFilePath);
                        Console.WriteLine("Updated balances after exchange:");
                        PrintBalances(balances);
                        break;
                    }
                case 0:
                    keepGoing = false;
                    continue;
                default:
                    Console.WriteLine("Unknown option. Please try again ");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Do you want to perform another operation?");
        Console.WriteLine("1) Yes");
        Console.WriteLine("2) No");
        int cont = ReadInt("Choose 1-2: ");
        keepGoing = cont == 1;
    }

    Console.WriteLine("Goodbye!");
}
catch (Exception ex)
{
    Log.Error(ex, "Exception happened in ATM Simulator");

    Console.WriteLine();
    Console.WriteLine("Some Error was occured! See the message bellow:");
    Console.WriteLine(ex.Message);
}


int ReadInt(string prompt)
{
    Console.Write(prompt);

    int intInput;
    while (!int.TryParse(Console.ReadLine(), out intInput))
    {
        Console.Write("Invalid number. Try again ");
    }
    return intInput;
}

decimal ReadDecimal(string prompt)
{
    Console.Write(prompt);

    decimal decimalInput;
    while (!decimal.TryParse(Console.ReadLine(), out decimalInput))
    {
        Console.Write("Invalid amount. Try again ");
    }
    return decimalInput;
}

CurrencyCode ChooseCurrency(string prompt)
{
    Console.WriteLine(prompt);
    Console.WriteLine("1) GEL");
    Console.WriteLine("2) USD");
    Console.WriteLine("3) EUR");

    while (true)
    {
        int n = ReadInt("Choose 1-3: ");
        switch (n)
        {
            case 1: return CurrencyCode.GEL;
            case 2: return CurrencyCode.USD;
            case 3: return CurrencyCode.EUR;
            default: Console.WriteLine("Invalid choice. Try again "); break;
        }
    }
}

void PrintBalances(List<BalanceAmount> balances)
{
    if (balances.Count == 0)
    {
        Console.WriteLine("(no accounts)");
        return;
    }

    foreach (var b in balances.OrderBy(x => x.CurrencyCode))
    {
        Console.WriteLine($" - {b.CurrencyCode}: {b.Amount}");
    }
}

void PrintTransactions(List<Transaction> transactions)
{
    if (transactions.Count == 0)
    {
        Console.WriteLine("(no transactions)");
        return;
    }

    foreach (var t in transactions)
    {
        Console.WriteLine($"{t.TransactionDate:u} | {t.TransactionType} | {t.CurrencyCode} | {t.Amount} | {t.Description}");
    }
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
                CurrencyCode = CurrencyCode.USD,
                Amount = -50.00m,
                Description = "Converted -50 USD to +136 GEL (rate 2.72)"
            },
            new Transaction
            {
                TransactionDate = now,
                TransactionType = TransactionType.CurrencyConversion,
                CurrencyCode = CurrencyCode.GEL,
                Amount = 136.00m,
                Description = "Converted -50 USD to +136 GEL (rate 2.72)"
            }
        }
    };

    cards.Add(card1);
    cards.Add(card2);
    cards.Add(card3);

    return cards;
}

#endregion