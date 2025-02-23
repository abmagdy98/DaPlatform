using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DaPlatform.Models
{
    public class ImageUser
    {
        [Key, Column(Order = 0)]
        [Required]
        [Display(Name = "Username")]
        public string userName { get; set; }

        [Key, Column(Order = 1)]
        [Required]
        [Display(Name = "Image ID")]
        public string imageID { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string userFullName { get; set; }

        [Required]
        [Display(Name = "Image Name")]
        public string imageName { get; set; }

        [Required]
        [Display(Name ="Image Private Key")]
        public string img_privateKey_encrypted { get; set; }
    }
}