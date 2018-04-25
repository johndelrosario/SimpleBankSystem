using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Data.Contexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleBankSystem.Data.Repositories
{
    public class TransactionRepository : IDisposable
    {
        private SimpleBankContext Context { get; set; }

        public TransactionRepository(SimpleBankContext context)
        {
            Context = context;
        }

        public async Task<TransactionResult> DoTransaction(TransactionEntry entry, int sleep = 0)
        {
            using (var trans = Context.Database.BeginTransaction())
            {
                try
                {
                    var debitUser = await Context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstOrDefaultAsync(u => u.Id == entry.DebitAccount);
                    var creditUser = await Context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstOrDefaultAsync(u => u.Id == entry.CreditAccount);

                    Thread.Sleep(sleep);

                    if ((entry.Type == TransactionType.Deposit && debitUser == null) ||
                       (entry.Type == TransactionType.Withdraw && creditUser == null) ||
                       (entry.Type == TransactionType.Transfer && (debitUser == null || creditUser == null)) ||
                       (entry.Type == TransactionType.Transfer && (debitUser.Id == creditUser.Id)))
                    {
                        return new TransactionResult(false, "Invalid transaction");
                    }

                    if (creditUser != null)
                    {
                        if (entry.Amount > creditUser.Balance)
                        {
                            return new TransactionResult(false, "Insufficient balance");
                        }
                    }

                    Context.Transactions.Add(new Transaction
                    {
                        Amount = entry.Amount,
                        DebitAccount = entry.DebitAccount,
                        CreditAccount = entry.CreditAccount,
                        DateCreated = DateTime.Now,
                        Remarks = entry.Remarks
                    });

                    await Context.SaveChangesAsync();

                    if (debitUser != null)
                    {
                        debitUser.Balance = debitUser.ComputedBalance;
                        Context.Entry(debitUser).State = EntityState.Modified;
                    }

                    if (creditUser != null)
                    {
                        creditUser.Balance = creditUser.ComputedBalance;
                        Context.Entry(creditUser).State = EntityState.Modified;
                    }

                    await Context.SaveChangesAsync();

                    trans.Commit();
                    return new TransactionResult(true, "Transaction success");
                }
                catch (DbUpdateConcurrencyException)
                {
                    trans.Rollback();
                    return new TransactionResult(false, "Concurrent transaction.");
                }
                catch (Exception e)
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

        public class TransactionEntry
        {
            public TransactionType Type { get; set; }

            public double Amount { get; set; }

            public string DebitAccount { get; set; }

            public string CreditAccount { get; set; }

            public string Remarks { get; set; }
        }

        public enum TransactionType
        {
            Withdraw,
            Deposit,
            Transfer
        }
    }
}
