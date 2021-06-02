using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GWCU_USSD_WebApp.Models
{
    public class UserBank
    {
        public int UserBankID { get; set; }
        public int UserID { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public virtual User User { get; set; }
    }
}