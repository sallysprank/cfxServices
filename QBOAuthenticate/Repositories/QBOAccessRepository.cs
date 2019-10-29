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
    public class QBOAccessRepository : IQBOAccessRepository
    {
        private readonly IConfiguration _config;

        public QBOAccessRepository(IConfiguration config)
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

        public bool AddQBOAccess(string clientId, string clientSecret, string companyId, string accessToken, string refreshToken, int subscriberId)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    string sQuery = "INSERT " +
                        "QBOAccess ([ClientId],[ClientSecret],[Company],[AccessToken],[RefreshToken],[SubscriberId]) " +
                        "VALUES (@ClientId,@ClientSecret,@Company,@AccessToken,@RefreshToken,@SubscriberId)";
                    conn.Open();
                    var result = conn.Execute(@sQuery, new
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        Company = companyId,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        SubscriberId = subscriberId
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

        public bool CheckExists(int id)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    conn.ExecuteScalar<bool>("select count(1) from QBOAccess where Id=@id", new { id });
                }
            }
            catch (Exception ex)
            {
                string addError = ex.Message.ToString();
                return false;
            }
            return true;
        }
    }
}
