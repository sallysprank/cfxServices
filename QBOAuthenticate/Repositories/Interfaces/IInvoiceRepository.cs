﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBOAuthenticate.Models;

namespace QBOAuthenticate.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetByID(int id);
        IEnumerable<Invoice> GetAllInvoices();
        bool AddInvoice(Invoice ourInvoice);
        bool UpdateInvoice(int id, Invoice ourInvoice);
        bool DeleteInvoice(int id);
    }
}

