using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace GWCU_USSD_WebApp.Models
{
    public class UserRequest
    {
        public int UserRequestID { get; set; }
        public int UserBankID { get; set; }
        public DateTime DateTime { get; set; }
        public float Amount { get; set; }
        public string InterestRate { get; set; }
        public string PaybackPeriod { get; set; }
        public bool IsApproved { get; set; }
        [Required]
        public virtual UserBank UserBank { get; set; }
    }
}