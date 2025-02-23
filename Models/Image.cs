using System.ComponentModel.DataAnnotations;

namespace DaPlatform.Models
{
    public class Image
    {
        [Required]
        [Display(Name = "Image ID")]
        public string ID { get; set; }

        [Required]
        [Display(Name = "Image Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Image publicKey")]
        public string publicKey { get; set; }
    }
}
