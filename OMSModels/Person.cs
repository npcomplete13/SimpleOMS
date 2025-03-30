using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOMS.Model
{
    public abstract class Person
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required char MiddleInitial { get; set; }
        public string FullName
        {
            get { return FirstName + " " + MiddleInitial + ". " + LastName; }
        }
        public override string ToString()
        {
            return FullName;
        }
    }
}
