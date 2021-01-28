using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Dtos;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.Models;
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
        public async Task<IActionResult> GetCompanies()
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


            //var companies = _repository.Company.GetAllCompanies(trackChanges: false);
            //var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            //return Ok(companiesDto);

            var companies = await _repository.Company.GetAllCompaniesAsync(false);
            var compainesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(compainesDto);
        }
        
        [HttpGet("{id}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(id, false);
            if (company == null)
            {
                _logger.LogInfo("Company with id: {id} does't exist in the database.");
                return NotFound();
            }
            else
            {
                var companyDto = _mapper.Map<CompanyDto>(company);
                return Ok(companyDto);
            }
        }
    
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);
            _repository.Company.CreateCompany(companyEntity);
            await _repository.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);

        }


        // 这种写法，不能直接通过 URL 进行获取， 只能使用 CreateAtRoute 的方式。
        // api/companies/collection/[id1],[id2] 将会返回 415， 因为 string 不会被 转化成  IEnumerable<Guid>
        // 要解决这个问题，可以使用  custom model binding

        // 或者 不使用这种方式，而采用 [FromBody] 传参

        //[HttpGet("collection/({ids})", Name = "CompanyCollection")]
        //public IActionResult GetCompanyCollection(IEnumerable<Guid> ids)
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }

            var companyEntities = await _repository.Company.GetByIdsAsync(ids, false);

            if (ids.Count() != companyEntities.Count())
            {
                _logger.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            return Ok(companiesToReturn);
        }
        
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection == null)
            {
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach(var company in companyEntities)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }
        
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            // 因为 ValidateCompanyExistsAttribute 做了校验工作
            //var company = await _repository.Company.GetCompanyAsync(id, false);
            //if (company == null)
            //{
            //    _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
            //    return NotFound();
            //}

            var company = HttpContext.Items["company"] as Company;


            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();

            return NoContent();
        }
    
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
        {
            var companyEntity = HttpContext.Items["company"] as Company;

            //var companyEntity = _repository.Company.GetCompany(id, false);
            //if (companyEntity == null)
            //{
            //    _logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
            //    return NotFound();
            //}



            // 该语法回将前面一个参数的 属性 跟 后一个参数的属性进行比较
            // 最后将 前一个参数的属性变动值，赋值给后一个属性。

            // 因为 CompanyForUpdateDto 和 EmployeeForUpdateDto 的缘故，如果含有 employee 字段，则会继续添加新的 employee
            _mapper.Map(company, companyEntity);
            await _repository.SaveAsync();

            return NoContent();
        }
    }
}
