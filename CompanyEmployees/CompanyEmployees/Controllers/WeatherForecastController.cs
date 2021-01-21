using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    // [Route("[controller]")]
    [Route("w")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;

        public WeatherForecastController(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Company>> Get()
        {
            return new ActionResult<IEnumerable<Company>>(_repositoryManager.Company.FindAll());
        }
    }
}
