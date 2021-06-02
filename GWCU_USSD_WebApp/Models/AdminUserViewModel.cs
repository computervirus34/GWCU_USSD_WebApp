using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GWCU_USSD_WebApp.Models
{
    public class AdminUserViewModel
    {
        [Required(ErrorMessage ="Username field is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password field is required")]
        public string Password { get; set; }
        [Display(Name ="Secret Key")]
        [Required(ErrorMessage = "SecretKey field is required")]
        public string SecretKey { get; set; }
    }
}