using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOMS.Model
{
    public class Employee : Person
    {
        public required int EmployeeID { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
