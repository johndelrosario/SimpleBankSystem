using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankSystem.Data.Repositories
{
    public class TransactionRepository
    {
        private SimpleBankContext Context { get; set; }

        public TransactionRepository(SimpleBankContext context)
        {
            Context = context;
        }

        public async Task<TransactionResult> DoTransaction(TransactionType type, double amount, string debitAccount, string creditAccount, string remarks = null)
        {
            using (var trans = await Context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var debitUser = await Context.Users.FirstOrDefaultAsync(u => u.Id == debitAccount);
                    var creditUser = await Context.Users.FirstOrDefaultAsync(u => u.Id == creditAccount);

                    if((type == TransactionType.Deposit && debitUser == null) ||
                       (type == TransactionType.Withdraw && creditUser == null) ||
                       (type == TransactionType.Transfer && (debitUser == null || creditUser == null)) ||
                       (type == TransactionType.Transfer && (debitUser.Id == creditUser.Id)))
                    {
                        return new TransactionResult(false, "Invalid transaction");
                    }

                    if (creditUser != null)
                    {
                        var balance = creditUser.GetBalance(Context);

                        if (amount > balance)
                        {
                            return new TransactionResult(false, "Insufficient balance");
                        }
                    }

                    Context.Transactions.Add(new Transaction
                    {
                        Amount = amount,
                        DebitAccount = debitAccount,
                        CreditAccount = creditAccount,
                        DateCreated = DateTime.Now,
                        Remarks = remarks
                    });

                    await Context.SaveChangesAsync();
                    trans.Commit();

                    return new TransactionResult(true, "Transaction success");
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public async Task<List<Transaction>> GetTransactions(string userId)
        {
            var transactions = await Context.Transactions
                                                .Include(tr => tr.DebitAccountUser)
                                                .Include(tr => tr.CreditAccountUser)
                                                .Where(tr => tr.CreditAccount == userId || tr.DebitAccount == userId)
                                                .OrderByDescending(tr => tr.DateCreated)
                                                .ToListAsync();

            return transactions;
        }

        public class TransactionResult
        {
            public TransactionResult(bool isSuccess, string message)
            {
                IsSuccess = isSuccess;
                Message = message;
            }

            public string Message { get; set; }

            public bool IsSuccess { get; set; }
        }

        public enum TransactionType
        {
            Withdraw,
            Deposit,
            Transfer
        }
    }
}
