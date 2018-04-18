using Microsoft.AspNetCore.Identity;
using SimpleBankSystem.Data.Contexts;
using System;
using System.Collections.Generic;
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

        public double GetBalance(SimpleBankContext context)
        {
            var balance = 0d;

            foreach (var trans in context.Transactions.Where(tr => tr.CreditAccount == Id || tr.DebitAccount == Id))
            {
                if (trans.DebitAccount == Id)
                {
                    balance += trans.Amount;
                }
                else if (trans.CreditAccount == Id)
                {
                    balance -= trans.Amount;
                }
            }

            return balance;
        }
    }
}
