using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DaPlatform.Dtos
{
    public class MembershipTypeDto
    {
        public byte ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}