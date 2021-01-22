﻿using AutoMapper;
using CompanyEmployees.Dtos;
using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            //try
            //{
            //    var companies = _repository.Company.GetAllCompanies(trackChanges: false);
            //    var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            //    return Ok(companiesDto);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"Something went worng in the {nameof(GetCompanies)} action {ex}");
            //    return StatusCode(500, "Internal server error");
            //}

            // 使用 app.UseExceptionHandler 后，可以全局捕获错误

            // throw new Exception("Exception");
            var companies = _repository.Company.GetAllCompanies(trackChanges: false);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesDto);
        }
    }
}
