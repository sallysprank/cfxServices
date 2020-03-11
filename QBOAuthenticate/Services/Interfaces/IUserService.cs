using QBOAuthenticate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QBOAuthenticate.Services.Interfaces
{
    public interface IUserService
    {
        User Authenticate(Authenticate userDetails);
    }
}
