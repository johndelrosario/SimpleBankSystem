using SimpleBankSystem.Data.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBankSystem.ViewModels.Account
{
    public class Register
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(10)]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public User MapToUser()
        {
            var user = new User
            {
                UserName = UserName,
                AccountName = AccountName,
            };

            return user;
        }
    }
}
