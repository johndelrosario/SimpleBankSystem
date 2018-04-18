using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBankSystem.Data.Contexts;
using SimpleBankSystem.Data.Identity;

namespace SimpleBankSystem.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(SimpleBankContext sbContext, UserManager sbUserManager)
            : base(sbContext, sbUserManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
