using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBankSystem.Data.Identity
{
    public class UserStore : UserStore<User>
    {
        public UserStore(SimpleBankContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }
    }
}
