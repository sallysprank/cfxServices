using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataServices.Models;

namespace DataServices.Repositories.Interfaces
{
    public interface IQBOAccessRepository
    {
        QBOAccess GetById(int sId);
        bool UpdateQBOAccess(int id, string companyId, string accessToken, string refreshToken, QBOAccess ourQBOAccess);
        bool AddQBOAccess(string clientId, string clientSecret, string companyId, string accessToken, string refreshToken, int subscriberId);
        bool DeleteQBOAccess(int subscriberId);
        bool CheckExists(int subscriberId);
    }
}
