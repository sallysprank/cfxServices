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
    public class AspNetUserRepository : IAspNetUserRepository
    {
        private readonly IConfiguration _config;

        public AspNetUserRepository(IConfiguration config)
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

        public AspNetUsers GetUserDetailsByIdPassword(string userName, string password)
        {
            using (IDbConnection conn = Connection)
            {
                var parameters = new { UserName = userName, Password = password };
                string sQuery = "SELECT * FROM AspNetUsers WHERE UserName = @UserName AND PasswordHash = @Password";
                conn.Open();
                var result = conn.Query<AspNetUsers>(sQuery, parameters);
                return result.FirstOrDefault();
            }
        }
    }
}
