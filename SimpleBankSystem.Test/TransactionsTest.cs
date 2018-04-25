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
                var result = await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                {
                    Type = TransactionRepository.TransactionType.Deposit,
                    Amount = amount,
                    DebitAccount = targetUser.Id,
                    Remarks = "Deposit"
                });

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
                    await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                    {
                        Type = TransactionRepository.TransactionType.Deposit,
                        Amount = amount,
                        DebitAccount = targetUser.Id,
                        Remarks = "Deposit"
                    });
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
                var result = await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                {
                    Type = TransactionRepository.TransactionType.Withdraw,
                    Amount = amount,
                    CreditAccount = targetUser.Id,
                    Remarks = "Withdraw"
                });

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
                    await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                    {
                        Type = TransactionRepository.TransactionType.Withdraw,
                        Amount = amount,
                        CreditAccount = targetUser.Id,
                        Remarks = "Withdraw"
                    });
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
                var result = await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                {
                    Type = TransactionRepository.TransactionType.Withdraw,
                    Amount = amount,
                    CreditAccount = targetUser.Id,
                    Remarks = "Withdraw"
                });

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
                var result = await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                {
                    Type = TransactionRepository.TransactionType.Transfer,
                    Amount = amount,
                    DebitAccount = transferUser.Id,
                    CreditAccount = targetUser.Id,
                    Remarks = "Transfer"
                });

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
                var result = await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                {
                    Type = TransactionRepository.TransactionType.Transfer,
                    Amount = amount,
                    DebitAccount = "invalidtestuser",
                    CreditAccount = targetUser.Id,
                    Remarks = "Transfer"
                });

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
                var result = await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                {
                    Type = TransactionRepository.TransactionType.Transfer,
                    Amount = amount,
                    DebitAccount = targetUser.Id,
                    CreditAccount = targetUser.Id,
                    Remarks = "Transfer"
                });

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

                    await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                    {
                        Type = TransactionRepository.TransactionType.Transfer,
                        Amount = amount,
                        DebitAccount = transferUser.Id,
                        CreditAccount = targetUser.Id,
                        Remarks = "Transfer"
                    });
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
                    Task.Run(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();

                            await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                            {
                                Type = TransactionRepository.TransactionType.Withdraw,
                                Amount = amount,
                                CreditAccount = targetUser.Id,
                                Remarks = "Withdraw"
                            }, 3000);
                        }
                    }),
                    Task.Run(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();

                            await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                            {
                                Type = TransactionRepository.TransactionType.Withdraw,
                                Amount = amount,
                                CreditAccount = targetUser.Id,
                                Remarks = "Withdraw"
                            }, 0);
                        }
                    }),
                    Task.Run(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        using (var transactionRepository = scope.ServiceProvider.GetService<TransactionRepository>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();

                            await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                            {
                                Type = TransactionRepository.TransactionType.Withdraw,
                                Amount = amount,
                                CreditAccount = targetUser.Id,
                                Remarks = "Withdraw"
                            }, 1500);
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

                    Assert.IsTrue(targetUser.Balance == 0, $"Balance is {targetUser.Balance}");
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
                    Task.Run(async () =>
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

                            await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                            {
                                Type = TransactionRepository.TransactionType.Transfer,
                                Amount = amount,
                                DebitAccount = transferUser.Id,
                                CreditAccount = targetUser.Id,
                                Remarks = "Transfer"
                            }, 3000);
                        }
                    }),
                    Task.Run(async () =>
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

                            await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                            {
                                Type = TransactionRepository.TransactionType.Transfer,
                                Amount = amount,
                                DebitAccount = transferUser.Id,
                                CreditAccount = targetUser.Id,
                                Remarks = "Transfer"
                            }, 0);
                        }
                    }),
                    Task.Run(async () =>
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

                            await transactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                            {
                                Type = TransactionRepository.TransactionType.Transfer,
                                Amount = amount,
                                DebitAccount = transferUser.Id,
                                CreditAccount = targetUser.Id,
                                Remarks = "Transfer"
                            }, 1500);
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

                    Assert.IsTrue(targetUser.Balance == 0, $"Transferer balance is {targetUser.Balance}");
                    Assert.IsTrue(transferUser.Balance == (baseBalance + amount), $"Transferee balance is {transferUser.Balance}");
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
                    CreatedDate = DateTime.Now,
                    Balance = baseBalance
                },
                new User
                {
                    UserName = "Test2",
                    AccountName = "Test2",
                    CreatedDate = DateTime.Now,
                    Balance = baseBalance
                }
            };

            using (var scope = setup.ServiceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetService<UserManager>();
                var context = scope.ServiceProvider.GetService<SimpleBankContext>();
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

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
