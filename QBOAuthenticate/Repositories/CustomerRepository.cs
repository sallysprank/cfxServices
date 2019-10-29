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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IConfiguration _config;

        public CustomerRepository(IConfiguration config)
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

        public Customer GetByID(int subscriberId, int customerId)
        {
            using (IDbConnection conn = Connection)
            {
                var parameters = new { SubscriberId = subscriberId, CustomerId = customerId };
                string sQuery = "SELECT CustomerId FROM Customer WHERE SubscriberId = @SubscriberId AND CustomerId = @CustomerId";
                conn.Open();
                var result = conn.Query<Customer>(sQuery, parameters);
                return result.FirstOrDefault();
            }

        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            IEnumerable<Customer> customer;
            using (IDbConnection conn = Connection)
            {
                string sQuery = "SELECT CustomerId, GivenName, FamilyName, CompanyName, PrimaryEmailAddress FROM Customer";
                conn.Open();
                customer = conn.Query<Customer>(sQuery);
                return customer;
            }

        }

        public bool AddCustomer(Customer ourCustomer)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = "INSERT " +
                    "Customer ([CustomerId],[Title],[GivenName],[FamilyName],[Suffix],[DisplayName],[CompanyName],[Active],[PrimaryPhone],[MobilePhone],[PrimaryEmailAddress],[Balance],[Notes],[SubscriberId]) " +
                    "VALUES (@CustomerId,@Title,@GivenName,@FamilyName,@Suffix,@DisplayName,@CompanyName,@Active,@PrimaryPhone,@MobilePhone,@PrimaryEmailAddress,@Balance,@Notes,@SubscriberId)";
                conn.Open();
                var result = conn.Execute(@sQuery, new
                {
                    CustomerId = ourCustomer.CustomerId,
                    Title = ourCustomer.Title,
                    GivenName = ourCustomer.GivenName,
                    FamilyName = ourCustomer.FamilyName,
                    Suffix = ourCustomer.Suffix,
                    DisplayName = ourCustomer.DisplayName,
                    CompanyName = ourCustomer.CompanyName,
                    Active = ourCustomer.Active,
                    PrimaryPhone = ourCustomer.PrimaryPhone,
                    MobilePhone = ourCustomer.MobilePhone,
                    PrimaryEmailAddress = ourCustomer.PrimaryEmailAddress,
                    Balance = ourCustomer.Balance,
                    Notes = ourCustomer.Notes,
                    ourCustomer.SubscriberId
                });
                if (result > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool UpdateCustomer(int id, Customer ourCustomer)
        {
            if (id != ourCustomer.CustomerId)
            {
                return false;
            }
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"UPDATE Customer SET [Title] = @Title,[GivenName] = @GivenName,[FamilyName] = @FamilyName," +
                    "[Suffix] = @Suffix,[DisplayName] = @DisplayName,[CompanyName] = @CompanyName," +
                    "[Active] = @Active,[PrimaryPhone] = @PrimaryPhone,[MobilePhone] = @MobilePhone," +
                    "[PrimaryEmailAddress] = @PrimaryEmailAddress,[Balance] = @Balance,[Notes] = @Notes,[SubscriberId] = @SubscriberId " +
                    "WHERE [CustomerId] = @CustomerId";
                conn.Open();
                var results = conn.Execute(@sQuery, ourCustomer);
                if (results > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool DeleteCustomer(int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = "DELETE Customer WHERE [CustomerId] = " + id;
                conn.Open();
                var results = conn.Execute(@sQuery, new { CustomerId = id });
                if (results > 0)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
