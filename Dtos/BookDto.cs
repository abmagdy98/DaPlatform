using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DaPlatform.Dtos
{
    public class BookDto
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public GenreDto Genre { get; set; }

        [Required]
        public byte GenreID { get; set; }
        public int? PageCount { get; set; }
        public int? NumberInStock { get; set; }
        public AuthorDto Author { get; set; }

        [Required]
        public int AuthorID { get; set; }
    }
}