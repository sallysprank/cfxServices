using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QBODataCollect.Models;
using QBODataCollect.Repositories;
using QBODataCollect.Repositories.Interfaces;
using System.Data.CData.QuickBooksOnline;
using System.Xml;
using System.Text;
using System.IO;
using System.Data;
using LoggerService;
using System.Collections;

namespace QBODataCollect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IQBOAccessRepository _qboaccessRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly ISubscriberRepository _subscriberRepo;
        private ILoggerManager _logger;
        private int subscriberId;
        private string appOauthAccessToken = "";
        private string appOauthRefreshToken = "";
        private List<Customer> customerList = new List<Customer>();
        private List<Invoice> invoiceList = new List<Invoice>();

        public MasterController(ICustomerRepository customerRepo, IQBOAccessRepository qboaccessRepo, IInvoiceRepository invoiceRepo, ISubscriberRepository subscriberRepo, ILoggerManager logger)
        {
            _customerRepo = customerRepo;
            _qboaccessRepo = qboaccessRepo;
            _invoiceRepo = invoiceRepo;
            _subscriberRepo = subscriberRepo;
            _logger = logger;
        }

        // GET: api/master/Id
        [HttpGet("{id}")]
        public ActionResult<bool> Client(int id)
        {
            //If the id is 0, return all subscribers otherwise return the requested subscriber
            bool bRtn;
            subscriberId = id;
            IEnumerable<Subscriber> subscriber;

            if (subscriberId == 0)
            {
                subscriber = _subscriberRepo.GetAllSubscribers();
            }
            else
            {
                subscriber = _subscriberRepo.GetById(subscriberId);
                if (subscriber == null)
                {
                    return false;
                }
            }

            foreach (Subscriber subs in subscriber)
            {
                return true;
            }

            //QBOAccess qboAccess = _qboaccessRepo.GetById(subscriberId);
            //// save Access Id
            //int qboAccessId = qboAccess.Id;
            ////throw new Exception("Exception while fetching QBO access record.");

            //// Refresh QBO connection
            //bRtn = RefreshQBO(qboAccess);
            //if (bRtn == false) return false;

            //// Update Access table with new refresh token
            //try
            //{
            //    bRtn = _qboaccessRepo.UpdateQBOAccess(qboAccessId, appOauthAccessToken, appOauthRefreshToken, qboAccess);
            //    if (bRtn == false) return false;
            //}   
            //catch
            //{
            //    // Need to add error processing
            //    return false;
            //}

            ////Time to get some data from QBO
            //// Get and Update Customers & Invoices
            //bRtn = GetQBOCustomers(qboAccess);
            return true;
        }

        
        
        //Refresh QBO
        private bool RefreshQBO(QBOAccess qboAccess)
        {
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.Offline = false;
            connString.OAuthClientId = qboAccess.ClientId;
            connString.OAuthClientSecret = qboAccess.ClientSecret;
            connString.CompanyId = qboAccess.Company;
            connString.OAuthRefreshToken = qboAccess.RefreshToken;
            connString.OAuthVersion = "2.0";
            connString.UseSandbox = true;
            connString.InitiateOAuth = "GETANDREFRESH";
            connString.Logfile = "c:\\users\\public\\documents\\rssApiLog.txt";
            connString.Verbosity = "5";

            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
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
                                //will need some error processing
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Response.Clear();
                //await Response.WriteAsync(ex.Message + ex.InnerException);
                return false;
            }
            return true;
        }

        // Get Customers
        private bool GetQBOCustomers(QBOAccess qboAccess)
        {
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.Offline = false;
            connString.OAuthAccessToken = appOauthAccessToken;
            connString.OAuthClientId = qboAccess.ClientId;
            connString.OAuthClientSecret = qboAccess.ClientSecret;
            connString.CompanyId = qboAccess.Company;
            connString.OAuthVersion = "2.0";
            connString.UseSandbox = true;
            //connString.InitiateOAuth = "GETANDREFRESH";

            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    using (QuickBooksOnlineCommand cmdQBO = new QuickBooksOnlineCommand("Select * FROM Customers", connQBO))
                    {
                        using (QuickBooksOnlineDataReader reader = cmdQBO.ExecuteReader())
                        {
                            int colIndex = 0;
                            string GName;
                            string FName;
                            string Suf;
                            string DName;
                            string CName;
                            string PPhone;
                            string MPhone;
                            string PEmail;
                            string Nte;

                            while (reader.Read())
                            {
                                if (Int32.TryParse((string)reader["Id"], out int CId))
                                {
                                }
                                else
                                {
                                    continue;
                                }
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
                                colIndex = reader.GetOrdinal("Notes");
                                Nte = Validate.SafeGetString(reader, colIndex);
                                customerList.Add(new Customer
                                {
                                    CustomerId = CId,
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
                                    Notes = Nte,
                                    SubscriberId = subscriberId
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Error Processing
                return false;
            }
            foreach (var cust in customerList)
            {
                Customer customer = _customerRepo.GetByID(subscriberId, cust.CustomerId);
                if (customer == null)
                {
                    // customer not found, add it
                    var result = _customerRepo.AddCustomer(cust);
                    if (result == false) return false;
                    // Get any invoices this customer may have
                    result = GetInvoices(cust, connString);
                    if (result == false) return false;
                }
                else
                {
                    // we found a customer update it
                    var result = _customerRepo.UpdateCustomer(cust.CustomerId, cust);
                    if (result == false) return false;
                    // Get any invoices this customer may have
                    result = GetInvoices(cust, connString);
                    if (result == false) return false;
                }
            }
            return true;
        }

        //Get Customer Invoices
        private bool GetInvoices(Customer customer, QuickBooksOnlineConnectionStringBuilder connString)
        {
            // Need to clear the list
            invoiceList = new List<Invoice>();
            try
            {
                using (QuickBooksOnlineConnection connInv = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    using (QuickBooksOnlineCommand cmdInv = new QuickBooksOnlineCommand("Select * FROM Invoices WHERE CustomerRef = " + customer.CustomerId, connInv))
                    {
                        using (QuickBooksOnlineDataReader reader = cmdInv.ExecuteReader())
                        {
                            bool addInvoice = true;
                            string CustIId;
                            int colIndex = 0;
                            string IDNbr;
                            DateTime IDate;
                            DateTime IDueDate;
                            Decimal ITotalAmt;
                            Decimal IBalance;
                            string ITxns;
                            DateTime ILastPymtDate = DateTime.MaxValue;
                            DateTime ILastReminder = DateTime.MaxValue;

                            while (reader.Read())
                            {

                                if (Int32.TryParse((string)reader["Id"], out int IId))
                                {
                                }
                                else
                                {
                                    continue;
                                }
                                colIndex = reader.GetOrdinal("CustomerRef");
                                CustIId = Validate.SafeGetString(reader, colIndex);
                                colIndex = reader.GetOrdinal("DocNumber");
                                IDNbr = reader.GetString(colIndex);
                                IDate = reader.GetDateTime("TxnDate");
                                IDueDate = reader.GetDateTime("DueDate");
                                ITotalAmt = reader.GetDecimal("TotalAmt");
                                IBalance = reader.GetDecimal("Balance");
                                colIndex = reader.GetOrdinal("LinkedTxnAggregate");
                                ITxns = Validate.SafeGetString(reader, colIndex);
                                //Filter Invoices to keep
                                addInvoice = true;
                                if (IBalance == 0)
                                {
                                    // Get the last payment date
                                    XmlDocument xDoc = new XmlDocument();
                                    // Convert string to stream
                                    byte[] byteArray = Encoding.ASCII.GetBytes(ITxns);
                                    MemoryStream stream = new MemoryStream(byteArray);
                                    xDoc.Load(stream);
                                    XmlNodeList xnList = xDoc.SelectNodes("/LinkedTxnAggregate/Row");
                                    foreach (XmlNode xn in xnList)
                                    {
                                        string txnId = xn["TxnId"].InnerXml;
                                        string txnType = xn["TxnType"].InnerXml;
                                        if (txnType == "Payment")
                                        {
                                            DateTime txnDate = GetPymtDate(txnId, connString);
                                            DateTime now = DateTime.UtcNow;
                                            int monthDiff = GetMonthDifference(now, txnDate);
                                            if (monthDiff < 6)
                                            {
                                                ILastPymtDate = txnDate;
                                            }
                                            else
                                            {
                                                addInvoice = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                // Don't add the invoice
                                if (addInvoice == false) break;

                                invoiceList.Add(new Invoice
                                {
                                    InvoiceId = IId,
                                    CustomerId = Convert.ToInt32(CustIId),
                                    InvDocNbr = IDNbr,
                                    InvDate = IDate,
                                    InvDueDate = IDueDate,
                                    InvTotalAmt = ITotalAmt,
                                    InvBalance = IBalance,
                                    InvTxns = ITxns,
                                    InvLastPymtDate = ILastPymtDate,
                                    InvLastReminder = ILastReminder
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Error Processing
                string txnResult = ex.Message.ToString();
                return false;
            }

            foreach (var inv in invoiceList)
            {
                Invoice invoice = _invoiceRepo.GetByID(inv.InvoiceId);
                if (invoice == null)
                {
                    // Need to add a invoice record
                    var result = _invoiceRepo.AddInvoice(inv);
                    if (result == false) return false;
                }
                else
                {
                    // Need to update invoice
                    var result = _invoiceRepo.UpdateInvoice(inv.InvoiceId, inv);
                    if (result == false) return false;
                }
            }
            return true;
        }

        //Get Invoice Payments

        private DateTime GetPymtDate(string txnId, QuickBooksOnlineConnectionStringBuilder connString)
        {
            DateTime toDay = DateTime.Now;
            try
            {
                using (QuickBooksOnlineConnection connPymt = new QuickBooksOnlineConnection(connString.ToString()))
                {
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
            catch
            {
                // Error Processing
                return toDay;
            }
        }

        private static int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
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
                string txnError = ex.Message.ToString();
                return string.Empty;
            }
            
        }
    }

}