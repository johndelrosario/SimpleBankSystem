using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleBankSystem.Data.Repositories
{
    public class TransactionRepository : IDisposable
    {
        private static readonly SemaphoreSlim _transLock = new SemaphoreSlim(1, 1);

        private SimpleBankContext Context { get; set; }

        public TransactionRepository(SimpleBankContext context)
        {
            Context = context;
        }

        public async Task<TransactionResult> DoTransaction(TransactionType type, double amount, string debitAccount, string creditAccount, string remarks = null, int sleep = 0)
        {
            try
            {
                await (_transLock.WaitAsync());
                using (var trans = Context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var debitUser = await Context.Users
                                                        .Include(u => u.DebitTransactions)
                                                        .Include(u => u.CreditTransactions)
                                                        .FirstOrDefaultAsync(u => u.Id == debitAccount);
                        var creditUser = await Context.Users
                                                        .Include(u => u.DebitTransactions)
                                                        .Include(u => u.CreditTransactions)
                                                        .FirstOrDefaultAsync(u => u.Id == creditAccount);

                        if ((type == TransactionType.Deposit && debitUser == null) ||
                           (type == TransactionType.Withdraw && creditUser == null) ||
                           (type == TransactionType.Transfer && (debitUser == null || creditUser == null)) ||
                           (type == TransactionType.Transfer && (debitUser.Id == creditUser.Id)))
                        {
                            return new TransactionResult(false, "Invalid transaction");
                        }

                        if (creditUser != null)
                        {
                            Thread.Sleep(sleep);
                            if (amount > creditUser.Balance)
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
            finally
            {
                _transLock.Release();
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

        public void Dispose()
        {
            Context.Dispose();
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
