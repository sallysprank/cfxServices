using Dapper;
using QBODataCollect.Repositories.Interfaces;
using QBODataCollect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QBODataCollect.Repositories
{
    public class SubscriberRepository : ISubscriberRepository
    {
        private readonly IConfiguration _config;

        public SubscriberRepository(IConfiguration config)
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

        public IEnumerable<Subscriber> GetById(int id)
        {
            IEnumerable<Subscriber> subscriber;
            using (IDbConnection conn = Connection)
            {
                var parameters = new { Id = id };
                string sQuery = "SELECT Id FROM Subscriber WHERE Id = @Id";
                conn.Open();
                subscriber = conn.Query<Subscriber>(sQuery, parameters);
                return subscriber;
            }

        }

        public IEnumerable<Subscriber> GetAllSubscribers()
        {
            IEnumerable<Subscriber> subscriber;
            using (IDbConnection conn = Connection)
            {
                string sQuery = "SELECT Id FROM Subscriber";
                conn.Open();
                subscriber = conn.Query<Subscriber>(sQuery);
                return subscriber;
            }

        }
    }
}