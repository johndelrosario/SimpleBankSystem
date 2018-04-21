using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;
using SimpleBankSystem.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBankSystem.Test
{
    [TestClass]
    public class TransactionsTest
    {
        private double baseBalance = 100;

        [TestMethod]
        public async Task CanGetBalance()
        {
            var setup = new Setup();
            await CreateUsers(setup);

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            {
                {
                    var targetUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstAsync();

                    Assert.IsTrue(targetUser.Balance == baseBalance);
                }
            }
        }

        [TestMethod]
        public async Task CanDepositToUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Deposit, amount, targetUser.Id, null, "Deposit");

                Assert.IsTrue(result.IsSuccess);
            }
        }

        [TestMethod]
        public async Task DepositToUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                {
                    var targetUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstAsync();
                    await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Deposit, amount, targetUser.Id, null, "Deposit");
                }

                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                {
                    var targetUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstAsync();
                    Assert.IsTrue(targetUser.Balance == (baseBalance + amount));
                }
            }
        }

        [TestMethod]
        public async Task CanWithdrawToUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw");

                Assert.IsTrue(result.IsSuccess);
            }
        }

        [TestMethod]
        public async Task WithdrawToUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                {
                    var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                    await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw");
                }

                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                {
                    var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                    Assert.IsTrue(targetUser.Balance == (baseBalance - amount));
                }
            }
        }

        [TestMethod]
        public async Task CannotWithdrawMoreThanBalance()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 200;

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
            {
                var targetUser = await context.Users
                                            .Include(u => u.DebitTransactions)
                                            .Include(u => u.CreditTransactions)
                                            .FirstAsync();
                var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw");

                Assert.IsFalse(result.IsSuccess);
            }
        }

        [TestMethod]
        public async Task CanTransferToUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var transferUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .LastAsync();
                var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, transferUser.Id, targetUser.Id, "Transfer");

                Assert.IsTrue(result.IsSuccess);
            }
        }

        [TestMethod]
        public async Task CannotTransferToInvalidUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, "invalidtestuser", targetUser.Id, "Transfer");

                Assert.IsFalse(result.IsSuccess);
            }
        }

        [TestMethod]
        public async Task CannotTransferToSameUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, targetUser.Id, targetUser.Id, "Transfer");

                Assert.IsFalse(result.IsSuccess);
            }
        }

        [TestMethod]
        public async Task TransferToUser()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 10;

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                {
                    var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                    var transferUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .LastAsync();

                    await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, transferUser.Id, targetUser.Id, "Transfer");
                }

                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                {
                    var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                    var transferUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .LastAsync();

                    Assert.IsTrue(targetUser.Balance == (baseBalance - amount));
                    Assert.IsTrue(transferUser.Balance == (baseBalance + amount));
                }
            }
        }

        [TestMethod]
        public async Task WithdrawSimultaneously()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 100;

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                Task[] taskArr =
                {
                    Task.Factory.StartNew(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();

                            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw", 5000);
                        }
                    }),
                    Task.Factory.StartNew(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();

                            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw", 0);
                        }
                    }),
                };

                await Task.WhenAll(taskArr);

                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                {
                    var targetUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstAsync();

                    Assert.IsTrue(targetUser.Balance >= 0);
                }
            }
        }

        [TestMethod]
        public async Task TransferSimultaneously()
        {
            var setup = new Setup();
            await CreateUsers(setup);
            var amount = 100;

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                Task[] taskArr =
                {
                    Task.Factory.StartNew(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();
                            var transferUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .LastAsync();

                            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, transferUser.Id, targetUser.Id, "Transfer", 5000);
                        }
                    }),
                    Task.Factory.StartNew(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();

                            var transferUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .LastAsync();

                            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, transferUser.Id, targetUser.Id, "Transfer", 0);
                        }
                    }),
                };

                await Task.WhenAll(taskArr);

                using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                {
                    var targetUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstAsync();

                    var transferUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .LastAsync();

                    Assert.IsTrue(targetUser.Balance >= 0);
                    Assert.IsTrue(transferUser.Balance == (baseBalance + amount));
                }
            }
        }

        public async Task CreateUsers(Setup setup)
        {
            var userList = new List<User>
            {
                new User
                {
                    UserName = "Test",
                    AccountName = "Test",
                    AccountNumber = 1,
                    CreatedDate = DateTime.Now
                },
                new User
                {
                    UserName = "Test2",
                    AccountName = "Test2",
                    AccountNumber = 1,
                    CreatedDate = DateTime.Now
                }
            };

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetService<UserManager>();
                var context = scope.ServiceProvider.GetService<SimpleBankContext>();

                foreach (var user in userList)
                {
                    await userManager.CreateAsync(user, "password");

                    context.Transactions.Add(new Data.Transaction
                    {
                        Amount = baseBalance,
                        DebitAccount = user.Id
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
