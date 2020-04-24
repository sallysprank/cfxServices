using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QBODataCollect.Models
{
    public class Subscriber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SubscriberName { get; set; }

        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(100)]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(30)]
        [DataType(DataType.PhoneNumber)]
        public string PrimaryPhone { get; set; }

        [DataType(DataType.Date)]
        public DateTime SignUpDate { get; set; }

        #region  Chargebee Related Fields
        [Required]
        public string SubscriberEmail { get; set; }

        public string SubscriptionId { get; set; }

        public string CustomerId { get; set; }
        #endregion

        //public ICollection<ApplicationUsers> Users { get; set; }

        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public string ReminderEmailAddress { get; set; }
        public Boolean SendAutoReminders { get; set; }
        public Boolean SubscriptionActive { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateofLastSync { get; set; }


    }
}

