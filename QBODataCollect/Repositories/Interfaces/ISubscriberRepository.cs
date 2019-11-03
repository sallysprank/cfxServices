using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBODataCollect.Models;

namespace QBODataCollect.Repositories.Interfaces
{
    public interface ISubscriberRepository
    {
        IEnumerable<Subscriber> GetById(int id);
        IEnumerable<Subscriber> GetAllSubscribers();
    }
}
