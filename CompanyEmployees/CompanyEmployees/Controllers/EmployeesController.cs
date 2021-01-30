using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Dtos;
using Contracts;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age can't be less than min age.");
            }

            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} does't exist in the database.");
                return NotFound();
            }

            var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId,employeeParameters , false);

            Response.Headers.Add("X-pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));


            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

            //return Ok(employeesDto);

            // An error occurred while trying to create an XmlSerializer for the type 'System.Collections.Generic.List 'System.Dynamic.ExpandoObject, System.Linq.Expressions,'.
            // System.InvalidOperationException: To be XML serializable, types which inherit from IEnumerable must have an implementation of Add(System.Object)
            var shapeData = _dataShaper.ShapeData(employeesDto, employeeParameters.Fields);
            return Ok(shapeData);
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, false);
            if (employeeDb == null)
            {
                _logger.LogInfo($"Employee with id: {id} does't exist in the database");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeDb);
            return Ok(employeeDto);
        }
    
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            // 因为 SuppressModelStateInvalidFilter 设置成 true 了， 所以下面代码有效
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object");

                // 除了在属性上添加 错误信息，还可以自定义 某个 属性的错误信息
                ModelState.AddModelError(nameof(EmployeeForCreationDto.Age), "必须超过 18 岁");
                
                return UnprocessableEntity(ModelState);
            }

            if (employee == null)
            {
                _logger.LogError("EmployeeForCreation object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }




            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }
        
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;
            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();
            return NoContent();
        }
    
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employee, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }
    
        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            // patchDoc.ApplyTo(employeeToPatch);

            // 要先检测 patch 是否错误
            patchDoc.ApplyTo(employeeToPatch, ModelState);
            // 还要验证 Model 是否错误
            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            

            _mapper.Map(employeeToPatch, employeeEntity);

            await _repository.SaveAsync();
            return NoContent();


        }
    
        

    }
}
