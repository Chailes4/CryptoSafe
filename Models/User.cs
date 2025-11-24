using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace CryptoSafe.Models
{
    public class User
    {
        [PrimaryKey]
        public Guid Id { get; set; } // Unique identifier for the user

        [Unique]  // Ensure that the email is unique
        public string Email { get; set; }

        public string Password { get; set; }
        public Guid UserId { get; internal set; }
    }
}
