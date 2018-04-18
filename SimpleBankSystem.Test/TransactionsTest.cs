using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            var context = setup.GetService<SimpleBankContext>();
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var userBalance = targetUser.GetBalance(context);
            Assert.IsTrue(userBalance == baseBalance);
        }

        [TestMethod]
        public async Task CanDepositToUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Deposit, amount, targetUser.Id, null, "Deposit");

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task DepositToUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Deposit, amount, targetUser.Id, null, "Deposit");
            var userBalance = targetUser.GetBalance(context);

            Assert.IsTrue(userBalance == (baseBalance + amount));
        }

        [TestMethod]
        public async Task CanWithdrawToUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw");

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task WithdrawToUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw");
            var userBalance = targetUser.GetBalance(context);

            Assert.IsTrue(userBalance == (baseBalance - amount));
        }

        [TestMethod]
        public async Task CannotWithdrawMoreThanBalance()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 200;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Withdraw, amount, null, targetUser.Id, "Withdraw");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public async Task CanTransferToUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var transferUser = await context.Users.LastAsync();
            var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, transferUser.Id, targetUser.Id, "Transfer");

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public async Task CannotTransferToInvalidUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, "invalidtestuser", targetUser.Id, "Transfer");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public async Task CannotTransferToSameUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var result = await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, targetUser.Id, targetUser.Id, "Transfer");

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public async Task TransferToUser()
        {
            var setup = new Setup();
            var context = setup.GetService<SimpleBankContext>();
            var transactionRepository = setup.GetService<TransactionRepository>();
            var amount = 10;
            await CreateUsers(setup);

            var targetUser = await context.Users.FirstAsync();
            var transferUser = await context.Users.LastAsync();
            await transactionRepository.DoTransaction(TransactionRepository.TransactionType.Transfer, amount, transferUser.Id, targetUser.Id, "Transfer");

            var targetUserBalance = targetUser.GetBalance(context);
            var transferUserBalance = transferUser.GetBalance(context);

            Assert.IsTrue(targetUserBalance == (baseBalance - amount));
            Assert.IsTrue(transferUserBalance == (baseBalance + amount));
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

            var userManager = setup.GetService<UserManager>();
            var context = setup.GetService<SimpleBankContext>();

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
