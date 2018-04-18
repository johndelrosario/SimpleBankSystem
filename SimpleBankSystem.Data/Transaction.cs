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
        [Key]
        public long Id { get; set; }

        [ForeignKey("DebitAccountUser")]
        public string DebitAccount { get; set; }

        [ForeignKey("CreditAccountUser")]
        public string CreditAccount { get; set; }

        public double Amount { get; set; }

        public string Remarks { get; set; }

        public DateTime DateCreated { get; set; }

        public User DebitAccountUser { get; set; }

        public User CreditAccountUser { get; set; }
    }
}
