using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GWCU_USSD_WebApp.Models
{
    public class UserTransaction
    {
        public int UserTransactionID { get; set; }
        public DateTime DateTime { get; set; }
        public string ReferenceNumber { get; set; }
        public float Debit { get; set; }
        public float Credit { get; set; }
        public float Balance { get; set; }
        public float AmountToBePaid { get; set; }
        public float AmountDisbursed { get; set; }
        public float AmountEarned { get; set; }
        public bool IsReversed { get; set; }
        public bool? IsLoanTopUp { get; set; }
        public int UserRequestID { get; set; }
        public virtual UserRequest UserRequest { get; set; }
    }
}