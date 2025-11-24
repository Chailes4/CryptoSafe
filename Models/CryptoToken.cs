using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace CryptoSafe.Models
{
    public class CryptoToken
    {
        [PrimaryKey]
        public Guid TokenId { get; set; }  // Unique ID for each token
        public string ImageUrl { get; set; }
        public string TokenName { get; set; }  // Token name (e.g., USDT, TON, DOGS, CATI)
        public decimal TokenPriceInUSDT { get; set; }  // Store price in USDT
        public decimal Amount { get; set; }  // Amount of the token
        [Indexed]
        public Guid UserId { get; set; }  // Foreign key to the User

        [Ignore]  
        public decimal USDTValue { get; set; }
    }
}
