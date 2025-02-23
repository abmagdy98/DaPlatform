using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.Owin;

namespace DaPlatform.Models
{
    public class ImageFormViewModel
    {
        public List<ApplicationUser> Users { get; set; }

        [Required]
        [Display(Name = "Image ID")]
        public string ID { get; set; }

        [Required]
        [Display(Name = "Image Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Administrator privateKey")]
        public string adminPrivateKey { get; set; }

        public bool[] isAuthorized { get; set; }


        [Required]

        public string Title
        {
            get
            {
                if (ID != null)
                    return "Edit Image";

                return "New Image";
            }
        }
        public ImageFormViewModel()
        {
            Users = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.ToList();
        }
        public ImageFormViewModel(Image image)
        {
            this.ID = image.ID;
            this.Name = image.Name;
            this.Users = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.ToList();
            this.isAuthorized = new bool[Users.Count];
        }
    }
}
