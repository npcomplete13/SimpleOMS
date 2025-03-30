using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOMS.Model
{
    public class Customer : Person
    {
        public required int CustomerID { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
