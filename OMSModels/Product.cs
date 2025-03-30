using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOMS.Model
{
    public class Product
    {
        public required int ProductID { get; set; }
        public required string Name { get; set; }
        public required float Price { get; set; }
    }
}
