using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GWCU_USSD_WebApp.Models
{
    public class User
    {
        public User()
        {
            UserRequests = UserRequests == null ? new List<UserRequest>() : UserRequests;
            UserTransactions = UserTransactions == null ? new List<UserTransaction>() : UserTransactions;
        }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAccountEnabled { get; set; }
        public float CreditLimit { get; set; }
        public float CreditUtilized { get; set; }
        public string ipposi { get; set; }
        public int yearDob { get; set; }
        public DateTime dob { get; set; } = DateTime.Now;

        [NotMapped]
        public float CreditAvailable { get { return CreditLimit - CreditUtilized; } }
        public string InterestRates { get; set; }
        public string PaybackPeriods { get; set; }
        public virtual ICollection<UserRequest> UserRequests { get; set; }
        public virtual ICollection<UserBank> UserBanks { get; set; }
        public virtual ICollection<UserTransaction> UserTransactions { get; set; }

        public string Institution { get; set; }
    }
}