using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;
using SimpleBankSystem.ViewModels.Account;

namespace SimpleBankSystem.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private SignInManager<User> SignInManager { get; set; }

        public AccountController(
            SimpleBankContext sbContext,
            UserManager sbUserManager,
            SignInManager<User> signInManager) : base(sbContext, sbUserManager)
        {
            SignInManager = signInManager;
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
                try
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
                catch (DbUpdateException e)
                {
                    if (e.InnerException != null && e.InnerException is System.Data.SqlClient.SqlException)
                    {
                        var ex = e.InnerException as System.Data.SqlClient.SqlException;
                        ModelState.AddModelError("", "Account name already taken");
                    }
                    else
                    {
                        ModelState.AddModelError("", e.Message);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
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
                    var message = string.Empty;
                    if (CurrentUser.Deposit(viewModel.Amount, out message))
                    {
                        Context.Entry(CurrentUser).State = EntityState.Modified;
                        await Context.SaveChangesAsync();

                        ModelState.Clear();
                        viewModel = new Deposit
                        {
                            IsSuccess = true,
                            Message = message,
                        };
                    }
                    else
                    {
                        viewModel.IsSuccess = false;
                        viewModel.Message = message;
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
                    var message = string.Empty;
                    if (CurrentUser.Withdraw(viewModel.Amount, out message))
                    {
                        Context.Entry(CurrentUser).State = EntityState.Modified;
                        await Context.SaveChangesAsync();

                        ModelState.Clear();
                        viewModel = new Withdraw
                        {
                            IsSuccess = true,
                            Message = message,
                        };
                    }
                    else
                    {
                        viewModel.IsSuccess = false;
                        viewModel.Message = message;
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    viewModel.IsSuccess = false;
                    viewModel.Message = "Concurrent transaction detected. Please refresh the page and try again.";
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
                    var message = string.Empty;
                    var targetUser = Context.Users
                                            .Include(us => us.DebitTransactions)
                                            .Include(us => us.CreditTransactions)
                                            .FirstOrDefault(us => us.AccountNumber == viewModel.AccountNumber);

                    if (CurrentUser.TransferToUser(viewModel.Amount, targetUser, viewModel.Remarks, out message))
                    {
                        Context.Entry(CurrentUser).State = EntityState.Modified;
                        Context.Entry(targetUser).State = EntityState.Modified;
                        await Context.SaveChangesAsync();

                        ModelState.Clear();
                        viewModel = new Transfer
                        {
                            IsSuccess = true,
                            Message = message,
                        };
                    }
                    else
                    {
                        viewModel.IsSuccess = false;
                        viewModel.Message = message;
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    viewModel.IsSuccess = false;
                    viewModel.Message = "Concurrent transaction detected. Please refresh the page and try again.";
                }
                catch (Exception e)
                {
                    viewModel.IsSuccess = false;
                    viewModel.Message = e.Message;
                }
            }

            return View(viewModel);
        }

        public IActionResult Transactions()
        {
            var transactions = CurrentUser.GetTransactions();

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