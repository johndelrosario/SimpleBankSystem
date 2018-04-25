using Microsoft.AspNetCore.Identity;
using SimpleBankSystem.Data.Contexts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SimpleBankSystem.Data.Identity
{
    public class User : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountNumber { get; set; }

        public string AccountName { get; set; }

        public DateTime CreatedDate { get; set; }

        [ConcurrencyCheck]
        public double Balance { get; set; }

        [NotMapped]
        public double ComputedBalance
        {
            get
            {
                var balance = 0d;
                var totalDebit = 0d;
                var totalCredit = 0d;

                if (DebitTransactions != null && CreditTransactions != null)
                {
                    totalDebit = DebitTransactions.Sum(tr => tr.Amount);
                    totalCredit = CreditTransactions.Sum(tr => tr.Amount);
                    balance = totalDebit - totalCredit;
                }

                return balance;
            }
        }

        public ICollection<Transaction> DebitTransactions { get; set; }

        public ICollection<Transaction> CreditTransactions { get; set; }
    }
}
