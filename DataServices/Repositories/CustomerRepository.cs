using Dapper;
using DataServices.Repositories.Interfaces;
using DataServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataServices.Repositories
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

        public Customer GetByID(int subscriberId, string qbcustomerId)
        {
            using (IDbConnection conn = Connection)
            {
                var parameters = new { SubscriberId = subscriberId, QBCustomerId = qbcustomerId };
                string sQuery = "SELECT CustomerId FROM Customers WHERE SubscriberId = @SubscriberId AND QBCustomerId = @QBCustomerId";
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
                string sQuery = "SELECT CustomerId, GivenName, FamilyName, CompanyName, PrimaryEmailAddress FROM Customers";
                conn.Open();
                customer = conn.Query<Customer>(sQuery);
                return customer;
            }

        }

        public int AddCustomer(Customer ourCustomer)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = "INSERT INTO " +
                    "Customers ([QBCustomerId],[Title],[GivenName],[FamilyName],[Suffix],[DisplayName],[CompanyName],[Active],[PrimaryPhone],[MobilePhone],[PrimaryEmailAddress],[Balance],[Notes],[SubscriberId],[SendAutoReminder]) " +
                    "OUTPUT INSERTED.CustomerId " +
                    "VALUES (@QBCustomerId,@Title,@GivenName,@FamilyName,@Suffix,@DisplayName,@CompanyName,@Active,@PrimaryPhone,@MobilePhone,@PrimaryEmailAddress,@Balance,@Notes,@SubscriberId,@SendAutoReminder)";
                conn.Open();
                int result = (int)conn.ExecuteScalar(@sQuery, new
                {
                    QBCustomerId = ourCustomer.QBCustomerId,
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
                    SubscriberId = ourCustomer.SubscriberId,
                    SendAutoReminder = ourCustomer.SendAutoReminder
                });
                return result;
                //if (result > 0)
                //{
                //    return true;
                //}
                //return false;
            }
        }

        public bool UpdateCustomer(Customer ourCustomer)
        {
            using (IDbConnection conn = Connection)
            {
                string sQuery = @"UPDATE Customers SET [Title] = @Title,[GivenName] = @GivenName,[FamilyName] = @FamilyName," +
                    "[Suffix] = @Suffix,[DisplayName] = @DisplayName,[CompanyName] = @CompanyName," +
                    "[Active] = @Active,[PrimaryPhone] = @PrimaryPhone,[MobilePhone] = @MobilePhone," +
                    "[PrimaryEmailAddress] = @PrimaryEmailAddress,[Balance] = @Balance " +
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
                string sQuery = "DELETE Customers WHERE [CustomerId] = " + id;
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
