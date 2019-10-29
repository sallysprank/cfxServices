﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBODataCollect.Models;

namespace QBODataCollect.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Customer GetByID(int subid,int id);
        IEnumerable<Customer> GetAllCustomers();
        bool AddCustomer(Customer ourCustomer);
        bool UpdateCustomer(int id, Customer ourCustomer);
        bool DeleteCustomer(int id);
    }
}
