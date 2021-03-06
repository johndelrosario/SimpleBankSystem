﻿using Microsoft.AspNetCore.Identity;
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
        private readonly string _transactionDepositSuccessMessage = "{0} deposited sucessfully into the account";
        private readonly string _transactionWithdrawSuccessMessage = "{0} withdrawn sucessfully from the account";
        private readonly string _transactionTransferSuccessMessage = "{0} transferred sucessfully to account {1}";

        private readonly string _transactionDepositRemark = "Deposit";
        private readonly string _transactionWithdrawRemark = "Withdraw";

        private readonly string _transactionInvalidAmountMessage = "Amount must be a positive number";
        private readonly string _transactionInsufficientBalanceMessage = "Insufficient balance";
        private readonly string _transactionTransferInvalidAccountMessage = "Invalid account number";
        private readonly string _transactionTransferInvalidSameAccountMessage = "Cannot transfer to own account.";

        public User()
        {
            CreatedDate = DateTime.Now;
            DebitTransactions = new List<Transaction>();
            CreditTransactions = new List<Transaction>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountNumber { get; set; }

        public string AccountName { get; set; }

        public DateTime CreatedDate { get; set; }

        [ConcurrencyCheck]
        public double Balance { get; set; }

        public bool Deposit(double amount, out string message, string remarks = "")
        {
            var isSuccess = false;

            if (amount <= 0)
            {
                message = _transactionInvalidAmountMessage;
            }
            else
            {
                DebitTransactions.Add(new Transaction
                {
                    DebitAccount = Id,
                    Amount = amount,
                    Remarks = !string.IsNullOrWhiteSpace(remarks) ? remarks : _transactionDepositRemark
                });

                Balance += amount;

                message = string.Format(_transactionDepositSuccessMessage, amount);
                isSuccess = true;
            }

            return isSuccess;
        }

        public bool Withdraw(double amount, out string message, string remarks = "")
        {
            var isSuccess = false;

            if (amount <= 0)
            {
                message = _transactionInvalidAmountMessage;
            }
            else if (amount > Balance)
            {
                message = _transactionInsufficientBalanceMessage;
            }
            else
            {
                CreditTransactions.Add(new Transaction
                {
                    CreditAccount = Id,
                    Amount = amount,
                    Remarks = !string.IsNullOrWhiteSpace(remarks) ? remarks : _transactionWithdrawRemark
                });

                Balance -= amount;

                message = string.Format(_transactionWithdrawSuccessMessage, amount);
                isSuccess = true;
            }

            return isSuccess;
        }

        public bool TransferToUser(double amount, User targetUser, string additionalRemarks, out string message)
        {
            var isSuccess = false;

            if (amount <= 0)
            {
                message = _transactionInvalidAmountMessage;
            }
            else if (targetUser == null || targetUser.Id == Id)
            {
                message = targetUser == null ? _transactionTransferInvalidAccountMessage : _transactionTransferInvalidSameAccountMessage;
            }
            else
            {
                var remarks = $"{(!string.IsNullOrWhiteSpace(additionalRemarks) ? $"{additionalRemarks}<br />" : string.Empty)}Transferred by {AccountName} to {targetUser.AccountName}";

                if (Withdraw(amount, out message, remarks) && targetUser.Deposit(amount, out message, remarks))
                {
                    message = string.Format(_transactionTransferSuccessMessage, amount, targetUser.AccountNumber);
                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        public List<Transaction> GetTransactions()
        {
            var transactions = DebitTransactions.Union(CreditTransactions)
                                                .OrderByDescending(tr => tr.DateCreated)
                                                .ToList();

            return transactions;
        }

        [ConcurrencyCheck]
        public virtual ICollection<Transaction> DebitTransactions { get; set; }

        [ConcurrencyCheck]
        public virtual ICollection<Transaction> CreditTransactions { get; set; }
    }
}
