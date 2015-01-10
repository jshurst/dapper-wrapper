using System.Collections.Generic;

namespace DapperWrapper.Tests.Support.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
    }
}
