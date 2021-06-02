using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GWCU_USSD_WebApp.Models;

namespace GWCU_USSD_WebApp.ViewModels
{
    public class SmsViewModel
    {
        public List<string> Numbers { get; set; }
        public string Message { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}