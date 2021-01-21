﻿using AutoMapper;
using CompanyEmployees.Dtos;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Profiles
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(
                c => c.FullAdress, 
                opt => opt.MapFrom(x => string.Join("", x.Address, x.Country)));
        }
    }
}
