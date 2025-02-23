

namespace DaPlatform.Models
{
    public class ImagePullFormViewModel
    {
        public string userPrivateKey { get; set; }
        public string imageID { get; set; }

        public ImagePullFormViewModel()
        {
            
        }

        public ImagePullFormViewModel(string imageID)
        {
            this.imageID = imageID;
        }


    }
}