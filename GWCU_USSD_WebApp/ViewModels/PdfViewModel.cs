using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GWCU_USSD_WebApp.ViewModels
{
    public class PdfViewModel
    {
        public int Id { get; set; }
        public string PreparedFor { get; set; }
        public int EmployeeId { get; set; }
        public int transNo { get; set; }
        public string Institution { get; set; }
        public string InterestRate { get; set; }
        public float TotalPayment { get; set; }
        public string LoadTerm { get; set; }
        public double PayPerYear { get; set; }
        public double LoanAMount { get; set; }
        public float AmountToPay { get; set; }
        public string ReferenceNo { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public double Balance { get; set; }
        public double AMountDisbursed { get; set; }
        public DateTime DateTime { get; set; }
        public string RequestedOn { get; set; }



    }
}