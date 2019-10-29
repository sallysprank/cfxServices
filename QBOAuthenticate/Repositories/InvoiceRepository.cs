using Dapper;
using QBOAuthenticate.Repositories.Interfaces;
using QBOAuthenticate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QBOAuthenticate.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IConfiguration _config;

        public InvoiceRepository(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnectionString"));
            }
        }

        public Invoice GetByID(int invoiceId)
        {
            using (IDbConnection conn = Connection)
            {
                var @params = new { InvoiceId = invoiceId };
                string sQuery = "SELECT invoiceId FROM Invoice WHERE InvoiceId = @InvoiceId";
                conn.Open();
                var result = conn.Query<Invoice>(sQuery, @params);
                return result.FirstOrDefault();
            }

        }

        public IEnumerable<Invoice> GetAllInvoices()
        {
            IEnumerable<Invoice> invoice;
            using (IDbConnection conn = Connection)
            {
                string sQuery = "SELECT InvoiceId FROM Invoice";
                conn.Open();
                invoice = conn.Query<Invoice>(sQuery);
                return invoice;
            }

        }

        public bool AddInvoice(Invoice ourInvoice)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string sQuery = "INSERT " +
                        "Invoice ([InvoiceId],[CustomerId],[InvDocNbr],[InvDate],[InvDueDate],[InvTotalAmt],[InvBalance],[InvTxns],[InvLastPymtDate],[InvLastReminder]) " +
                        "VALUES (@InvoiceId,@CustomerId,@InvDocNbr,@InvDate,@InvDueDate,@InvTotalAmt,@InvBalance,@InvTxns,@InvLastPymtDate,@InvLastReminder)";
                    conn.Open();
                    var result = conn.Execute(@sQuery, new
                    {
                        InvoiceId = ourInvoice.InvoiceId,
                        CustomerId = ourInvoice.CustomerId,
                        InvDocNbr = ourInvoice.InvDocNbr,
                        InvDate = ourInvoice.InvDate,
                        InvDueDate = ourInvoice.InvDueDate,
                        InvTotalAmt = ourInvoice.InvTotalAmt,
                        InvBalance = ourInvoice.InvBalance,
                        InvTxns = ourInvoice.InvTxns,
                        InvLastPymtDate = ourInvoice.InvLastPymtDate,
                        InvLastReminder = ourInvoice.InvLastReminder
                    });
                    if (result > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                string addError = ex.Message.ToString();
                return false;
            }

        }

        public bool UpdateInvoice(int id, Invoice ourInvoice)
        {
            if (id != ourInvoice.InvoiceId)
            {
                return false;
            }
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string sQuery = @"UPDATE Invoice SET [CustomerId] = @CustomerId,[InvDocNbr] = @InvDocNbr,[InvDate] = @InvDate," +
                            "[InvDueDate] = @InvDueDate,[InvTotalAmt] = @InvTotalAmt,[InvBalance] = @InvBalance," +
                            "[InvTxns] = @InvTxns,[InvLastPymtDate] = @InvLastPymtDate,[InvLastReminder] = @InvLastReminder " +
                            "WHERE [InvoiceId] = @InvoiceId";
                    conn.Open();
                    var results = conn.Execute(@sQuery, ourInvoice);
                    if (results > 0)
                    {
                        return true;
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                string updateError = ex.Message.ToString();
                return false;
            }

        }

        public bool DeleteInvoice(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = "DELETE Invoice WHERE [InvoiceId] = " + id;
                conn.Open();
                var results = conn.Execute(@sQuery, new { InvoiceId = id });
                if (results > 0)
                {
                    return true;
                }
                return false;
            }
        }

    }
}