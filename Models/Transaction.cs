using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace CryptoSafe.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public string TokenName { get; set; }
        public decimal Amount { get; set; }
        public decimal GasFee { get; set; }
        public decimal PriceInUSDT { get; set; }
        public DateTime Timestamp { get; set; }
        public string TransactionType { get; set; }

        public Guid? SenderId { get; set; }
        public Guid? ReceiverId { get; set; }
    }
}
