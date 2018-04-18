using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleBankSystem.Data.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBankSystem.Data.Contexts
{
    public class SimpleBankContext : IdentityDbContext<User>
    {
        public SimpleBankContext(DbContextOptions<SimpleBankContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasIndex(b => b.AccountNumber).IsUnique(true);
        }

        public DbSet<Transaction> Transactions { get; set; }
    }
}
