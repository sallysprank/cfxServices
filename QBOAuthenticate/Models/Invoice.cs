using System;
using System.ComponentModel.DataAnnotations;

namespace QBOAuthenticate.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string QBInvoiceId { get; set; }
        public int CustomerId { get; set; }
        public string InvDocNbr { get; set; }
        public DateTime InvDate { get; set; }
        public DateTime InvDueDate { get; set; }
        public Decimal InvTotalAmt { get; set; }
        public Decimal InvBalance { get; set; }
        public String InvTxns { get; set; }
        public DateTime InvLastPymtDate { get; set; }
        public DateTime InvLastReminder { get; set; }
        [StringLength(1000)]
        public string Notes { get; set; }
        public bool SendAutoReminder { get; set; }
    }
}
