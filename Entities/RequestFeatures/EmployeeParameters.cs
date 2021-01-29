using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.RequestFeatures
{
    public class EmployeeParameters: RequestParameters
    {
        public uint MinAge { get; set; } = 0;
        public uint MaxAge { get; set; } = int.MaxValue;
        public bool ValidAgeRange => MaxAge > MinAge;
        public string SearchTerm { get; set; }

    }
}
