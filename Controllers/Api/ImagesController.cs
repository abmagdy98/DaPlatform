using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using DaPlatform.Dtos;
using DaPlatform.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace DaPlatform.Controllers.Api
{
    public class ImagesController : ApiController
    {
        private ApplicationDbContext _context;
        //private readonly UserManager<ApplicationUser> userManager;

        

        public ImagesController()
        {
            _context = new ApplicationDbContext();
        }

        public IHttpActionResult GetImages(string query = null)
        {
            var imagesQuery = _context.Images.Where(i => i.ID != null && !i.ID.Equals(""));

            if (!String.IsNullOrWhiteSpace(query))
                imagesQuery = imagesQuery.Where(i => i.Name.Contains(query));

            var imageDtos = imagesQuery
                .ToList()
                .Select(Mapper.Map<Image, ImageDto>);

            return Ok(imageDtos);
        }

        public IHttpActionResult GetImage(string id)
        {
            var image = _context.Images.SingleOrDefault(c => c.ID == id);

            if (image == null)
                return NotFound();

            return Ok(Mapper.Map<Image, ImageDto>(image));
        }

        [HttpPost]
        public IHttpActionResult CreateImage(ImageDto imageDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var image = Mapper.Map<ImageDto, Image>(imageDto);
            _context.Images.Add(image);
            _context.SaveChanges();

            imageDto.ID = image.ID;
            return Created(new Uri(Request.RequestUri + "/" + image.ID), imageDto);
        }

        [HttpPut]
        public IHttpActionResult UpdateImage(string id, ImageDto imageDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var imageInDb = _context.Images.SingleOrDefault(i => i.ID == id);

            if (imageInDb == null)
                return NotFound();

            Mapper.Map(imageDto, imageInDb);

            _context.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        public IHttpActionResult DeleteImage(string id)
        {
            var imageInDb = _context.Images.SingleOrDefault(i => i.ID == id);

            if (imageInDb == null)
                return NotFound();

            _context.Images.Remove(imageInDb);
            _context.SaveChanges();

            return Ok();
        }
    }
}
