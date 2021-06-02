using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GWCU_USSD_WebApp.Models;

namespace GWCU_USSD_WebApp.ViewModels
{
    public class UserRequestModel
    {
        public int UserRequestID { get; set; }
        public int UserBankID { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateTime { get; set; }
        public float Amount { get; set; }
        public string InterestRate { get; set; }
        public string PaybackPeriod { get; set; }
        public bool IsApproved { get; set; }
        public  IEnumerable<UserBank> UserBank { get; set; }
    }
}