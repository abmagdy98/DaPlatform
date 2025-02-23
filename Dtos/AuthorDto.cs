using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DaPlatform.Dtos
{
    public class AuthorDto
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
    }
}