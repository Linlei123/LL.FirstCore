using AutoMapper;
using LL.Core.Model.Dto;
using LL.Core.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.Core.AutoMapper
{
    public class CustomProfile : Profile
    {
        public CustomProfile()
        {
            CreateMap<BaseUserInfo, UserInfoDto>().ReverseMap();
        }
    }
}
