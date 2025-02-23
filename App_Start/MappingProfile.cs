using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using DaPlatform.Dtos;
using DaPlatform.Models;

namespace DaPlatform.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            Mapper.CreateMap<Image, ImageDto>();
            Mapper.CreateMap<ImageDto, Image>()
                .ForMember(i => i.ID, opt => opt.Ignore());
        }
    }
}