﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DataServices.Models;
using DataServices.Repositories;
using DataServices.Repositories.Interfaces;
//using QBODataCollect.Models;
//using QBODataCollect.Repositories;
//using QBODataCollect.Repositories.Interfaces;
using System.Data.CData.QuickBooksOnline;
using System.Xml;
using System.Text;
using System.IO;
using System.Data;
using LoggerService;
using Microsoft.AspNetCore.Authorization;
using QBODataCollect.ViewModels;
using Microsoft.Extensions.Configuration;
using QBODataCollect.Helpers;

namespace QBODataCollect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MasterController : ControllerBase
    {
        private bool useSandBox;
        private string runTimeLicense = "";
        private readonly ICustomerRepository _customerRepo;
        private readonly IQBOAccessRepository _qboaccessRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ISubscriberRepository _subscriberRepo;
        private readonly IErrorLogRepository _errorLogRepo;
        private ILoggerManager _logger;
        private int subscriberId;
        private readonly string serviceName = "";
        private string currentMethodName = "";
        private string appOauthAccessToken = "";
        private string appOauthRefreshToken = "";
        private string companyId = "";
        private List<Customer> customerList = new List<Customer>();
        private List<Invoice> invoiceList = new List<Invoice>();
        protected IConfiguration _configuration;
        private readonly string SuccessMessage = "Sync completed successfully";

        public MasterController(ICustomerRepository customerRepo, IQBOAccessRepository qboaccessRepo, IInvoiceRepository invoiceRepo, ISubscriberRepository subscriberRepo, ILoggerManager logger, IErrorLogRepository errorLogRepo, IConfiguration configuration)
        {
            _customerRepo = customerRepo;
            _qboaccessRepo = qboaccessRepo;
            _invoiceRepo = invoiceRepo;
            _subscriberRepo = subscriberRepo;
            _logger = logger;
            _errorLogRepo = errorLogRepo;
            _configuration = configuration;
            useSandBox = Convert.ToBoolean(_configuration["CDataConfiguration:useSandBox"]);
            runTimeLicense = _configuration["CDataConfiguration:connectionRunTimeLicense"];
            serviceName = GetType().Namespace.Substring(0, GetType().Namespace.IndexOf('.'));
        }

        // GET: api/master/Id
        [HttpGet("{id}")]
        public ActionResult<QBSyncResponse> Client(int id)
        {
            //If the id is 0, return all subscribers otherwise return the requested subscriber
            string bRtn;
            bool updateAccessTableResult;
            subscriberId = id;
            IEnumerable<Subscriber> subscriber;
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();

            if (subscriberId == 0)
            {
                subscriber = _subscriberRepo.GetAllSubscribers();
            }
            else
            {
                subscriber = _subscriberRepo.GetById(subscriberId);
                if (subscriber == null)
                {
                    return new QBSyncResponse()
                    {
                        ResponseStatus = false,
                        ResponseMessage = "Subscriber not found."
                    };
                }
            }

            foreach (Subscriber subs in subscriber)
            {
                subscriberId = subs.Id;
                _logger.LogInfo("Begin Subscriber " + subscriberId + " Authorization");
                QBOAccess qboAccess = _qboaccessRepo.GetById(subscriberId);
                if(qboAccess == null)
                {
                    return new QBSyncResponse()
                    {
                        ResponseStatus = false,
                        ResponseMessage = "You must authorize with QuickBooks before syncing your data."
                    };
                }
                // save Access Id
                int qboAccessId = qboAccess.Id;

                // Refresh QBO connection
                bRtn = RefreshQBO(qboAccess);
                if (bRtn != SuccessMessage)
                {
                    _qboaccessRepo.DeleteQBOAccess(subscriberId);
                    return new QBSyncResponse()
                    {
                        ResponseStatus = false,
                        ResponseMessage = bRtn
                    };
                }
                // Update Access table with new refresh token
                try
                {
                    AESCryptography cryptography = new AESCryptography(_configuration);
                    appOauthRefreshToken = cryptography.Encrypt(appOauthRefreshToken);
                    if (IsAllDigits(qboAccess.Company))
                    {
                        companyId = cryptography.Encrypt(qboAccess.Company);
                    }
                    else
                    {
                        companyId = qboAccess.Company;
                    }
                    updateAccessTableResult = _qboaccessRepo.UpdateQBOAccess(qboAccessId, companyId, appOauthAccessToken, appOauthRefreshToken, qboAccess);
                    if (updateAccessTableResult == false)
                    {
                        return new QBSyncResponse()
                        {
                            ResponseStatus = false,
                            ResponseMessage = "You will need to re-authorize your QuickBooks account and try to sync again."
                        };
                    }
                }
                catch(Exception ex)
                {
                    _errorLogRepo.InsertErrorLog(new ErrorLog
                    {
                        SubscriberId = subscriberId,
                        ErrorMessage = ex.Message,
                        ServiceName = serviceName,
                        MethodName = currentMethodName,
                        ErrorDateTime = DateTime.Now
                    });
                    return new QBSyncResponse()
                    {
                        ResponseStatus = false,
                        ResponseMessage = "Error occurred " + ex.Message
                    };
                }
                _logger.LogInfo("End Subscriber " + subscriberId + " Authorization");

                //Time to get some data from QBO
                _logger.LogInfo("Begin QBO Data Access for Subscriber " + subscriberId);
                // Get and Update Customers & Invoices
                bRtn = GetQBOCustomers(qboAccess);
                if(bRtn != SuccessMessage)
                {
                    return new QBSyncResponse()
                    {
                        ResponseStatus = false,
                        ResponseMessage = bRtn
                    };
                }
                _logger.LogInfo("End QBO Data Access for Subscriber " + subscriberId);
                //Update the last sync date in the subscriber table
                var updateSyncDateResult = _subscriberRepo.UpdateSubscriber(subscriberId, DateTime.Now, subs);
                if (updateSyncDateResult == false)
                {
                    return new QBSyncResponse()
                    {
                        ResponseStatus = false,
                        ResponseMessage = "Not able to update last sync date for subscriber"
                    };
                }
            }
            return new QBSyncResponse()
            {
                ResponseStatus = true,
                ResponseMessage = SuccessMessage
            };
        }
        
        //Refresh QBO
        private string RefreshQBO(QBOAccess qboAccess)
        {
            AESCryptography cryptography = new AESCryptography(_configuration);
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.Offline = false;
            connString.OAuthClientId = qboAccess.ClientId;
            connString.OAuthClientSecret = qboAccess.ClientSecret;
            if(!IsAllDigits(qboAccess.Company))
            {
                qboAccess.Company = cryptography.Decrypt(qboAccess.Company);
                qboAccess.RefreshToken = cryptography.Decrypt(qboAccess.RefreshToken);
            }
            connString.CompanyId = qboAccess.Company;
            connString.OAuthRefreshToken = qboAccess.RefreshToken;
            connString.OAuthVersion = "2.0";
            connString.UseSandbox = useSandBox;
            //connString.InitiateOAuth = "GETANDREFRESH";
            connString.Logfile = "c:\\users\\public\\documents\\rssApiLog.txt";
            connString.Verbosity = "5";
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();

            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    connQBO.RuntimeLicense = runTimeLicense;
                    using (QuickBooksOnlineCommand cmdQBO = new QuickBooksOnlineCommand("RefreshOAuthAccessToken", connQBO))
                    {
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("OAuthRefreshToken", qboAccess.RefreshToken));
                        cmdQBO.CommandType = CommandType.StoredProcedure;

                        using (QuickBooksOnlineDataReader reader = cmdQBO.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                appOauthAccessToken = (String)reader["OAuthAccessToken"];
                                appOauthRefreshToken = (String)reader["OAuthRefreshToken"];
                            }
                            else
                            {
                                _errorLogRepo.InsertErrorLog(new ErrorLog
                                {
                                    SubscriberId = qboAccess.SubscriberId,
                                    ErrorMessage = "You will need to re-authorize your QuickBooks account and try to sync again.",
                                    ServiceName = serviceName,
                                    MethodName = currentMethodName,
                                    ErrorDateTime = DateTime.Now
                                });
                                return "You will need to re-authorize your QuickBooks account and try to sync again.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = qboAccess.SubscriberId,
                    ErrorMessage = ex.Message,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return ex.Message;
            }
            return SuccessMessage;
        }

        // Get Customers
        private string GetQBOCustomers(QBOAccess qboAccess)
        {
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.Offline = false;
            connString.OAuthAccessToken = appOauthAccessToken;
            connString.OAuthClientId = qboAccess.ClientId;
            connString.OAuthClientSecret = qboAccess.ClientSecret;   
            connString.CompanyId = qboAccess.Company;       
            connString.OAuthVersion = "2.0";
            connString.UseSandbox = useSandBox;
            // To insert error log in catch statement, made this variable public
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();
            int colIndex = 0;
            string QBCId;
            string GName;
            string FName;
            string Suf;
            string DName = "";
            string CName;
            string PPhone;
            string MPhone;
            string PEmail;
            // string Nte;

            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    connQBO.RuntimeLicense = runTimeLicense;
                    using (QuickBooksOnlineCommand cmdQBO = new QuickBooksOnlineCommand("Select * FROM Customers WHERE Active IN (true,false)", connQBO))
                    {
                        using (QuickBooksOnlineDataReader reader = cmdQBO.ExecuteReader())
                        {
                           

                            while (reader.Read())
                            {
                                //if (Int32.TryParse((string)reader["Id"], out int CId))
                                //{
                                //}
                                //else
                                //{
                                //    continue;
                                //}
                                QBCId = reader.GetString("Id");
                                colIndex = reader.GetOrdinal("GivenName");
                                GName = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("FamilyName");
                                FName = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("Suffix");
                                Suf = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("DisplayName");
                                DName = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("CompanyName");
                                CName = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("PrimaryPhone_FreeFormNumber");
                                PPhone = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("Mobile_FreeFormNumber");
                                MPhone = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("PrimaryEmailAddr_Address");
                                PEmail = Validate.SafeGetString(reader, colIndex);
                                //colIndex = reader.GetOrdinal("Notes");
                                //Nte = Validate.SafeGetString(reader, colIndex);
                                customerList.Add(new Customer
                                {
                                    CustomerId = 0,
                                    QBCustomerId = QBCId,
                                    GivenName = GName,
                                    FamilyName = FName,
                                    Suffix = Suf,
                                    DisplayName = DName,
                                    CompanyName = CName,
                                    Active = (bool)reader["Active"],
                                    PrimaryPhone = PPhone,
                                    MobilePhone = MPhone,
                                    PrimaryEmailAddress = PEmail,
                                    Balance = Convert.ToDecimal(reader["Balance"]),
                                    // Notes = Nte,
                                    SubscriberId = subscriberId,
                                    SendAutoReminder = true
                                });
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = qboAccess.Id,
                    ErrorMessage = ex.Message,
                    DisplayName = DName,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return "Error occurred " + ex.Message;
            }
            foreach (var cust in customerList)
            {
                Customer customer = _customerRepo.GetByID(subscriberId, cust.QBCustomerId);
                if (customer == null)
                {
                    // customer not found, add it
                    var customerId = _customerRepo.AddCustomer(cust);
                    if (customerId == 0)
                    {
                        return "Not able to add customer in customer table.";
                    }
                    // Get any invoices this customer may have
                    var result = GetInvoices(cust, customerId, connString);
                    if (result != SuccessMessage)
                    {
                        return result;
                    }
                }
                else
                {
                    // we found a customer update it
                    cust.CustomerId = customer.CustomerId;
                    var result = _customerRepo.UpdateCustomer(cust);
                    if (result == false)
                    {
                        return "Not able to update customer in customer table.";
                    }
                    // Get any invoices this customer may have
                    var invoiceResult = GetInvoices(cust, customer.CustomerId, connString);
                    if (invoiceResult != SuccessMessage)
                    {
                        return invoiceResult;
                    } 
                }
            }
            return SuccessMessage;
        }

        //Get Customer Invoices
        private string GetInvoices(Customer customer, int customerId, QuickBooksOnlineConnectionStringBuilder connString)
        {
            // Need to clear the list
            invoiceList = new List<Invoice>();
            // To insert error log in catch statement, made this variable public
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();
            bool addInvoice = false;
            int CustId;
            string QBIId;
            int colIndex = 0;
            string IDNbr = "";
            DateTime IDate;
            DateTime IDueDate;
            Decimal ITotalAmt;
            Decimal IBalance;
            string ITxns;
            try
            {
                using (QuickBooksOnlineConnection connInv = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    connInv.RuntimeLicense = runTimeLicense;
                    using (QuickBooksOnlineCommand cmdInv = new QuickBooksOnlineCommand("Select * FROM Invoices WHERE CustomerRef = " + customer.QBCustomerId, connInv))
                    {
                        using (QuickBooksOnlineDataReader reader = cmdInv.ExecuteReader())
                        {
                           
                            DateTime ILastPymtDate = DateTime.MaxValue;
                            DateTime ILastReminder = DateTime.MaxValue;
                            while (reader.Read())
                            {

                                //if (Int32.TryParse((string)reader["Id"], out int IId))
                                //{
                                //}
                                //else
                                //{
                                //    continue;
                                //}
                                QBIId = reader.GetString("Id");
                                CustId = customerId;
                                colIndex = reader.GetOrdinal("DocNumber");
                                IDNbr = reader.GetString(colIndex);
                                IDate = reader.GetDateTime("TxnDate");
                                IDueDate = reader.GetDateTime("DueDate");
                                ITotalAmt = reader.GetDecimal("TotalAmt");
                                IBalance = reader.GetDecimal("Balance");
                                colIndex = reader.GetOrdinal("LinkedTxnAggregate");
                                ITxns = Validate.SafeGetString(reader, colIndex);
                                //Filter Invoices to keep
                                addInvoice = false;
                                if (IBalance > 0)
                                {
                                    // we always want to add the invoice if the balance > 0
                                    addInvoice = true;
                                }
                                // Get the last payment date
                                XmlDocument xDoc = new XmlDocument();
                                // Convert string to stream
                                byte[] byteArray = Encoding.ASCII.GetBytes(ITxns);
                                MemoryStream stream = new MemoryStream(byteArray);
                                if (stream.Length > 0)
                                {
                                    xDoc.Load(stream);
                                    XmlNodeList xnList = xDoc.SelectNodes("/LinkedTxnAggregate/Row");
                                    // If we have transaction information, process it
                                    if (xnList.Count > 0)
                                    {
                                        foreach (XmlNode xn in xnList)
                                        {
                                            string txnId = xn["TxnId"].InnerXml;
                                            string txnType = xn["TxnType"].InnerXml;
                                            if (txnType == "Payment")
                                            {
                                                DateTime txnDate = GetPymtDate(txnId, connString, IDNbr);
                                                DateTime now = DateTime.Now;
                                                //for test data
                                                //DateTime now = new DateTime(2014, 12, 31);
                                                int monthDiff = GetMonthDifference(now, txnDate);
                                                if (monthDiff < 6)
                                                {
                                                    ILastPymtDate = txnDate;
                                                    addInvoice = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (addInvoice == true)
                                                    {
                                                        //Balance is greater than zero
                                                        //Add the invoice
                                                        ILastPymtDate = txnDate;
                                                    }
                                                    else
                                                    {
                                                        addInvoice = false;
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                // Don't add the invoice
                                if (addInvoice == false) continue;

                                invoiceList.Add(new Invoice
                                {
                                    InvoiceId = 0,
                                    QBInvoiceId = QBIId,
                                    CustomerId = CustId,
                                    InvDocNbr = IDNbr,
                                    InvDate = IDate,
                                    InvDueDate = IDueDate,
                                    InvTotalAmt = ITotalAmt,
                                    InvBalance = IBalance,
                                    InvTxns = ITxns,
                                    InvLastPymtDate = ILastPymtDate,
                                    InvLastReminder = ILastReminder,
                                    SendAutoReminder = true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = subscriberId,
                    ErrorMessage = ex.Message,
                    InvDocNbr = IDNbr,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return "Error occurred " + ex.Message;
            }

            foreach (var inv in invoiceList)
            {
                Invoice invoice = _invoiceRepo.GetByID(inv.CustomerId, inv.QBInvoiceId);
                if (invoice == null)
                {
                    // Need to add a invoice record
                    var result = _invoiceRepo.AddInvoice(inv);
                    if (result == false)
                    {
                        return "Not able to add Invoice.";
                    }
                }
                else
                {
                    // Need to update invoice
                    inv.InvoiceId = invoice.InvoiceId;
                    var result = _invoiceRepo.UpdateInvoice(inv);
                    if (result == false)
                    {
                        return "Not able to update Invoice.";
                    }
                }
            }
            return SuccessMessage;
        }

        //Get Invoice Payments

        private DateTime GetPymtDate(string txnId, QuickBooksOnlineConnectionStringBuilder connString, string IDNbr)
        {
            //For test data
            //DateTime toDay = new DateTime(2014, 12, 31);
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();
            DateTime toDay = DateTime.Now;
            try
            {
                using (QuickBooksOnlineConnection connPymt = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    connPymt.RuntimeLicense = runTimeLicense;
                    using (QuickBooksOnlineCommand cmdPymt = new QuickBooksOnlineCommand("Select TxnDate FROM Payments WHERE Id = " + txnId, connPymt))
                    {
                        using (QuickBooksOnlineDataReader reader = cmdPymt.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime txnDate = reader.GetDateTime("TxnDate");
                                return txnDate;
                            }
                        }
                    }
                }
                return toDay;
            }
            catch(Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = subscriberId,
                    ErrorMessage = ex.Message,
                    InvDocNbr = IDNbr,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return toDay;
            }
        }

        private static int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }

        private static bool IsAllDigits(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
    }
    static class Validate
    {
        public static string SafeGetString(this QuickBooksOnlineDataReader reader, int colIndex)
        {
            try
            {
                if (!reader.IsDBNull(colIndex))
                    return reader.GetString(colIndex);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            
        }
    }

}