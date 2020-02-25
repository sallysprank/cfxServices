using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataServices.Models;

namespace DataServices.Repositories.Interfaces
{
    public interface ISubscriberRepository
    {
        IEnumerable<Subscriber> GetById(int id);
        IEnumerable<Subscriber> GetAllSubscribers();
    }
}
