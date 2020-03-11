using DataServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataServices.Repositories.Interfaces
{
    public interface IAspNetUserRepository
    {
        AspNetUsers GetUserDetailsByIdPassword(string userName, string password);
    }
}
