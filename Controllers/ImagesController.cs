using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DaPlatform.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace DaPlatform.Controllers
{
    public class ImagesController : Controller
    {
        public static RSACryptoServiceProvider cryptoServiceProvider;
        private readonly ApplicationDbContext _context;

        public ImagesController()
        {
            _context = new ApplicationDbContext();
        }
        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: Images
        public ActionResult Index()
        {
            if (User.IsInRole(RoleName.CanManageImages))
            {
                var imgList = _context.Images.ToList();
                return View("List",imgList);
            }

            var curentUser = System.Web.HttpContext.Current
                                 .GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web
                                 .HttpContext.Current.User.Identity.GetUserId()).UserName;
            var authorizedList = _context.ImageUser.Where(usr => usr.userName == curentUser).ToList();
            return View("AuthorizedList", authorizedList);
        }

        public ActionResult Details(string id)
        {
            var image = _context.Images.Include(I => I.ID).SingleOrDefault(I => I.ID == id);

            if (image == null)
                return HttpNotFound();

            return View(image);
        }

        [Authorize(Roles = RoleName.CanManageImages)]
        public ViewResult New()
        {
            var viewModel = new ImageFormViewModel();
            cryptoServiceProvider = new RSACryptoServiceProvider(1024);
            return View("ImageForm", viewModel);
        }

        [Authorize(Roles = RoleName.CanManageImages)]
        public ActionResult Edit(string id)
        {
            var image = _context.Images.SingleOrDefault(I => I.ID == id);

            if (image == null)
                return HttpNotFound();

            var viewModel = new ImageFormViewModel(image);

            for (int i = 0; i < viewModel.Users.Count; i++)
            {
                var imageUser = _context.ImageUser.ToList().SingleOrDefault(imgusr =>
                                                imgusr.imageID == viewModel.ID &&
                                                imgusr.userName == viewModel.Users[i].UserName);
                if (imageUser != null)
                    viewModel.isAuthorized[i] = true;
                else
                    viewModel.isAuthorized[i] = false;
            }

            return View("ImageForm", viewModel);
        }        

        [HttpPost]
        [Authorize(Roles = RoleName.CanManageImages)]
        public ActionResult Save(ImageFormViewModel imageForm)
        {
            ApplicationUserManager userManager = System.Web.HttpContext.Current.GetOwinContext().
                    GetUserManager<ApplicationUserManager>();
            ApplicationUser theAdmin = userManager.FindById("4bab27de-f8b7-4835-aba8-ad3d8187a02a");
            string imagePrivateKey;
            ImageUser imageUser;

            Image image = new Image
            {
                ID = imageForm.ID
            ,
                Name = imageForm.Name
            };
            if (!ModelState.IsValid)
            {
                var viewModel = new ImageFormViewModel(image);

                return View("ImageForm", viewModel);
            }

            var image_exist = _context.Images.SingleOrDefault(I => I.ID == image.ID);
            if (image_exist == null)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                RSA.RSA.ExportPublicKey(cryptoServiceProvider, sw);
                image.publicKey = sb.ToString();

                _context.Images.Add(image);

                sb = new StringBuilder();
                sw = new StringWriter(sb);
                RSA.RSA.ExportPrivateKey(cryptoServiceProvider, sw);
                imagePrivateKey = sb.ToString();
                
                imageUser = new ImageUser
                {
                    imageID = imageForm.ID,
                    imageName = imageForm.Name,
                    userName = theAdmin.UserName,
                    userFullName = theAdmin.FullName,
                    img_privateKey_encrypted = RSA.RSA.Encrypt(imagePrivateKey, theAdmin.pubKey)
                };
            }
            else
            {
                
                ImageUser imageAdmin = _context.ImageUser.SingleOrDefault(imgusr =>
                                            imgusr.imageID == imageForm.ID &&
                                            imgusr.userName == theAdmin.UserName);
                imagePrivateKey = RSA.RSA.Decrypt(imageAdmin.img_privateKey_encrypted, imageForm.adminPrivateKey);

                image_exist.ID = image.ID;
                image_exist.Name = image.Name;
            }
            _context.SaveChanges();


            var lst_users = imageForm.Users;
            var lst_auth = imageForm.isAuthorized;
            var result = lst_users.Zip(lst_auth, (x, y) => new Tuple<ApplicationUser, bool>(x, y))
                            .ToList();
            
            IEnumerable<ImageUser> toTheWind = _context.ImageUser.Where(imgusr => imgusr.imageID == imageForm.ID &&
                                               imgusr.userName != theAdmin.UserName);
            _context.ImageUser.RemoveRange(toTheWind);
            _context.SaveChanges();
            
            foreach (var item in result)

            {
                if (!item.Item2)
                    continue;
                else
                {
                    var alreadyAuthorized = _context.ImageUser.SingleOrDefault(imgusr =>
                                            imgusr.imageID == imageForm.ID &&
                                            imgusr.userName == item.Item1.UserName);
                    if (alreadyAuthorized == null)
                    {
                        imageUser = new ImageUser
                        {
                            imageID = imageForm.ID,
                            imageName = imageForm.Name,
                            userName = item.Item1.UserName,
                            userFullName = item.Item1.FullName,
                            img_privateKey_encrypted = RSA.RSA.Encrypt(imagePrivateKey, item.Item1.pubKey)
                    };
                        _context.ImageUser.Add(imageUser);
                    }
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Images");
        }

        public ViewResult GoToPush()
        {
            return View("ImagePush");
        }

        [HttpPost]
        public ActionResult Push(ImagePushFormViewModel imageForm, HttpPostedFileBase upload)
        {
            cryptoServiceProvider = new RSACryptoServiceProvider(1024);
            string imagePrivateKey;
            ImageUser imageUser;
            ImageUser imageAdmin = null;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            RSA.RSA.ExportPublicKey(cryptoServiceProvider, sw);

            Image image = new Image
            {
                Name = imageForm.Name
            ,
                publicKey = sb.ToString()
            };

            if (!ModelState.IsValid)
            {
                var viewModel = new ImagePushFormViewModel(image);

                return View("ImagePush", viewModel);
            }


            var image_exist = _context.Images.SingleOrDefault(I => I.Name == image.Name);
            if (image_exist != null)
            {
                var viewModel = new ImagePushFormViewModel(image);

                return View("ImagePush", viewModel);
            }
            else
            {
                sb = new StringBuilder();
                sw = new StringWriter(sb);
                RSA.RSA.ExportPrivateKey(cryptoServiceProvider, sw);
                imagePrivateKey = sb.ToString();

                string path = Path.Combine("D:/DaPlatform_Files/Server", upload.FileName);
                upload.SaveAs(path);

                Process cmd = new Process();
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardError = false;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.FileName = "cmd.exe";
                //cmd.StartInfo.Arguments = "/k cd %windir%\\Sysnative " +
                //                  "& wsl podman load -i /mnt/d/DaPlatform_Files/Server/" + upload.FileName;
                cmd.Start();
                cmd.StandardInput.WriteLine("cd %windir%\\Sysnative");
                cmd.StandardInput.WriteLine("wsl podman load -i /mnt/d/DaPlatform_Files/Server/" + upload.FileName);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                string output = cmd.StandardOutput.ReadToEnd();
                cmd.WaitForExit();
                cmd.Close();
                List<string> lines = new List<string>();
                Match match = Regex.Match(output, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);
                while (match.Success && lines.Count < 3)
                {
                    lines.Insert(0, match.Value);
                    match = match.NextMatch();
                }

                image.ID = lines[0].Remove(0, 24);
                RSA.RSA.StorePEMtoLocalFileDirectory(cryptoServiceProvider, @"D:\tempFile.pem", false);
                cmd = new Process();
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.Start();
                cmd.StandardInput.WriteLine("cd %windir%\\Sysnative");
                cmd.StandardInput.WriteLine("wsl buildah push --encryption-key jwe:/mnt/d/tempFile.pem " + image.ID + " " + imageForm.Name);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                cmd.WaitForExit();
                cmd.Close();
                System.IO.File.Delete(@"D:\tempFile.pem");
                System.IO.File.Delete(path);
            }
            _context.Images.Add(image);

            imageUser = new ImageUser
            {
                imageID = image.ID,
                imageName = image.Name,
                userName = System.Web.HttpContext.Current.GetOwinContext().
                               GetUserManager<ApplicationUserManager>().FindById
                               (User.Identity.GetUserId()).UserName,
                userFullName = System.Web.HttpContext.Current.GetOwinContext().
                                   GetUserManager<ApplicationUserManager>().FindById
                                   (User.Identity.GetUserId()).FullName,
                img_privateKey_encrypted = RSA.RSA.Encrypt(imagePrivateKey,
                    System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().
                    FindById(User.Identity.GetUserId()).pubKey)
            };
            _context.ImageUser.Add(imageUser);
            if (!System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById
                (User.Identity.GetUserId()).UserName.
            Equals(System.Web.HttpContext.Current.GetOwinContext().
                GetUserManager<ApplicationUserManager>().FindById
                ("4bab27de-f8b7-4835-aba8-ad3d8187a02a").UserName))
            {
                imageAdmin = new ImageUser
                {
                    imageID = image.ID,
                    imageName = image.Name,
                    userName = System.Web.HttpContext.Current.GetOwinContext().
                           GetUserManager<ApplicationUserManager>().FindById
                           ("4bab27de-f8b7-4835-aba8-ad3d8187a02a").UserName,
                    userFullName = System.Web.HttpContext.Current.GetOwinContext().
                               GetUserManager<ApplicationUserManager>().FindById
                               ("4bab27de-f8b7-4835-aba8-ad3d8187a02a").FullName,
                    img_privateKey_encrypted = RSA.RSA.Encrypt(imagePrivateKey,
                System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().
                FindById("4bab27de-f8b7-4835-aba8-ad3d8187a02a").pubKey)
                };
                _context.ImageUser.Add(imageAdmin);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Images");
        }

        public ViewResult GoToPull(string id)
        {
            var viewModel = new ImagePullFormViewModel(id);
            return View("ImagePull", viewModel);
        }

        [HttpPost]
        public ActionResult Pull(ImagePullFormViewModel imageForm)
        {

            ImageUser imageUser = _context.ImageUser.ToList().SingleOrDefault(I => I.imageID == imageForm.imageID &&
                                  I.userName == System.Web.HttpContext.Current.GetOwinContext().
                                  GetUserManager<ApplicationUserManager>().FindById
                                  (User.Identity.GetUserId()).UserName);

            if (!ModelState.IsValid)
            {
                var viewModel = new ImagePullFormViewModel(imageForm.imageID);

                return View("ImagePull", viewModel);
            }
            if (imageUser == null)
                return HttpNotFound();

            string imagePrivateKey = RSA.RSA.Decrypt(imageUser.img_privateKey_encrypted, imageForm.userPrivateKey);
            System.IO.File.WriteAllText("D:/tempFile.pem", imagePrivateKey);

            Process cmd = new Process();
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.RedirectStandardError = false;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.Start();
            cmd.StandardInput.WriteLine("cd %windir%\\Sysnative");
            cmd.StandardInput.WriteLine("wsl buildah pull --decryption-key /mnt/d/tempFile.pem " + imageUser.imageName);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            string output = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();
            cmd.Close();
            List<string> lines = new List<string>();
            Match match = Regex.Match(output, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);
            while (match.Success && lines.Count < 3)
            {
                lines.Insert(0, match.Value);
                match = match.NextMatch();
            }
            string imgID = lines[0];

            cmd = new Process();
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.RedirectStandardError = false;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.Start();
            cmd.StandardInput.WriteLine("cd %windir%\\Sysnative");
            cmd.StandardInput.WriteLine("wsl podman save --output /mnt/d/DaPlatform_Files/server/img_"
                                        + imgID + ".tar " + imgID);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            cmd.Close();
            System.IO.File.Delete(@"D:\tempFile.pem");

            string path = "D:/DaPlatform_Files/Server/img_" + imgID + ".tar";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            System.IO.File.Delete(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "img_" + imgID + ".tar");
        }
    }
}
