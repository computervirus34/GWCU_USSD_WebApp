using GWCU_USSD_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GWCU_USSD_WebApp
{
    public class AppSettings
    {
        public static List<AdminUserViewModel> Admins
        {
            get
            {
                return new List<AdminUserViewModel>
                {
                    new AdminUserViewModel
                    {
                        Username = "GWCU",
                        Password = "FuZoRL77",
                        SecretKey = "MjsnbC6j",
                        UserRole = "Admin"
                    },
                    new AdminUserViewModel
                    {
                        Username = "abcd",
                        Password = "abcd",
                        SecretKey = "abcd",
                        UserRole = "Sub-admin"
                    }
                };
            }
        }


    }
}