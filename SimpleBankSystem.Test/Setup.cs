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
        private readonly IServiceProvider _serviceProvider;

        public Setup()
        {
            var services = new ServiceCollection();

            services.AddDbContext<SimpleBankContext>(options =>
            {
                options.UseInMemoryDatabase(Data.Constants.DatabaseName);
                options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

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
            services.AddScoped(typeof(Data.Repositories.TransactionRepository));

            _serviceProvider = services.BuildServiceProvider();
        }

        public T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
