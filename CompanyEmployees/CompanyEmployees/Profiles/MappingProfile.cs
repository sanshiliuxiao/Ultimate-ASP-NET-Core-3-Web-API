﻿using AutoMapper;
using CompanyEmployees.Dtos;
using Entities.Models;
using Entities.RequestFeatures;
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


            CreateMap<CompanyForCreationDto, Company>();
            CreateMap<CompanyForUpdateDto, Company>();

            CreateMap<Employee, EmployeeDto>();
            CreateMap<EmployeeForCreationDto, Employee>();
            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();

            CreateMap<UserForRegistrationDto, User>();

            CreateMap<UserForAuthenticationDto, UserForAuthentication>();
        
        }
    }
}
