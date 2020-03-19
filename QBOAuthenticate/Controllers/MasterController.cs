using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataServices.Models;
using DataServices.Repositories;
using DataServices.Repositories.Interfaces;
//using QBOAuthenticate.Models;
//using QBOAuthenticate.Repositories;
//using QBOAuthenticate.Repositories.Interfaces;
using System.Data.CData.QuickBooksOnline;
using System.Xml;
using System.Text;
using System.IO;
using System.Data;
using LoggerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace QBOAuthenticate.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private const string appClientId = "Q00xm3vqx90O704ifsLc2UZ2LTbbXvTx0LQHtNdDKQPHBxcHi0";
        private const string appClientSecret = "ANw9tDlZDVsXkEBt6ZrarGDrWjLHjRsGUtrc8wiv";
        private string appOauthAccessToken = "";
        private string appOauthRefreshToken = "";
        private string companyId = "";
        private readonly string serviceName = "";
        private string currentMethodName = "";
        private readonly ICustomerRepository _customerRepo;
        private readonly IQBOAccessRepository _qboaccessRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IErrorLogRepository _errorLogRepo;
        private ILoggerManager _logger;
        private IMemoryCache _cache;
        protected IConfiguration _configuration;
        public int subscriberId { get; set; }

        public MasterController(ICustomerRepository customerRepo, IQBOAccessRepository qboaccessRepo, IInvoiceRepository invoiceRepo, ILoggerManager logger, IErrorLogRepository errorLogRepo, IMemoryCache cache, IConfiguration configuration)
        {
            _customerRepo = customerRepo;
            _qboaccessRepo = qboaccessRepo;
            _invoiceRepo = invoiceRepo;
            _errorLogRepo = errorLogRepo;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
            serviceName = GetType().Namespace.Substring(0, GetType().Namespace.IndexOf('.'));
        }

        // GET: api/master/beginauthorize/id
        [HttpGet("{id}", Order = 1)]
        [Authorize]
        public ActionResult<bool> BeginAuthorize(int id)
        {
            _logger.LogInfo("Start BeginAuthorize for Subscriber " + id);
            HttpContext.Session.SetInt32("subscriberId", id); // set the session subscriber id
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.Offline = false;
            connString.OAuthClientId = appClientId;
            connString.OAuthClientSecret = appClientSecret;
            connString.UseSandbox = true;
            connString.Logfile = "c:\\users\\public\\documents\\QBOLog.txt";
            connString.Verbosity = "5";
            String callbackURL = _configuration["CFXServiceConfiguration:AuthanticateServiceEndPoint"] + "api/master/finalauthorize";
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();


            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    using (QuickBooksOnlineCommand cmdQBO = new QuickBooksOnlineCommand("GetOAuthAuthorizationURL", connQBO))
                    {
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("CallbackURL", callbackURL));
                        cmdQBO.CommandType = CommandType.StoredProcedure;

                        using (QuickBooksOnlineDataReader reader = cmdQBO.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Response.Redirect(reader["URL"] as string, false);
                            }
                            else
                            {
                                _errorLogRepo.InsertErrorLog(new ErrorLog
                                {
                                    SubscriberId = id,
                                    ErrorMessage = "No Authorization URL available",
                                    ServiceName = serviceName,
                                    MethodName = currentMethodName,
                                    ErrorDateTime = DateTime.Now
                                });
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = id,
                    ErrorMessage = ex.Message,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return false;
            }
            _logger.LogInfo("End BeginAuthorize for Subscriber " + id);
            return true;
        }

        [HttpGet("{id}", Order = 1)]
        [Authorize]
        public ActionResult<string> BeginAuthorizeForCFExpert(int id)
        {
            _logger.LogInfo("Start BeginAuthorize for Subscriber " + id);
            _cache.Set("subscriberId", id);
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.Offline = false;
            connString.OAuthClientId = appClientId;
            connString.OAuthClientSecret = appClientSecret;
            connString.UseSandbox = true;
            connString.Logfile = "c:\\users\\public\\documents\\QBOLog.txt";
            connString.Verbosity = "5";
            String callbackURL = _configuration["CFXServiceConfiguration:AuthanticateServiceEndPoint"] + "api/master/finalauthorize";
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();
            subscriberId = id;

            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    using (QuickBooksOnlineCommand cmdQBO = new QuickBooksOnlineCommand("GetOAuthAuthorizationURL", connQBO))
                    {
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("CallbackURL", callbackURL));
                        cmdQBO.CommandType = CommandType.StoredProcedure;

                        using (QuickBooksOnlineDataReader reader = cmdQBO.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader["URL"].ToString();
                            }
                            else
                            {
                                _errorLogRepo.InsertErrorLog(new ErrorLog
                                {
                                    SubscriberId = id,
                                    ErrorMessage = "No Authorization URL available",
                                    ServiceName = serviceName,
                                    MethodName = currentMethodName,
                                    ErrorDateTime = DateTime.Now
                                });
                                return "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = id,
                    ErrorMessage = ex.Message,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return "";
            }
        }

        // GET: api/master
        [HttpGet(Order = 2)]
        public ActionResult<bool> FinalAuthorize()
        {
            string verifierToken = "";
            int sid;
            try
            {
                sid = (int)HttpContext.Session.GetInt32("subscriberId");
            }
            catch(Exception)
            {
                sid = _cache.Get<int>("subscriberId");
            }
            // will delete this
            //var qboQueryString = Request.Query;
            //foreach (var qbItem in qboQueryString.Keys)
            //{
            //    System.Diagnostics.Debug.WriteLine("Key: " + qbItem + ", Value: " + Request.Query[qbItem]);
            //}
            _logger.LogInfo("Start FinalAuthorize for Subscriber " + sid);
            string qboCode = Request.Query["code"];
            if (qboCode == null)
                return false;
            verifierToken = qboCode;
            companyId = Request.Query["realmid"];
            var connString = new QuickBooksOnlineConnectionStringBuilder();
            connString.OAuthClientId = appClientId;
            connString.OAuthClientSecret = appClientSecret;
            connString.UseSandbox = true;
            connString.Logfile = "c:\\users\\public\\documents\\QBOLog.txt";
            connString.Verbosity = "5";
            string callbackURL = _configuration["CFXServiceConfiguration:AuthanticateServiceEndPoint"] + "api/master/finalauthorize";
            currentMethodName = this.ControllerContext.RouteData.Values["action"].ToString();

            try
            {
                using (QuickBooksOnlineConnection connQBO = new QuickBooksOnlineConnection(connString.ToString()))
                {
                    using (QuickBooksOnlineCommand cmdQBO = new QuickBooksOnlineCommand("GetOAuthAccessToken", connQBO))
                    {
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("Authmode", "WEB"));
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("Verifier", verifierToken));
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("CompanyId", companyId));
                        cmdQBO.Parameters.Add(new QuickBooksOnlineParameter("CallbackURL", callbackURL));
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
                                    SubscriberId = sid,
                                    ErrorMessage = "No OAuthRefreshToken available",
                                    ServiceName = serviceName,
                                    MethodName = currentMethodName,
                                    ErrorDateTime = DateTime.Now
                                });
                                return false;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogRepo.InsertErrorLog(new ErrorLog
                {
                    SubscriberId = sid,
                    ErrorMessage = ex.Message,
                    ServiceName = serviceName,
                    MethodName = currentMethodName,
                    ErrorDateTime = DateTime.Now
                });
                return false;
            }
            // Get/Update our QBOAccess record
            bool bRtn;
            QBOAccess qboAccess = _qboaccessRepo.GetById(sid);
            if (qboAccess == null)
            {
                bRtn = _qboaccessRepo.AddQBOAccess(appClientId,appClientSecret, companyId, appOauthAccessToken, appOauthRefreshToken, sid);
            }
            else {
                bRtn = _qboaccessRepo.UpdateQBOAccess(qboAccess.Id, appOauthAccessToken, appOauthRefreshToken, qboAccess);
            }
            if (bRtn == true)
            {
                _logger.LogInfo("End FinalAuthorize for Subscriber " + sid);
                return true;
            }
            return false;
        }
    }

    //static class Validate
    //{
    //    public static string SafeGetString(this QuickBooksOnlineDataReader reader, int colIndex)
    //    {
    //        try
    //        {
    //            if (!reader.IsDBNull(colIndex))
    //                return reader.GetString(colIndex);
    //            return string.Empty;
    //        }
    //        catch (Exception ex)
    //        {
    //            string txnError = ex.Message.ToString();
    //            return string.Empty;
    //        }

    //    }
    //}

}