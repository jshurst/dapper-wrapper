using DapperExtensions.Mapper;

namespace DapperWrapper.Tests.Support.Models
{
    public class PersonMapper : ClassMapper<Person>
    {
        public PersonMapper()
        {
            Map(p => p.Addresses).Ignore();
            AutoMap();
        }
    }
}
