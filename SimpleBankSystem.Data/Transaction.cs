using SimpleBankSystem.Data.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SimpleBankSystem.Data
{
    public class Transaction
    {
        public Transaction()
        {
            DateCreated = DateTime.Now;
        }

        [Key]
        public long Id { get; set; }

        [ForeignKey("DebitAccountUser")]
        public string DebitAccount { get; set; }

        [ForeignKey("CreditAccountUser")]
        public string CreditAccount { get; set; }

        [Range(1, double.MaxValue)]
        public double Amount { get; set; }

        public string Remarks { get; set; }

        public DateTime DateCreated { get; set; }

        [InverseProperty("DebitTransactions")]
        public User DebitAccountUser { get; set; }

        [InverseProperty("CreditTransactions")]
        public User CreditAccountUser { get; set; }
    }
}
