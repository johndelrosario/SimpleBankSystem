using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public async void GetCurrentUser()
        {
            CurrentUser = await UserManager.GetUserAsync(HttpContext.User);
        }
    }
}