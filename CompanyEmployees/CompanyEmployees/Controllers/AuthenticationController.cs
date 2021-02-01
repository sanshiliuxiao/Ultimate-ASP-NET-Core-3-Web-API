using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Dtos;
using Contracts;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthenticationManager _authManager;

        public AuthenticationController(ILoggerManager logger, IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IAuthenticationManager authManager)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _authManager = authManager;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="userForRegistration"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistration)
        {
            var user = _mapper.Map<User>(userForRegistration);

            var flag = true;
            foreach (var role in userForRegistration.Roles)
            {

                flag = await _roleManager.RoleExistsAsync(role);

            }
            if (!flag)
            {
                ModelState.TryAddModelError("roles error", "There is no role in the database");
                return BadRequest(ModelState);

            }

            var result = await _userManager.CreateAsync(user, userForRegistration.Password);
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            // 如果要验证数据库中是否存在该角色，则应该 注入 RoleManager<TRole> 实例

            await _userManager.AddToRolesAsync(user, userForRegistration.Roles);

            return StatusCode(201);

        }
        
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userDto)
        {
            var user = _mapper.Map<UserForAuthentication>(userDto);
        
            if (!await _authManager.ValidateUser(user))
            {
                _logger.LogWarn($"{nameof(Authenticate)}: Authentication failed. Wrong user name or password.");
                return Unauthorized();
            }

            return Ok(new { Token = await _authManager.CreateToken() });
        
        }
    
    }
}
