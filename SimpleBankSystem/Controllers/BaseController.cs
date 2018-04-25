using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Attributes;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;

namespace SimpleBankSystem.Controllers
{
    [UserLoader]
    public abstract class BaseController : Controller
    {
        protected SimpleBankContext Context { get; private set; }

        protected UserManager UserManager { get; private set; }

        protected User CurrentUser { get; set; }

        public BaseController(
            SimpleBankContext context, 
            UserManager userManager)
        {
            Context = context;
            UserManager = userManager;
        }

        public void GetCurrentUser()
        {
            var id = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            CurrentUser = Context.Users
                                    .Include(u => u.DebitTransactions)
                                    .Include(u => u.CreditTransactions)
                                    .FirstOrDefault(u => u.Id == id);
        }
    }
}