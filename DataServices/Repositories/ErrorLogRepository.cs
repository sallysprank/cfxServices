using Dapper;
using DataServices.Models;
using DataServices.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataServices.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IConfiguration _config;

        public ErrorLogRepository(IConfiguration config)
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

        public void InsertErrorLog(ErrorLog errorLog)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = "INSERT INTO " +
                    "ErrorLogs ([SubscriberId],[DisplayName],[InvDocNbr],[ErrorMessage],[MethodName],[ServiceName],[ErrorDateTime]) " +
                    "VALUES (@SubscriberId,@DisplayName,@InvDocNbr,@ErrorMessage,@MethodName,@ServiceName,@ErrorDateTime)";
                conn.Open();
                conn.Execute(@sQuery, new
                {
                    SubscriberId = errorLog.SubscriberId,
                    DisplayName = errorLog.DisplayName,
                    InvDocNbr = errorLog.InvDocNbr,
                    ErrorMessage = errorLog.ErrorMessage,
                    MethodName = errorLog.MethodName,
                    ServiceName = errorLog.ServiceName,
                    ErrorDateTime = errorLog.ErrorDateTime
                });
            }
        }
    }
}
