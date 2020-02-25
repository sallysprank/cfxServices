using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataServices.Models
{
    public class QBOAccess
    {

        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string ClientId { get; set; }
        [Required, MaxLength(150)]
        public string ClientSecret { get; set; }
        [Required, MaxLength(50)]
        public string Company { get; set; }
        [Required, MaxLength(4096)]
        public string AccessToken { get; set; }
        [Required, MaxLength(1024)]
        public string RefreshToken { get; set; }
        [Required]
        public int SubscriberId { get; set; }
    }
}
