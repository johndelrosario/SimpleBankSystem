using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBankSystem.ViewModels.Account
{
    public class Transfer
    {
        [Required]
        [Display(Name = "Account Number")]
        public long AccountNumber { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public double Amount { get; set; }

        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }

        public bool? IsSuccess { get; set; }

        public string Message { get; set; }
    }
}
