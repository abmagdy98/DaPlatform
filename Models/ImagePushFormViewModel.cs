using System.ComponentModel.DataAnnotations;

namespace DaPlatform.Models
{
    public class ImagePushFormViewModel
    {
        /*
        [Required]
        [Display(Name = "Image ID")]
        public string ID { get; set; }
        */

        [Required]
        [Display(Name = "Image Repository [:with tag]")]
        public string Name { get; set; }

        //[Required]
        //[Display(Name = "Your Private Key")]
        //public string userPrivateKey { get; set; }

        public ImagePushFormViewModel()
        {

        }

        public ImagePushFormViewModel(Image image)
        {
            this.Name = image.Name;
        }
    }
}