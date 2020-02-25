using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        [StringLength(50)]
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

        [StringLength(1000)]
        public string Notes { get; set; }

        public int SubscriberId { get; set; }

        [StringLength(100)]
        public string AltGivenName { get; set; }

        [StringLength(100)]
        public string AltFamilyName { get; set; }

        [StringLength(30)]
        [DataType(DataType.PhoneNumber)]
        public string AltPrimaryPhone { get; set; }

        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public string AltPrimaryEmailAddress { get; set; }

        public bool SendAutoReminder { get; set; }

    }
}
