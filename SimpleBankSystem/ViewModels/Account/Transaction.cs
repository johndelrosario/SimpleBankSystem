using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBankSystem.ViewModels.Account
{
    public class Transaction
    {
        public long? DebitAccountNumber { get; set; }

        public long? CreditAccountNumber { get; set; }

        public double Amount { get; set; }

        public string Remarks { get; set; }

        public DateTime DateCreated { get; set; }

        public static Transaction MapToDbTransaction(Data.Transaction dbTransaction)
        {
            var transaction = new Transaction
            {
                DebitAccountNumber = dbTransaction.DebitAccountUser?.AccountNumber,
                CreditAccountNumber = dbTransaction.CreditAccountUser?.AccountNumber,
                Amount = dbTransaction.Amount,
                Remarks = dbTransaction.Remarks,
                DateCreated = dbTransaction.DateCreated
            };

            return transaction;
        }
    }
}
