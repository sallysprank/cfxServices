using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class ErrorLog
    {
        [Key]
        public int ErrorId { get; set; }

        [Required]
        public int SubscriberId { get; set; }

        [StringLength(500)]
        public string DisplayName { get; set; }

        [StringLength(int.MaxValue)]
        public string InvDocNbr { get; set; }

        [StringLength(1000)]
        public string ErrorMessage { get; set; }

        [StringLength(100)]
        public string MethodName { get; set; }

        [StringLength(100)]
        public string ServiceName { get; set; }

        public DateTime ErrorDateTime { get; set; }

    }
}
