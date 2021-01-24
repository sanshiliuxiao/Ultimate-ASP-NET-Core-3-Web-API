using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Dtos
{
    // 通过继承的方式，避免重复写验证属性
    public class EmployeeForUpdateDto: EmployeeForMainpulationDto
    {
        

    }
}
