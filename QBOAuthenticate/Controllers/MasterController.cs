﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QBOAuthenticate.Models;
using QBOAuthenticate.Repositories;
using QBOAuthenticate.Repositories.Interfaces;
using System.Data.CData.QuickBooksOnline;
using System.Xml;
using System.Text;
using System.IO;
using System.Data;
using LoggerService;

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
        private readonly ICustomerRepository _customerRepo;
        private readonly IQBOAccessRepository _qboaccessRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private ILoggerManager _logger;

        public MasterController(ICustomerRepository customerRepo, IQBOAccessRepository qboaccessRepo, IInvoiceRepository invoiceRepo, ILoggerManager logger)
        {
            _customerRepo = customerRepo;
            _qboaccessRepo = qboaccessRepo;
            _invoiceRepo = invoiceRepo;
            _logger = logger;
        }

        // GET: api/master/beginauthorize/id
        [HttpGet("{id}", Order = 1)]
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
            String callbackURL = "http://localhost:1339/api/master/finalauthorize";

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
                                _logger.LogError("Subscriber " + id + "-No Authorization URL available");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Subscriber " + id + " " + ex.Message);
                return false;
            }
            _logger.LogInfo("End BeginAuthorize for Subscriber " + id);
            return true;
        }

        // GET: api/master
        [HttpGet(Order = 2)]
        public ActionResult<bool> FinalAuthorize()
        {
            string verifierToken = "";
            int sid = (int)HttpContext.Session.GetInt32("subscriberId");
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
            string callbackURL = "http://localhost:1339/api/master/finalauthorize";

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
                                _logger.LogError("Subscriber " + sid + "-No OAuthRefreshToken available");
                                return false;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Subscriber " + sid + " " + ex.Message);
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