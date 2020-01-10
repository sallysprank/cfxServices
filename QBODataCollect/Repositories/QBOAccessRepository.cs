using Dapper;
using QBODataCollect.Repositories.Interfaces;
using QBODataCollect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.DataProtection;
using System.Data;
using System.Data.SqlClient;

namespace QBODataCollect.Repositories
{
    public class QBOAccessRepository : IQBOAccessRepository
    {
        private readonly IConfiguration _config;
        private readonly IDataProtector _protector;

        public QBOAccessRepository(IConfiguration config, IDataProtectionProvider provider)
        {
            _config = config;
            _protector = provider.CreateProtector("cfxpert_qbo_access");
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnectionString"));
            }
        }
        public QBOAccess GetById(int sId)
        {
            string sQuery = "";

            // Get the QBO credentials
            using (IDbConnection conn = Connection)
            {
                var @params = new { SubscriberId = sId };
                sQuery = "SELECT * FROM QBOAccess WHERE SubscriberId = @SubscriberId";
                conn.Open();
                var result = conn.Query<QBOAccess>(sQuery, @params);
                return result.FirstOrDefault();
            }
        }

        public bool UpdateQBOAccess(int id, string accessToken, string refreshToken, QBOAccess ourQBOAccess)
        {
            try
            {
                if (id != ourQBOAccess.Id)
                {
                    return false;
                }
                using (IDbConnection conn = Connection)
                {
                    string sQuery = @"UPDATE [dbo].[QBOAccess] SET [RefreshToken] = @refreshToken, [AccessToken] = @accessToken " +
                        "WHERE Id = @id";
                    conn.Open();
                    var results = conn.Execute(@sQuery, new { id, refreshToken, accessToken });
                    if (results > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                var exResult = ex.Message;
                return false;
            }

        }


    }
}
