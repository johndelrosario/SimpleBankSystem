using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;
using SimpleBankSystem.Data.Repositories;
using SimpleBankSystem.ViewModels.Account;

namespace SimpleBankSystem.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private SignInManager<User> SignInManager { get; set; }
        private TransactionRepository TransactionRepository { get; set; }

        public AccountController(
            SimpleBankContext sbContext,
            UserManager sbUserManager,
            SignInManager<User> signInManager,
            TransactionRepository transactionRepository) : base(sbContext, sbUserManager)
        {
            SignInManager = signInManager;
            TransactionRepository = transactionRepository;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            var viewModel = new Register();

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Register viewModel, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = viewModel.MapToUser();
                var result = await UserManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, false);

                    if (string.IsNullOrWhiteSpace(returnUrl))
                    {
                        return RedirectToAction("index", "home");
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            var viewModel = new Login();
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Login viewModel, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult Deposit()
        {
            var viewModel = new Deposit();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(Deposit viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await TransactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                    {
                        Type = TransactionRepository.TransactionType.Deposit,
                        Amount = viewModel.Amount,
                        DebitAccount = CurrentUser.Id,
                        Remarks = "Deposit"
                    });

                    if (result.IsSuccess)
                    {
                        ModelState.Clear();

                        viewModel = new Deposit
                        {
                            IsSuccess = true,
                            Message = $"{viewModel.Amount} deposited sucessfully into the account",
                        };
                    }
                    else
                    {
                        viewModel.IsSuccess = false;
                        viewModel.Message = result.Message;
                    }
                }
                catch (Exception e)
                {
                    viewModel.IsSuccess = false;
                    viewModel.Message = e.Message;
                }
            }

            return View(viewModel);
        }

        public IActionResult Withdraw()
        {
            var viewModel = new Withdraw();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(Withdraw viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await TransactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                    {
                        Type = TransactionRepository.TransactionType.Withdraw,
                        Amount = viewModel.Amount,
                        CreditAccount = CurrentUser.Id,
                        Remarks = "Withdraw"
                    });

                    if (result.IsSuccess)
                    {
                        ModelState.Clear();

                        viewModel = new Withdraw
                        {
                            IsSuccess = true,
                            Message = $"{viewModel.Amount} withdrawn sucessfully from the account",
                        };
                    }
                    else
                    {
                        viewModel.IsSuccess = false;
                        viewModel.Message = result.Message;
                    }
                }
                catch (Exception e)
                {
                    viewModel.IsSuccess = false;
                    viewModel.Message = e.Message;
                }
            }

            return View(viewModel);
        }

        public IActionResult Transfer()
        {
            var viewModel = new Transfer();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(Transfer viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var targetUser = Context.Users.FirstOrDefault(us => us.AccountNumber == viewModel.AccountNumber);

                    if (targetUser != null && targetUser.Id != CurrentUser.Id)
                    {
                        viewModel.Remarks += $"{(!string.IsNullOrWhiteSpace(viewModel.Remarks) ? "<br />" : string.Empty)}Transferred by {CurrentUser.AccountName} to {targetUser.AccountName}";
                        var result = await TransactionRepository.DoTransaction(new TransactionRepository.TransactionEntry
                        {
                            Type = TransactionRepository.TransactionType.Transfer,
                            Amount = viewModel.Amount,
                            DebitAccount = targetUser.Id,
                            CreditAccount = CurrentUser.Id,
                            Remarks = viewModel.Remarks
                        });

                        if (result.IsSuccess)
                        {
                            ModelState.Clear();

                            viewModel = new Transfer
                            {
                                IsSuccess = true,
                                Message = $"{viewModel.Amount} transferred sucessfully to account {viewModel.AccountNumber}",
                            };
                        }
                        else
                        {
                            viewModel.IsSuccess = false;
                            viewModel.Message = result.Message;
                        }
                    }
                    else
                    {
                        viewModel.IsSuccess = false;
                        viewModel.Message = targetUser == null ? "Invalid account number" : "Cannot transfer to own account.";
                    }
                }
                catch (Exception e)
                {
                    viewModel.IsSuccess = false;
                    viewModel.Message = e.Message;
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Transactions()
        {
            var transactions = await TransactionRepository.GetTransactions(CurrentUser.Id);

            var viewModel = transactions.Select(tr => Transaction.MapToDbTransaction(tr))
                                        .ToList();

            return View(viewModel);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}