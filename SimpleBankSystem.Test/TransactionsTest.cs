using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;
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
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();

                Assert.IsTrue(targetUser.Balance == baseBalance);
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
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var message = string.Empty;
                var isSuccess = targetUser.Deposit(amount, out message);

                Assert.IsTrue(isSuccess);
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
                {
                    var targetUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .FirstAsync();
                    var message = string.Empty;
                    var isSuccess = targetUser.Deposit(amount, out message);

                    await context.SaveChangesAsync();
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
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var message = string.Empty;
                var isSuccess = targetUser.Withdraw(amount, out message);

                Assert.IsTrue(isSuccess);
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
                {
                    var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                    var message = string.Empty;
                    targetUser.Withdraw(amount, out message);

                    await context.SaveChangesAsync();
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
            {
                var targetUser = await context.Users
                                            .Include(u => u.DebitTransactions)
                                            .Include(u => u.CreditTransactions)
                                            .FirstAsync();
                var message = string.Empty;
                var isSuccess = targetUser.Withdraw(amount, out message);

                Assert.IsFalse(isSuccess);
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
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var transferUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .LastAsync();
                var message = string.Empty;
                var isSuccess = targetUser.TransferToUser(amount, transferUser, string.Empty, out message);

                Assert.IsTrue(isSuccess);
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
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var message = string.Empty;
                var isSuccess = targetUser.TransferToUser(amount, null, string.Empty, out message);

                await context.SaveChangesAsync();

                Assert.IsFalse(isSuccess);
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
            {
                var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                var message = string.Empty;
                var isSuccess = targetUser.TransferToUser(amount, targetUser, string.Empty, out message);

                await context.SaveChangesAsync();

                Assert.IsFalse(isSuccess);
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
                {
                    var targetUser = await context.Users
                                                .Include(u => u.DebitTransactions)
                                                .Include(u => u.CreditTransactions)
                                                .FirstAsync();
                    var transferUser = await context.Users
                                                    .Include(u => u.DebitTransactions)
                                                    .Include(u => u.CreditTransactions)
                                                    .LastAsync();
                    var message = string.Empty;
                    var isSuccess = targetUser.TransferToUser(amount, transferUser, string.Empty, out message);

                    await context.SaveChangesAsync();
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
                var taskList = new List<Task>();
                for (var count = 1; count <= 5; count++)
                {
                    taskList.Add(Task.Run(async () =>
                    {
                        using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
                        {
                            var targetUser = await context.Users
                                                            .Include(u => u.DebitTransactions)
                                                            .Include(u => u.CreditTransactions)
                                                            .FirstAsync();
                            var message = string.Empty;
                            var isSuccess = targetUser.Withdraw(amount, out message);

                            await context.SaveChangesAsync();
                        }
                    }));
                }

                await Assert.ThrowsExceptionAsync<DbUpdateConcurrencyException>(() =>
                {
                    return Task.WhenAll(taskList);
                });
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
                var taskList = new List<Task>();
                for (var count = 1; count <= 5; count++)
                {
                    taskList.Add(Task.Run(async () =>
                    {
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
                            var message = string.Empty;
                            var isSuccess = targetUser.TransferToUser(amount, transferUser, string.Empty, out message);

                            await context.SaveChangesAsync();
                        }
                    }));
                }

                await Assert.ThrowsExceptionAsync<DbUpdateConcurrencyException>(() =>
                {
                    return Task.WhenAll(taskList);
                });
            }
        }

        public async Task CreateUsers(Setup setup)
        {
            var userList = new List<User>
            {
                new User
                {
                    UserName = "Test",
                    AccountName = "Test"
                },
                new User
                {
                    UserName = "Test2",
                    AccountName = "Test2"
                }
            };

            using (var scope = setup.ServiceProvider.CreateScope())
            using (var userManager = scope.ServiceProvider.GetService<UserManager>())
            using (var context = scope.ServiceProvider.GetService<SimpleBankContext>())
            {
                var message = string.Empty;
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

                using (var trans = await context.Database.BeginTransactionAsync())
                {
                    foreach (var user in userList)
                    {
                        await userManager.CreateAsync(user, "password");
                        var targetUser = await context.Users
                                                        .Include(us => us.DebitTransactions)
                                                        .Include(us => us.CreditTransactions)
                                                        .FirstOrDefaultAsync(us => us.Id == user.Id);

                        targetUser.Deposit(baseBalance, out message);
                        await context.SaveChangesAsync();
                    }

                    trans.Commit();
                }
            }
        }
    }
}