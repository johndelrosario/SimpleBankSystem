using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBankSystem.ViewModels.Account
{
    public class Deposit
    {
        [Required]
        [Range(1, double.MaxValue)]
        public double Amount { get; set; }

        public bool? IsSuccess { get; set; }

        public string Message { get; set; }
    }
}
