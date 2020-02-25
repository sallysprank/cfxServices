using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataServices.Models;

namespace DataServices.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Customer GetByID(int subid, string id);
        IEnumerable<Customer> GetAllCustomers();
        int AddCustomer(Customer ourCustomer);
        bool UpdateCustomer(Customer ourCustomer);
        bool DeleteCustomer(int id);
    }
}
