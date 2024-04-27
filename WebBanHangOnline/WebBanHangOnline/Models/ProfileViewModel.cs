using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "FullName is required.")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public string Phone { get; set; }
        [Required(ErrorMessage = "Gender is required.")]
        public bool Gender { get; set; }
        public float WalletPoint { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }

        public string RePassword { get; set; }

    }
}