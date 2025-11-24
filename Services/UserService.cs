using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using CryptoSafe.Models;

namespace CryptoSafe.Services
{
    public class UserService
    {
        private SQLiteAsyncConnection _database;
        private const decimal PesoToUSDTConversionRate = 59.1m;

        private readonly Dictionary<string, decimal> _conversionRates;

        public UserService()
        {
            InitializeDatabase();

            _conversionRates = new Dictionary<string, decimal>
            {
                { "USDT", 1.0m },    // 1 USDT = 1 USDT
                { "TON", 2.5m },     // 1 TON = 2.5 USDT
                { "DOGS", 0.1m },    // 1 DOGS = 0.1 USDT
                { "CATI", 0.05m },    // 1 CATI = 0.05 USDT
                { "BTC", 35000.0m }, // 1 BTC = 35000 USDT
                { "USDC", 1.0m },    // 1 USDC = 1 USDT
                { "SOL", 30.0m },    // 1 SOL = 30 USDT
                { "DOGE", 0.07m },   // 1 DOGE = 0.07 USDT
                { "ETH", 1800.0m },  // 1 ETH = 1800 USDT
                { "BNB", 230.0m },   // 1 BNB = 230 USDT
                { "TRX", 0.09m }     // 1 TRX = 0.09 USDT
            };
        }
        private async Task InitializeDatabase()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "wallet.db");
            _database = new SQLiteAsyncConnection(dbPath);

            try
            {
                await _database.CreateTableAsync<User>();  // Create User table if it doesn't exist
                await _database.CreateTableAsync<CryptoToken>();  // Create CryptoToken table
                await _database.CreateTableAsync<Transaction>();
                await _database.ExecuteAsync("PRAGMA foreign_keys = ON;");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
                throw; // Re-throw the exception to handle it higher up
            }

        }


        //LOGIN FUNCTIONS -------------------------------------------------------------------------

        public async Task<bool> ValidateLoginAsync(string email, string password)
        {
            var user = await _database.Table<User>().Where(u => u.Email == email && u.Password == password).FirstOrDefaultAsync();
            return user != null;
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _database.Table<User>().Where(u => u.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        }


        //REGISTER -------------------------------------------------------------------------------

        public async Task<bool> CreateAccountAsync(string email, string password)
        {
            // Check if the email already exists
            var existingUser = await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return false; // Email already exists
            }

            // Create and add the new user with a unique Id
            User newUser = new User
            {
                Id = Guid.NewGuid(),  // Generate a unique Id
                Email = email,
                Password = password
            };

            await _database.InsertAsync(newUser);  // Insert the user into the database

            // Assign default tokens to the new user
            await AddDefaultTokensToUserAsync(newUser.Id);

            return true;
        }

        public async Task AddDefaultTokensToUserAsync(Guid userId)
        {
            var defaultTokens = new List<CryptoToken>
            {
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "USDT", TokenPriceInUSDT = 1.0m, Amount = 0, UserId = userId, ImageUrl = "usdt.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "TON", TokenPriceInUSDT = 5.3m, Amount = 0, UserId = userId, ImageUrl = "ton.png"},
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "DOGS", TokenPriceInUSDT = 0.1m, Amount = 0, UserId = userId, ImageUrl = "dogs.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "CATI", TokenPriceInUSDT = 0.05m, Amount = 0, UserId = userId, ImageUrl = "cati.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "BTC", TokenPriceInUSDT = 35000.0m, Amount = 0, UserId = userId, ImageUrl = "btc.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "USDC", TokenPriceInUSDT = 1.0m, Amount = 0, UserId = userId, ImageUrl = "usdc.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "SOL", TokenPriceInUSDT = 30.0m, Amount = 0, UserId = userId, ImageUrl = "sol.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "DOGE", TokenPriceInUSDT = 0.07m, Amount = 0, UserId = userId, ImageUrl = "doge.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "ETH", TokenPriceInUSDT = 1800.0m, Amount = 0, UserId = userId, ImageUrl = "eth.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "BNB", TokenPriceInUSDT = 230.0m, Amount = 0, UserId = userId, ImageUrl = "bnb.png" },
                new CryptoToken { TokenId = Guid.NewGuid(), TokenName = "TRX", TokenPriceInUSDT = 0.09m, Amount = 0, UserId = userId, ImageUrl = "trx.png" }
            };

            foreach (var token in defaultTokens)
            {
                await _database.InsertAsync(token);
            }

        }

        //HOMEPAGE --------------------------------------------------------------------------------
        public async Task<List<CryptoToken>> GetUserTokensAsync(Guid userId)
        {
            return await _database.Table<CryptoToken>().Where(t => t.UserId == userId).ToListAsync();
        }

        public decimal GetConversionRateToUSDT(string tokenName)
        {
            if (_conversionRates.ContainsKey(tokenName))
            {
                return _conversionRates[tokenName];
            }
            else
            {
                Console.WriteLine($"Conversion rate for {tokenName} not found.");
                return 0; // Return 0 if the token is not recognized
            }
        }
        public async Task UpdateTokenBalanceAsync(CryptoToken token)
        {
            await _database.UpdateAsync(token);
        }

        //BUY USDT ----------------------------------------------------------------------------

        public async Task<bool> BuyUSDTAsync(Guid userId, decimal pesoAmount)
        {
            const decimal ConversionRate = 59.1m;

            // Calculate the amount of USDT the user will receive
            decimal usdtAmount = pesoAmount / ConversionRate;

            // Fetch the user's USDT token
            var userToken = await _database.Table<CryptoToken>()
                .Where(t => t.UserId == userId && t.TokenName == "USDT")
                .FirstOrDefaultAsync();

            if (userToken != null)
            {
                // Update the user's USDT balance
                userToken.Amount += usdtAmount;
                await _database.UpdateAsync(userToken);
            }
            else
            {
                // If the user doesn't have USDT, create a new entry
                var newToken = new CryptoToken
                {
                    TokenId = Guid.NewGuid(),
                    TokenName = "USDT",
                    TokenPriceInUSDT = 1.0m,  // 1 USDT = 1 USD
                    Amount = usdtAmount,
                    UserId = userId
                };

                await _database.InsertAsync(newToken);
            }

            await AddTransactionAsync(userId, "USDT", usdtAmount, 1.0m, "Buy", senderId: null, receiverId: null);

            return true;
        }

        //TRANSACTION ----------------------------------------------------------------------------------------

        public async Task AddTransactionAsync(Guid userId, string tokenName, decimal amount, decimal priceInUSDT, string transactionType, Guid? senderId = null, Guid? receiverId = null)
        {
            decimal gasFee = 0m;

            if (transactionType == "Send" || transactionType == "Recieve")
            {
                gasFee = 0.001m;
            }
            else if (transactionType == "Swap To" || transactionType == "Swap From")
            {
                gasFee = 0.01m;
            }

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                UserId = userId,
                TokenName = tokenName,
                Amount = amount,
                PriceInUSDT = priceInUSDT,
                Timestamp = DateTime.UtcNow,
                TransactionType = transactionType,
                SenderId = senderId,
                ReceiverId = receiverId,
                GasFee = gasFee
            };

            await _database.InsertAsync(transaction);
        }

        public async Task<List<Transaction>> GetTransactionsAsync(Guid userId)
        {
            return await _database.Table<Transaction>()
                                  .Where(t => t.UserId == userId)
                                  .OrderByDescending(t => t.Timestamp)
                                  .ToListAsync();
        }


        // SEND ------------------------------------------------------------------------------------

        public async Task<bool> TransferTokensAsync(Guid senderId, Guid receiverId, string tokenName, decimal amount)
        {
            // Fetch the sender's token
            var senderToken = await _database.Table<CryptoToken>()
                .Where(t => t.UserId == senderId && t.TokenName == tokenName)
                .FirstOrDefaultAsync();

            if (senderToken == null || senderToken.Amount < amount)
            {
                return false; // Not enough balance or token doesn't exist
            }

            // Deduct the amount from the sender
            senderToken.Amount -= amount;
            await _database.UpdateAsync(senderToken);

            await AddTransactionAsync(senderId, tokenName, -amount, senderToken.TokenPriceInUSDT, "Send", senderId, receiverId);

            // Fetch or create the receiver's token
            var receiverToken = await _database.Table<CryptoToken>()
                .Where(t => t.UserId == receiverId && t.TokenName == tokenName)
                .FirstOrDefaultAsync();

            if (receiverToken == null)
            {
                // Create a new token for the receiver
                receiverToken = new CryptoToken
                {
                    TokenId = Guid.NewGuid(),
                    TokenName = tokenName,
                    TokenPriceInUSDT = senderToken.TokenPriceInUSDT,
                    Amount = amount,
                    UserId = receiverId
                };
                await _database.InsertAsync(receiverToken);
            }
            else
            {
                // Add the amount to the receiver's balance
                receiverToken.Amount += amount;
                await _database.UpdateAsync(receiverToken);
            }

            await AddTransactionAsync(receiverId, tokenName, amount, receiverToken.TokenPriceInUSDT, "Receive", senderId, receiverId);

            return true;
        }

        //SWAP ---------------------------------------------------------------------------------------

        // Add a crypto token to a user
        public async Task AddCryptoTokenToUserAsync(Guid userId, string tokenName, decimal amount)
        {
            // Check if the token already exists for this user
            var token = await _database.Table<CryptoToken>().Where(t => t.TokenName == tokenName && t.UserId == userId).FirstOrDefaultAsync();

            if (token != null)
            {
                // Update the token balance if it already exists for the user
                token.Amount += amount;
                await _database.UpdateAsync(token);
            }
            else
            {
                // Retrieve the receiver's token details to get the ImageUrl
                var receiverToken = await _database.Table<CryptoToken>()
                                                   .Where(t => t.TokenName == tokenName && t.UserId == userId)
                                                   .FirstOrDefaultAsync();

                // If token doesn't exist, create a new one for the user with the receiver's ImageUrl (or default if unavailable)
                var newToken = new CryptoToken
                {
                    TokenId = Guid.NewGuid(),
                    TokenName = tokenName,
                    TokenPriceInUSDT = GetConversionRateToUSDT(tokenName),
                    Amount = amount,
                    UserId = userId,
                    ImageUrl = receiverToken?.ImageUrl // Use receiver's token ImageUrl if it exists, otherwise null or a default image
                };

                await _database.InsertAsync(newToken);
            }
        }


    }
}
