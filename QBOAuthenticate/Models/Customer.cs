using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QBOAuthenticate.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string QBCustomerId { get; set; }

        [StringLength(16)]
        public string Title { get; set; }

        [StringLength(100)]
        public string GivenName { get; set; }

        [StringLength(100)]
        public string FamilyName { get; set; }

        [StringLength(16)]
        public string Suffix { get; set; }

        [StringLength(500)]
        public string DisplayName { get; set; }

        [StringLength(100)]
        public string CompanyName { get; set; }

        public Boolean Active { get; set; }

        [StringLength(30)]
        public string PrimaryPhone { get; set; }

        [StringLength(30)]
        public string MobilePhone { get; set; }

        [StringLength(100)]
        public string PrimaryEmailAddress { get; set; }

        public decimal Balance { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        public int SubscriberId { get; set; }
    }
}
