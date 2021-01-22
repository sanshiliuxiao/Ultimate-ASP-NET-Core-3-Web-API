using AutoMapper;
using CompanyEmployees.Dtos;
using Contracts;
using Microsoft.AspNetCore.Mvc;
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

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} does't exist in the database.");
                return NotFound();
            }

            var employeesFromDb = _repository.Employee.GetEmployees(companyId, false);

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

            return Ok(employeesDto);
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeDb = _repository.Employee.GetEmployee(companyId, id, false);
            if (employeeDb == null)
            {
                _logger.LogInfo($"Employee with id: {id} does't exist in the database");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeDb);
            return Ok(employeeDto);
        }
    }
}
