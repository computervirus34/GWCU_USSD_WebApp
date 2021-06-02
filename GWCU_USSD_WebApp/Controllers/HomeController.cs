using GWCU_USSD_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Configuration;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AfricasTalkingCS;
using GWCU_USSD_WebApp.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotativa;
using GWCU_USSD_WebApp.Migrations;
using GWCU_USSD_WebApp.Utility;
using System.Data.Entity.Validation;
using System.Web.Services;

namespace GWCU_USSD_WebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ApplicationDbContext db = ApplicationDbContext.Create();

       // private static string apikey = "sandbox";
        private static string apikey = "935289b65f78256a99f2aa69ab12098cbba9f0d4fba3275c179f0f7c374b1034";

        //private static string username = "gwcu";
        private static string username = "sandbox";

        private static AfricasTalkingGateway _atGWInstance = new AfricasTalkingGateway(username, apikey);
        public ActionResult Index()
        {
            //SendEmails("hanzalaphool@gmail.com", "hello","hello");
            return View();
        }

        #region Users
        public ActionResult Users()
        {
            return View();
        }

        public ActionResult UserForm(int UserID = 0)
        {
            if (TempData["UsernameExists"] != null)
                ViewBag.UsernameExists = true;

            if (UserID != 0)
            {
                var user = db.AccountUsers.SingleOrDefault(d => d.UserID == UserID);
                if (user != null) return View(user);
            }

            var new_user = new User();
            new_user.UserBanks = new List<UserBank>
                {
                    new UserBank { BankName = "", AccountNumber = "" },
                    new UserBank { BankName = "", AccountNumber = "" }
                };

            return View(new_user);
        }

        public ActionResult SaveUser(User model, FormCollection formData)
        {
            if (ModelState.IsValid)
            {
                DateTime current = DateTime.Now;
                model.yearDob = Years(model.dob, current);

                if (model.UserID == 0)
                {
                    //if (db.AccountUsers.SingleOrDefault(d => d.Username == model.Username) != null)
                    //{
                    //    TempData["UsernameExists"] = true;
                    //    return Redirect("UserForm");
                    //}
                    var newPassword = formData["Password"];
                    if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrWhiteSpace(newPassword))
                        model.PasswordHash = CreateMD5(newPassword);
                    else
                        model.PasswordHash = CreateMD5(model.PasswordHash);

                    foreach (var bank in model.UserBanks)
                    {
                        db.Entry(bank).State = System.Data.Entity.EntityState.Added;
                    }

                    model.IsAccountEnabled = true;
                    db.AccountUsers.Add(model);
                    db.SaveChanges();
                }
                else
                {
                    var newPassword = formData["Password"];
                    if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrWhiteSpace(newPassword))
                        model.PasswordHash = CreateMD5(newPassword);

                    foreach (var bank in model.UserBanks)
                    {
                        db.Entry(bank).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                return Redirect("Users");
            }
            return Redirect("UserForm");
        }

        public ActionResult GetUsers()
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Users data    
                var user_data = (from user in db.AccountUsers
                                 select user);

                //Sorting  
                if (string.IsNullOrEmpty(sortColumn)) sortColumn = "UserID";
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    user_data = user_data.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    user_data = user_data.Where(m => m.Username.Contains(searchValue) || m.FirstName.Contains(searchValue) || m.LastName.Contains(searchValue));
                }

                //total number of rows count     
                recordsTotal = user_data.Count();
                //Paging     
                var data = user_data.Skip(skip).Take(pageSize).ToList().Select(d => new
                {
                    UserID = d.UserID,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    CreditLimit = d.CreditLimit,
                    CreditUtilized = d.CreditUtilized,
                    CreditAvailable = d.CreditAvailable,
                    InterestRates = $"{d.InterestRates} %",
                    PaybackPeriods = $"{d.PaybackPeriods} months",
                    IsAccountEnabled = d.IsAccountEnabled,
                    Institution = d.Institution,
                    ipposi = d.ipposi,
                    yearDob = d.yearDob,
                    dob = d.dob
                });
                //Returning Json Data    
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public bool ToggleUser(int UserID)
        {
            var acc_User = db.AccountUsers.SingleOrDefault(d => d.UserID == UserID);
            if (acc_User != null)
            {
                acc_User.IsAccountEnabled = !acc_User.IsAccountEnabled;
                db.SaveChanges();
            }
            return true;
        }

        public bool DeleteUser(int UserID)
        {
            var acc_User = db.AccountUsers.SingleOrDefault(d => d.UserID == UserID);
            if (acc_User != null)
            {
                db.AccountUsers.Remove(acc_User);
                db.SaveChanges();
            }
            return true;
        }

        #endregion
        #region Requests
        public ActionResult Requests()
        {
            return View();
        }

        public ActionResult GetRequests()
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Users data    
                var requests_data = (from user in db.UserRequests
                                     select user);

                //Sorting  
                if (string.IsNullOrEmpty(sortColumn)) sortColumn = "UserRequestID";
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    requests_data = requests_data.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    requests_data = requests_data.Where(m => m.UserBank.User.FirstName.Contains(searchValue) || m.UserBank.User.LastName.Contains(searchValue) || m.UserBank.BankName.Contains(searchValue));
                }

                //total number of rows count     
                recordsTotal = requests_data.Count();
                //Paging     
                var data = requests_data.Skip(skip).Take(pageSize).ToList().Select(d => new
                {
                    RequestID = d.UserRequestID,
                    FullName = $"{d.UserBank.User.FirstName} {d.UserBank.User.LastName}",
                    Amount = d.Amount,
                    InterestRate = d.InterestRate,
                    PaybackPeriod = d.PaybackPeriod,
                    BankAccount = d.UserBank.BankName,
                    AccountNo = d.UserBank.AccountNumber,
                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(d.DateTime, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")).ToString("dd-MMM-yyyy hh:mm tt"),
                    IsApproved = d.IsApproved,
                    yearDob = d.UserBank.User.yearDob
                });
                //Returning Json Data    
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }


        public bool ApproveRequest(int RequestID, string Reference = "", bool IsTopUp = false)
        {
            var request = db.UserRequests.SingleOrDefault(d => d.UserRequestID == RequestID);
            if (request != null)
            {
                var userTransactions = db.UserTransactions.Where(d => d.UserRequest != null && d.UserRequest.UserBank != null && d.UserRequest.UserBank.UserID == request.UserBank.UserID);
                var creditSum = userTransactions.Select(d => d.Credit).DefaultIfEmpty(0).Sum();
                var debitSum = userTransactions.Select(d => d.Debit).DefaultIfEmpty(0).Sum();

                var interestRate = Convert.ToSingle(request.InterestRate.Split('%')[0]) / 100;
                var paybackPeriod = Convert.ToInt32(request.PaybackPeriod.Split(' ')[0]);
                var amountToBePaid = Convert.ToSingle(request.Amount * Math.Pow((1 + (interestRate)), paybackPeriod));
                db.UserTransactions.Add(new UserTransaction
                {
                    UserRequestID = request.UserRequestID,
                    AmountDisbursed = request.Amount,
                    DateTime = DateTime.UtcNow,
                    ReferenceNumber = Reference,
                    IsLoanTopUp = IsTopUp,
                    Balance = (debitSum - creditSum + request.Amount),
                    Debit = request.Amount,
                    Credit = 0,
                    AmountToBePaid = amountToBePaid,
                    AmountEarned = (amountToBePaid - request.Amount)
                });

                var u2 = request.UserBank.User.PhoneNumber;
                //int temp;
                //var user = (from req in db.UserRequests
                //    join bank in db.UserBanks on request.UserBankID equals bank.UserBankID
                //    join u in db.Users on bank.UserID equals int.TryParse(u.Id, out temp)?temp:0
                //            where req.UserRequestID == RequestID
                //    select u).SingleOrDefault();

                request.IsApproved = true;
                db.SaveChanges();
                SendSMS(u2, "Congratulations " + request.UserBank.User.FirstName + "! your request for #" + request.Amount + " has been approved.");
            }
            return true;
        }

        //public bool LoanTopUp(int RequestID, string Reference = "")
        //{
        //    var request = db.UserRequests.SingleOrDefault(d => d.UserRequestID == RequestID);
        //    if (request != null)
        //    {
        //        var userTransactions = db.UserTransactions.Where(d => d.UserRequest != null && d.UserRequest.UserBank != null && d.UserRequest.UserBank.UserID == request.UserBank.UserID);
        //        var creditSum = userTransactions.Select(d => d.Credit).DefaultIfEmpty(0).Sum();
        //        var debitSum = userTransactions.Select(d => d.Debit).DefaultIfEmpty(0).Sum();

        //        var interestRate = Convert.ToSingle(request.InterestRate.Split('%')[0]) / 100;
        //        var paybackPeriod = Convert.ToInt32(request.PaybackPeriod.Split(' ')[0]);
        //        var amountToBePaid = Convert.ToSingle(request.Amount * Math.Pow((1 + (interestRate)), paybackPeriod));
        //        db.UserTransactions.Add(new UserTransaction
        //        {
        //            UserRequestID = request.UserRequestID,
        //            AmountDisbursed = request.Amount,
        //            DateTime = DateTime.UtcNow,
        //            ReferenceNumber = Reference,
        //            Balance = (debitSum - creditSum + request.Amount),
        //            Debit = request.Amount,
        //            Credit = 0,
        //            AmountToBePaid = amountToBePaid,
        //            AmountEarned = (amountToBePaid - request.Amount)
        //        });

        //        var u2 = request.UserBank.User.PhoneNumber;
        //        //int temp;
        //        //var user = (from req in db.UserRequests
        //        //    join bank in db.UserBanks on request.UserBankID equals bank.UserBankID
        //        //    join u in db.Users on bank.UserID equals int.TryParse(u.Id, out temp)?temp:0
        //        //            where req.UserRequestID == RequestID
        //        //    select u).SingleOrDefault();

        //        request.IsApproved = true;
        //        db.SaveChanges();
        //        SendSMS(u2, "Congratulations " + request.UserBank.User.FirstName + "! your request for #" + request.Amount + " has been approved.");
        //    }
        //    return true;
        //}

        public bool SendSMS(string receiver, string msg)
        {
            try
            {
                var phoneNumber = receiver;
                var message = msg;
                var from = "GWCU Credit";
                var gatewayResponse = _atGWInstance.SendMessage(phoneNumber, message, from);
                var success = gatewayResponse["SMSMessageData"]["Recipients"][0]["status"] == "Success";
                Assert.IsTrue(success, "Should successfully send message to a valid phone number");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
        public bool DeleteRequest(int RequestID)
        {
            var request = db.UserRequests.SingleOrDefault(d => d.UserRequestID == RequestID);
            if (request != null)
            {
                db.UserRequests.Remove(request);
                db.SaveChanges();
            }
            return true;
        }

        #endregion
        #region Transactions
        public ActionResult Transactions()
        {
            return View();
        }


        public ActionResult Pdf(int id)
        {
            var trans = db.UserTransactions.Where(c => c.UserTransactionID == id).FirstOrDefault();

            var model = new PdfViewModel
            {
                Id = trans.UserTransactionID,
                transNo = trans.UserTransactionID,
                AMountDisbursed = trans.AmountDisbursed,
                Balance = trans.Balance,
                Credit = trans.Credit,
                Debit = trans.Debit,
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(trans.DateTime, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")),
                Institution = trans.UserRequest.UserBank.User.Institution,
                AmountToPay = trans.AmountToBePaid,
                InterestRate = trans.UserRequest.InterestRate,
                LoadTerm = trans.UserRequest.PaybackPeriod,
                LoanAMount = trans.UserRequest.Amount,
                TotalPayment = trans.AmountToBePaid,
                PreparedFor = trans.UserRequest.UserBank.User.FirstName + " " + trans.UserRequest.UserBank.User.LastName,
                ReferenceNo = trans.ReferenceNumber,
                RequestedOn = TimeZoneInfo.ConvertTimeFromUtc(trans.UserRequest.DateTime, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")).ToString("dd-MMM-yyyy hh:mm tt"),

            };

            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Grid.pdf");
            }
        }

        public ActionResult GetTransactions()
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Users data    
                var transaction_data = (from ut in db.UserTransactions
                                        select ut);

                //Sorting  
                if (string.IsNullOrEmpty(sortColumn)) sortColumn = "UserTransactionID";
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    transaction_data = transaction_data.OrderBy(sortColumn + " " + sortColumnDir);
                }
                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    transaction_data = transaction_data.Where(m => m.UserRequest.UserBank.User.FirstName.Contains(searchValue) || m.UserRequest.UserBank.User.LastName.Contains(searchValue) || m.ReferenceNumber.Contains(searchValue));
                }

                //total number of rows count     
                recordsTotal = transaction_data.Count();
                //Paging     
                var data = transaction_data.Skip(skip).Take(pageSize).ToList().Select(d => new
                {
                    TransactionID = d.UserTransactionID,
                    FullName = $"{d.UserRequest.UserBank.User.FirstName} {d.UserRequest.UserBank.User.LastName}",
                    ReferenceNo = d.ReferenceNumber,
                    IsTopUP = d.IsLoanTopUp == true? "Yes":"No",
                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(d.DateTime, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")).ToString("dd-MMM-yyyy hh:mm tt"),
                    RequestDateTime = TimeZoneInfo.ConvertTimeFromUtc(d.UserRequest.DateTime, TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")).ToString("dd-MMM-yyyy hh:mm tt"),
                    Institution = d.UserRequest.UserBank.User.Institution,
                    Debit = d.Debit,
                    Credit = d.Credit,
                    Balance = d.Balance,
                    PaybackPeriods = d.UserRequest.UserBank.User.PaybackPeriods,
                    AmountToBePaid = d.AmountToBePaid,
                    AmountEarned = d.AmountEarned,
                    AmountDisbursed = d.AmountDisbursed,
                    TimePeriod = $"{d.DateTime.ToString("dd-MMM-yyyy")} TO {d.DateTime.AddMonths(Convert.ToInt32(d.UserRequest.PaybackPeriod.Split(' ')[0])).ToString("dd-MMM-yyyy")}",
                    InterestRate = d.UserRequest.InterestRate,
                    BankName = d.UserRequest.UserBank.BankName,
                    AccountNumber = d.UserRequest.UserBank.AccountNumber,
                    ipposi = d.UserRequest.UserBank.User.ipposi,
                    IsReversed = d.IsReversed
                });
                //Returning Json Data    
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public bool DeleteTransaction(int TransactionID)
        {
            var transaction = db.UserTransactions.SingleOrDefault(d => d.UserTransactionID == TransactionID);
            if (transaction != null)
            {
                db.UserTransactions.Remove(transaction);
                db.SaveChanges();
            }
            return true;
        }

        public bool ReverseTransaction(int TransactionID)
        {
            var transaction = db.UserTransactions.SingleOrDefault(d => d.UserTransactionID == TransactionID);
            if (transaction != null)
            {
                transaction.IsReversed = true;

                db.UserTransactions.Add(new UserTransaction
                {
                    UserRequestID = transaction.UserRequestID,
                    AmountDisbursed = transaction.AmountDisbursed,
                    AmountEarned = transaction.AmountEarned,
                    AmountToBePaid = transaction.AmountToBePaid,
                    Balance = transaction.Balance,
                    Credit = transaction.Debit,
                    Debit = transaction.Credit,
                    DateTime = DateTime.UtcNow,
                    ReferenceNumber = $"Reversing Transaction Ref: {transaction.ReferenceNumber}",
                    IsReversed = true
                });
                db.SaveChanges();
            }
            return true;
        }
        #endregion

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public string MakeRequest()
        {
            var responseString = string.Empty;
            try
            {
                var amount = 0f;
                var sessionID = Request["sessionId"].ToString();
                var networkCode = Request["networkCode"].ToString();
                var serviceCode = Request["serviceCode"].ToString();
                var phoneNumber = Request["phoneNumber"].ToString();
                var text = Request["text"] != null ? Request["text"].ToString() : string.Empty;

         

                //First Request
                if (text == "")
                    responseString = "CON Welcome to GCWU Credit Union. What is your username?";
                else
                {
                    var splitRequest = text.Split('*');

                    if (splitRequest.Length == 1)
                        responseString = "CON What is your password?";

                    if (splitRequest.Length == 2)
                    {
                        var username = splitRequest[0];
                        var password = splitRequest[1];
                        var passwordHash = CreateMD5(password);

                        var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
                        if (user != null)
                            responseString = "CON Please select an option:\n 1. Borrow Money\n 2. See Account Status";
                        else
                            responseString = "END Wrond username and/or password. Please try again!";
                    }

                    if (splitRequest.Length == 3)
                    {
                        var username = splitRequest[0];
                        var password = splitRequest[1];
                        var passwordHash = CreateMD5(password);
                        var menuOption = splitRequest[2];

                        var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
                        if (user != null)
                        {
                            if (menuOption.Equals("1"))
                            {
                                var allowedAmountOptions = $" 1. {user.CreditAvailable * 0.05f}\n 2. {user.CreditAvailable * 0.10f}\n 3. {user.CreditAvailable * 0.20f}\n 4. {user.CreditAvailable * 0.50f}\n 5. {user.CreditAvailable}";
                                responseString = "CON Please select an amount:\n" + allowedAmountOptions;
                            }

                            if (menuOption.Equals("2"))
                            {
                                var lastTransaction = user.UserTransactions.LastOrDefault();
                                var balance = lastTransaction != null ? lastTransaction.Balance : 0f;
                                responseString = $"END Account Status:\n Balance: {balance}, Limit: {user.CreditLimit}, Credit Remaining: {user.CreditAvailable}";
                            }
                        }
                        else
                            responseString = "END An error occured while processing your request. Please try again!";
                    }

                    if (splitRequest.Length == 4)
                    {
                        var username = splitRequest[0];
                        var password = splitRequest[1];
                        var passwordHash = CreateMD5(password);
                        var menuOption = splitRequest[2];
                        var creditAmountOption = splitRequest[3];


                        var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
                        if (user != null)
                        {
                            var interestOptions = "";
                            var optionNumber = 0;
                            var interestRates = user.InterestRates.Split(',');
                            var paybackPeriods = user.PaybackPeriods.Split(',');

                            foreach (var interestRate in interestRates)
                            {
                                optionNumber++;
                                interestOptions += $"\n {optionNumber}. {interestRate}% in {paybackPeriods[optionNumber - 1]} months";
                            }

                            responseString = $"CON Select payback period and interest rate: {interestOptions}";
                        }
                    }

                    if (splitRequest.Length == 5)
                    {
                        var username = splitRequest[0];
                        var password = splitRequest[1];
                        var passwordHash = CreateMD5(password);
                        var menuOption = splitRequest[2];
                        var creditAmount = splitRequest[3];
                        var interestOption = splitRequest[4];

                        var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
                        if (user != null)
                        {
                            var bankNames = user.UserBanks.Where(d => !string.IsNullOrEmpty(d.BankName) || !string.IsNullOrEmpty(d.AccountNumber) || !string.IsNullOrWhiteSpace(d.BankName) || !string.IsNullOrWhiteSpace(d.AccountNumber)).Select(d => d.BankName);
                            var bankOptions = "";
                            var optionNumber = 0;

                            foreach (var bank in bankNames)
                            {
                                optionNumber++;
                                bankOptions += $"\n {optionNumber}. {bank}";
                            }
                            responseString = $"CON Select destination bank account: {bankOptions}";
                        }
                    }

                    if (splitRequest.Length == 6)
                    {
                        var username = splitRequest[0];
                        var password = splitRequest[1];
                        var passwordHash = CreateMD5(password);
                        var menuOption = splitRequest[2];
                        var creditAmount = splitRequest[3];
                        var interestOption = splitRequest[4];
                        var bankOption = splitRequest[5];

                        var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
                        if (user != null)
                        {
                            responseString = "CON Do you confirm you have read and understood our terms and conditions at: https://bit.ly/2WngXZf transaction.\n\n 1.Confirm\n 2.Cancel";
                        }
                    }

                    if (splitRequest.Length == 7)
                    {
                        var username = splitRequest[0];
                        var password = splitRequest[1];
                        var passwordHash = CreateMD5(password);
                        var menuOption = splitRequest[2];
                        var creditAmountOption = splitRequest[3];
                        var interestOption = splitRequest[4];
                        var bankOption = splitRequest[5];
                        var confirmOption = splitRequest[6];

                        if (confirmOption.Equals("1"))
                        {

                            var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
                            if (user != null)
                            {
                                var creditAmount = 0f;
                                switch (creditAmountOption)
                                {
                                    case "1":
                                        creditAmount = user.CreditAvailable * 0.05f;
                                        break;
                                    case "2":
                                        creditAmount = user.CreditAvailable * 0.10f;
                                        break;
                                    case "3":
                                        creditAmount = user.CreditAvailable * 0.20f;
                                        break;
                                    case "4":
                                        creditAmount = user.CreditAvailable * 0.50f;
                                        break;
                                    case "5":
                                    default:
                                        creditAmount = user.CreditAvailable;
                                        break;
                                }

                                var interestRate = user.InterestRates.Split(',')[Convert.ToInt32(interestOption) - 1];
                                var paybackPeriod = user.PaybackPeriods.Split(',')[Convert.ToInt32(interestOption) - 1];
                                var bank = user.UserBanks.ToArray()[(Convert.ToInt32(bankOption) - 1)];

                                db.UserRequests.Add(new UserRequest
                                {
                                    DateTime = DateTime.UtcNow,
                                    Amount = creditAmount,
                                    InterestRate = $"{interestRate}%",
                                    PaybackPeriod = $"{paybackPeriod} months",
                                    UserBankID = bank.UserBankID,
                                    UserBank = bank
                                });
                                user.CreditUtilized += creditAmount;
                                db.SaveChanges();

                                responseString = "END Transaction Confirmed. Thanks for your business.";
                                amount = creditAmount;
                                SendEmails("requests@gwcu.ca", "Request Created", user.FirstName + " of " + user.Institution + " and ippois no " + user.ipposi + " has requested amount for " + amount + " for " + DateTime.UtcNow);

                            }
                        }
                    }

                    //var username1 = splitRequest[0];
                    //var password1 = splitRequest[1];
                    //var passwordHash1 = CreateMD5(password1);

                    //var user1 = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash1));


                }
            }

            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    //Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    //   );

                    using (FileStream fs = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath("~/errorlog.txt"), FileMode.Open, FileAccess.Write))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes(eve.Entry.Entity.GetType().Name + "---" + eve.Entry.State);
                        fs.Write(info, 0, info.Length);
                    }

                    foreach (var ve in eve.ValidationErrors)
                    {
                      

                        //Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                        //    ve.PropertyName, ve.ErrorMessage);

                        using (FileStream fs = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath("~/errorlog.txt"), FileMode.Open, FileAccess.Write))
                        {
                            byte[] info = new UTF8Encoding(true).GetBytes(ve.PropertyName + "----" + ve.ErrorMessage);
                            fs.Write(info, 0, info.Length);
                        }
                    }
                }


                throw;
            }

            //catch (Exception ex)
            //{

            //    using (FileStream fs = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath("~/errorlog.txt"), FileMode.Open, FileAccess.Write))
            //    {
            //        byte[] info = new UTF8Encoding(true).GetBytes(ex.Message.ToString());
            //        fs.Write(info, 0, info.Length);
            //    }
            //}

            return responseString;
        }


        // client original code
        //[HttpPost]
        //[AllowAnonymous]
        //public string MakeRequest()
        //{
        //    var amount = 0f;
        //    var sessionID = Request["sessionId"].ToString();
        //    var networkCode = Request["networkCode"].ToString();
        //    var serviceCode = Request["serviceCode"].ToString();
        //    var phoneNumber = Request["phoneNumber"].ToString();
        //    var text = Request["text"] != null ? Request["text"].ToString() : string.Empty;

        //    var responseString = string.Empty;

        //    //First Request
        //    if (text == "")
        //        responseString = "CON Welcome to GCWU Credit Union. What is your username?";
        //    else
        //    {
        //        var splitRequest = text.Split('*');

        //        if (splitRequest.Length == 1)
        //            responseString = "CON What is your password?";

        //        if (splitRequest.Length == 2)
        //        {
        //            var username = splitRequest[0];
        //            var password = splitRequest[1];
        //            var passwordHash = CreateMD5(password);

        //            var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
        //            if (user != null)
        //                responseString = "CON Please select an option:\n 1. Borrow Money\n 2. See Account Status";
        //            else
        //                responseString = "END Wrond username and/or password. Please try again!";
        //        }

        //        if (splitRequest.Length == 3)
        //        {
        //            var username = splitRequest[0];
        //            var password = splitRequest[1];
        //            var passwordHash = CreateMD5(password);
        //            var menuOption = splitRequest[2];

        //            var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
        //            if (user != null)
        //            {
        //                if (menuOption.Equals("1"))
        //                {
        //                    var allowedAmountOptions = $" 1. {user.CreditAvailable * 0.05f}\n 2. {user.CreditAvailable * 0.10f}\n 3. {user.CreditAvailable * 0.20f}\n 4. {user.CreditAvailable * 0.50f}\n 5. {user.CreditAvailable}";
        //                    responseString = "CON Please select an amount:\n" + allowedAmountOptions;
        //                }

        //                if (menuOption.Equals("2"))
        //                {
        //                    var lastTransaction = user.UserTransactions.LastOrDefault();
        //                    var balance = lastTransaction != null ? lastTransaction.Balance : 0f;
        //                    responseString = $"END Account Status:\n Balance: {balance}, Limit: {user.CreditLimit}, Credit Remaining: {user.CreditAvailable}";
        //                }
        //            }
        //            else
        //                responseString = "END An error occured while processing your request. Please try again!";
        //        }

        //        if (splitRequest.Length == 4)
        //        {
        //            var username = splitRequest[0];
        //            var password = splitRequest[1];
        //            var passwordHash = CreateMD5(password);
        //            var menuOption = splitRequest[2];
        //            var creditAmountOption = splitRequest[3];


        //            var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
        //            if (user != null)
        //            {
        //                var interestOptions = "";
        //                var optionNumber = 0;
        //                var interestRates = user.InterestRates.Split(',');
        //                var paybackPeriods = user.PaybackPeriods.Split(',');

        //                foreach (var interestRate in interestRates)
        //                {
        //                    optionNumber++;
        //                    interestOptions += $"\n {optionNumber}. {interestRate}% in {paybackPeriods[optionNumber - 1]} months";
        //                }

        //                responseString = $"CON Select payback period and interest rate: {interestOptions}";
        //            }
        //        }

        //        if (splitRequest.Length == 5)
        //        {
        //            var username = splitRequest[0];
        //            var password = splitRequest[1];
        //            var passwordHash = CreateMD5(password);
        //            var menuOption = splitRequest[2];
        //            var creditAmount = splitRequest[3];
        //            var interestOption = splitRequest[4];

        //            var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
        //            if (user != null)
        //            {
        //                var bankNames = user.UserBanks.Where(d => !string.IsNullOrEmpty(d.BankName) || !string.IsNullOrEmpty(d.AccountNumber) || !string.IsNullOrWhiteSpace(d.BankName) || !string.IsNullOrWhiteSpace(d.AccountNumber)).Select(d => d.BankName);
        //                var bankOptions = "";
        //                var optionNumber = 0;

        //                foreach (var bank in bankNames)
        //                {
        //                    optionNumber++;
        //                    bankOptions += $"\n {optionNumber}. {bank}";
        //                }
        //                responseString = $"CON Select destination bank account: {bankOptions}";
        //            }
        //        }

        //        if (splitRequest.Length == 6)
        //        {
        //            var username = splitRequest[0];
        //            var password = splitRequest[1];
        //            var passwordHash = CreateMD5(password);
        //            var menuOption = splitRequest[2];
        //            var creditAmount = splitRequest[3];
        //            var interestOption = splitRequest[4];
        //            var bankOption = splitRequest[5];

        //            var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
        //            if (user != null)
        //            {
        //                responseString = "CON Do you confirm you have read and understood our terms and conditions at: https://bit.ly/2WngXZf transaction.\n\n 1.Confirm\n 2.Cancel";
        //            }
        //        }

        //        if (splitRequest.Length == 7)
        //        {
        //            var username = splitRequest[0];
        //            var password = splitRequest[1];
        //            var passwordHash = CreateMD5(password);
        //            var menuOption = splitRequest[2];
        //            var creditAmountOption = splitRequest[3];
        //            var interestOption = splitRequest[4];
        //            var bankOption = splitRequest[5];
        //            var confirmOption = splitRequest[6];

        //            if (confirmOption.Equals("1"))
        //            {

        //                var user = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash));
        //                if (user != null)
        //                {
        //                    var creditAmount = 0f;
        //                    switch (creditAmountOption)
        //                    {
        //                        case "1":
        //                            creditAmount = user.CreditAvailable * 0.05f;
        //                            break;
        //                        case "2":
        //                            creditAmount = user.CreditAvailable * 0.10f;
        //                            break;
        //                        case "3":
        //                            creditAmount = user.CreditAvailable * 0.20f;
        //                            break;
        //                        case "4":
        //                            creditAmount = user.CreditAvailable * 0.50f;
        //                            break;
        //                        case "5":
        //                        default:
        //                            creditAmount = user.CreditAvailable;
        //                            break;
        //                    }

        //                    var interestRate = user.InterestRates.Split(',')[Convert.ToInt32(interestOption) - 1];
        //                    var paybackPeriod = user.PaybackPeriods.Split(',')[Convert.ToInt32(interestOption) - 1];
        //                    var bank = user.UserBanks.ToArray()[(Convert.ToInt32(bankOption) - 1)];

        //                    db.UserRequests.Add(new UserRequest
        //                    {
        //                        DateTime = DateTime.UtcNow,
        //                        Amount = creditAmount,
        //                        InterestRate = $"{interestRate}%",
        //                        PaybackPeriod = $"{paybackPeriod} months",
        //                        UserBankID = bank.UserBankID,
        //                        UserBank = bank
        //                    });
        //                    user.CreditUtilized += creditAmount;
        //                    db.SaveChanges();

        //                    responseString = "END Transaction Confirmed. Thanks for your business.";
        //                    amount = creditAmount;
        //                    SendEmails("requests@gwcu.ca", "Request Created", user.FirstName + " of " + user.Institution + " and ippois no " + user.ipposi + " has requested amount for " + amount + " for " + DateTime.UtcNow);

        //                }
        //            }
        //        }

        //        //var username1 = splitRequest[0];
        //        //var password1 = splitRequest[1];
        //        //var passwordHash1 = CreateMD5(password1);

        //        //var user1 = db.AccountUsers.SingleOrDefault(d => d.Username.Equals(username) && d.PasswordHash.Equals(passwordHash1));


        //    }

        //    return responseString;
        //}

        [NonAction]
        public void SendEmails(string receiver, string subject, string message)
        {
            SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            var senderEmail = section.From;
            var host = section.Network.Host;
            int port = section.Network.Port;
            bool enableSsl = section.Network.EnableSsl;
            string gmail = section.Network.UserName;
            string password = section.Network.Password;
            try
            {
                if (ModelState.IsValid)
                {
                    var receiverEmail = receiver;
                    var sub = subject;
                    var body = message;
                    var smtp = new SmtpClient
                    {
                        Host = host,
                        Port = port,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiver)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }
                    //    ViewBag.Index = "Sucessfully sent";
                }
            }
            catch (Exception)
            {
                //ViewBag.Error = "Some Error";
            }
        }

        public static string CreateMD5(string input)
        {
            return Cipher.encrypt(input);
        }

        public static string DecryptMD5(string input)
        {
            return Cipher.decrypt(input);
        }

        public ActionResult ImportUsers(FileInputViewModel model)
        {
            DateTime current = DateTime.Now;
            var filePath = Server.MapPath("/Content/" + model.File.FileName);
            model.File.SaveAs(filePath);

            var existingUsers = db.AccountUsers.ToList();
            var userData = System.IO.File.ReadAllLines(filePath).Skip(1).Select(d => d.Split(',')).ToList();
            var users = new List<User>();

            foreach (var data in userData)
            {
                try
                {
                    var newUser = new User
                    {
                        IsAccountEnabled = true,
                        Username = data[0],
                        PasswordHash = CreateMD5(data[1]),
                        LastName = data[2],
                        FirstName = data[3],
                        PhoneNumber = data[4],
                        CreditLimit = (float)Convert.ToDecimal(data[5]),
                        CreditUtilized = (float)Convert.ToDecimal(data[6]),
                        InterestRates = string.Join(",", new string[] { data[7], data[8], data[9] }),
                        PaybackPeriods = string.Join(",", new string[] { data[10], data[11], data[12] }),
                        UserBanks = new List<UserBank>
                        {
                            new UserBank {BankName = data[13], AccountNumber = data[14]},
                            new UserBank {BankName = data[15], AccountNumber = data[16]}
                        },
                        Institution = data[17],
                        ipposi = data[18],
                        dob = Convert.ToDateTime(data[19]),
                        yearDob = Years(Convert.ToDateTime(data[19]), current)
                    };

                    db.AccountUsers.Add(newUser);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            db.SaveChanges();
            System.IO.File.Delete(filePath);
            return RedirectToAction("Users");
        }

        public ActionResult Send()
        {
            var model = new SmsViewModel();

            var users = db.AccountUsers.ToList();
            model.Users = users;
            return View(model);
        }

        [HttpPost]
        public JsonResult Send(List<string> list, string message, string all)
        {
            if (all == "all")
            {
                var list1 = db.AccountUsers.ToList();
                var result = false;
                foreach (var number in list1)
                {
                    result = SendSMS(number.PhoneNumber, message);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = false;
                foreach (var number in list)
                {
                    result = SendSMS(number, message);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [WebMethod]
        public JsonResult Send_SMS(List<string> _list, string messagetype, string isall)
        {
            List<User> userlist = null;

            StringBuilder message = null;
            if (messagetype == "welcome")
            {
                userlist = db.AccountUsers.ToList();
                message = new StringBuilder("Congratulations! #lastname, you are now eligbile to borrow from GWCU. These are your login details, please keep secure. Username: #username Password: #password Dial *347*911# to borrow now.");
            }
            else if (messagetype == "borrow")
            {
                userlist = db.AccountUsers.ToList();
                message = new StringBuilder("Hello #lastname, you still have #credit available of credit available. Dial *347*911# to access now!");
            }
            else if (messagetype == "referral")
            {
                //Button Should Only Work If Institution is “University Of Ilorin”(Filter)
                userlist = (from user in db.AccountUsers where user.Institution.Contains("Ilorin") select user).ToList();
                message = new StringBuilder("Hey #lastname, do you want to win 5k with no stress? Just refer a friend to GWCU Unilorin Multipurpose; when they register they will list your username: #username as a referral code. Once they borrow you win up to 5k instantly!");
            }

            if (isall == "all")
            {
                var result = false;
                if (userlist.Count > 0)
                {
                    foreach (var item in userlist)
                    {
                        message.Replace("#username", item.Username);
                        message.Replace("#password", DecryptMD5(item.PasswordHash));
                        message.Replace("#lastname", item.LastName);
                        message.Replace("#credit", Convert.ToString(item.CreditAvailable));
                        result = SendSMS(item.PhoneNumber, Convert.ToString(message));
                    }
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = false;
                if (_list != null)
                {
                    if (userlist.Count > 0)
                    {
                        foreach (var item in _list)
                        {
                            if (messagetype == "referral")
                            {
                                var userreferral = userlist.Where(x => x.PhoneNumber == item).ToList();
                                foreach (var inneritem in userreferral)
                                {
                                    message.Replace("#username", inneritem.Username);
                                    message.Replace("#password", DecryptMD5(inneritem.PasswordHash));
                                    message.Replace("#lastname", inneritem.LastName);
                                    message.Replace("#credit", Convert.ToString(inneritem.CreditAvailable));
                                    result = SendSMS(inneritem.PhoneNumber, Convert.ToString(message));
                                }
                            }
                            else
                            {
                                var user = userlist.First(x => x.PhoneNumber == item);
                                message.Replace("#username", user.Username);
                                message.Replace("#password", DecryptMD5(user.PasswordHash));
                                message.Replace("#lastname", user.LastName);
                                message.Replace("#credit", Convert.ToString(user.CreditAvailable));
                                result = SendSMS(user.PhoneNumber, Convert.ToString(message));
                            }
                        }
                    }
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetUsersIn()
        {
            var vmodel = (from u in db.AccountUsers
                          select new
                          {
                              UserId = u.UserID,
                              UserName = u.Username,
                              Phone = u.PhoneNumber,
                              Institution = u.Institution,
                              ipposi = u.ipposi,
                              FirstName = u.FirstName,
                              LastName = u.LastName
                          }).ToList();
            return Json(vmodel, JsonRequestBehavior.AllowGet);
        }


        public class FileInputViewModel
        {
            public HttpPostedFileBase File { get; set; }
        }

        int Years(DateTime start, DateTime end)
        {
            return (end.Year - start.Year - 1) +
                (((end.Month > start.Month) ||
                ((end.Month == start.Month) && (end.Day >= start.Day))) ? 1 : 0);
        }
    }
}

