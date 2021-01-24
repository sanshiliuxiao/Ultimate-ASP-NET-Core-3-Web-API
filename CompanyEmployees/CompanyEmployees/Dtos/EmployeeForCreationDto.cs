using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Dtos
{
    // 通过继承的方式，避免重复写验证属性
    public class EmployeeForCreationDto: EmployeeForMainpulationDto
    {
        // 可以覆盖原属性，也能添加新的属性
        [Range(14, int.MaxValue, ErrorMessage = "Age is required and it can't be lower than 14")]
        public new int Age { get; set; }
    }
}
