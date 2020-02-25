using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataServices.Models;

namespace DataServices.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetByID(int cid, string qbIId);
        IEnumerable<Invoice> GetAllInvoices();
        bool AddInvoice(Invoice ourInvoice);
        bool UpdateInvoice(Invoice ourInvoice);
        bool DeleteInvoice(int id);
    }
}

