using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class AspNetUsers
    {
        [Key]
        public string Id { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(256)]
        public string NormalizedUserName { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        [StringLength(256)]
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        [StringLength(int.MaxValue)]
        public string PasswordHash { get; set; }
        [StringLength(int.MaxValue)]
        public string SecurityStamp { get; set; }
        [StringLength(int.MaxValue)]
        public string ConcurrencyStamp { get; set; }
        [StringLength(int.MaxValue)]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        [StringLength(int.MaxValue)]
        public string Discriminator { get; set; }
        [StringLength(int.MaxValue)]
        public string FirstName { get; set; }
        [StringLength(int.MaxValue)]
        public string LastName { get; set; }
        public int SubscriberId { get; set; }
    }
}
