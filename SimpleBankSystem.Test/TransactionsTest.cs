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
        private readonly double _baseBalance = 100;

        [TestMethod]
        public void ShouldGetSameInitialUserBalance()
        {
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public void ShouldAllowDepositPositiveAmountToUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.Deposit(amount, out message);

            Assert.IsTrue(isSuccess);
        }

        [TestMethod]
        public void ShouldNotAllowDepositNegativeAmountToUser()
        {
            var amount = -100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.Deposit(amount, out message);

            Assert.IsFalse(isSuccess);
        }

        [TestMethod]
        public void ShouldRetainBalanceOnDepositNegativeAmountToUser()
        {
            var amount = -100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.Deposit(amount, out message);

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public void ShouldUpdateBalanceOnDepositPositiveAmountToUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.Deposit(amount, out message);

            Assert.IsTrue(user.Balance == (_baseBalance + amount));
        }

        [TestMethod]
        public void ShouldAllowWithdrawOfPositiveAmountWithinBalanceFromUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.Withdraw(amount, out message);

            Assert.IsTrue(isSuccess);
        }

        [TestMethod]
        public void ShouldNotAllowWithdrawNegativeAmountFromUser()
        {
            var amount = -100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.Withdraw(amount, out message);

            Assert.IsFalse(isSuccess);
        }

        [TestMethod]
        public void ShouldUpdateBalanceOnPositiveWithdrawWithinBalanceFromUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.Withdraw(amount, out message);

            Assert.IsTrue(user.Balance == _baseBalance - amount);
        }

        [TestMethod]
        public void ShouldRetainBalanceOnWithdrawNegativeAmountFromUser()
        {
            var amount = -100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.Withdraw(amount, out message);

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public void ShouldNotAllowWithdrawMoreThanBalanceFromUser()
        {
            var amount = 200;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.Withdraw(amount, out message);

            Assert.IsFalse(isSuccess);
        }

        [TestMethod]
        public void ShouldRetainBalanceOnWithdrawMoreThanBalanceFromUser()
        {
            var amount = 200;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.Withdraw(amount, out message);

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public void ShouldAllowTransferOfPositiveAmountWithinBalanceToUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };
            var user2 = new User
            {
                AccountName = "Test2",
                AccountNumber = 2,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.TransferToUser(amount, user2, string.Empty, out message);

            Assert.IsTrue(isSuccess);
        }

        [TestMethod]
        public void ShouldNotAllowTransferOfPositiveAmountOfMoreThanBalanceOfUser()
        {
            var amount = 200;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };
            var user2 = new User
            {
                AccountName = "Test2",
                AccountNumber = 2,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.TransferToUser(amount, user2, string.Empty, out message);

            Assert.IsFalse(isSuccess);
        }

        [TestMethod]
        public void ShouldNotAllowTransferOfNegativeAmountToUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };
            var user2 = new User
            {
                AccountName = "Test2",
                AccountNumber = 2,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.TransferToUser(amount, user2, string.Empty, out message);

            Assert.IsTrue(isSuccess);
        }

        [TestMethod]
        public void ShouldReceiveAmountOnTransferOfPositiveAmountToUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };
            var user2 = new User
            {
                AccountName = "Test2",
                AccountNumber = 2,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.TransferToUser(amount, user2, string.Empty, out message);

            Assert.IsTrue(user2.Balance == _baseBalance + amount);
        }

        [TestMethod]
        public void ShouldUpdateBalanceOnTransferOfPositiveAmountFromUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };
            var user2 = new User
            {
                AccountName = "Test2",
                AccountNumber = 2,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.TransferToUser(amount, user2, string.Empty, out message);

            Assert.IsTrue(user.Balance == _baseBalance - amount);
        }

        [TestMethod]
        public void ShouldRetainBalanceOnTransferOfMoreThanBalanceOfUser()
        {
            var amount = 200;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };
            var user2 = new User
            {
                AccountName = "Test2",
                AccountNumber = 2,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.TransferToUser(amount, user2, string.Empty, out message);

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public void ShouldNotTransferToInvalidUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.TransferToUser(amount, null, string.Empty, out message);

            Assert.IsFalse(isSuccess);
        }

        [TestMethod]
        public void ShouldRetainBalanceOnTransferToInvalidUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.TransferToUser(amount, null, string.Empty, out message);

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public void ShouldNotTransferToSameUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            var isSuccess = user.TransferToUser(amount, user, string.Empty, out message);

            Assert.IsFalse(isSuccess);
        }

        [TestMethod]
        public void ShouldRetainBalanceOnTransferToSameUser()
        {
            var amount = 100;
            var user = new User
            {
                AccountName = "Test",
                AccountNumber = 1,
                Balance = _baseBalance
            };

            var message = string.Empty;
            user.TransferToUser(amount, user, string.Empty, out message);

            Assert.IsTrue(user.Balance == _baseBalance);
        }

        [TestMethod]
        public async Task ShouldCatchExceptionOnWithdrawSimultaneously()
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
        public async Task ShouldCatchExceptionOnTransferSimultaneously()
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

                        targetUser.Deposit(_baseBalance, out message);
                        await context.SaveChangesAsync();
                    }

                    trans.Commit();
                }
            }
        }
    }
}