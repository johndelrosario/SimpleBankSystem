using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SimpleBankSystem.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBankSystem.Test
{
    public class Setup
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public Setup()
        {
            var services = new ServiceCollection();

            services.AddDbContext<SimpleBankContext>(options =>
            {
                options.UseSqlServer(Data.Constants.TestConnectionString);
            }, ServiceLifetime.Transient);

            services
                .AddIdentity<Data.Identity.User, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 0;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddUserStore<Data.Identity.UserStore>()
                .AddRoleStore<Data.Identity.RoleStore>()
                .AddUserManager<Data.Identity.UserManager>()
                .AddDefaultTokenProviders();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient(typeof(Data.Repositories.TransactionRepository));

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
