using System.Collections.Generic;
using DapperWrapper.Tests.Support.Models;

namespace DapperWrapper.Tests.Support.RegisteredRepositories
{
    public interface IPersonRepository
    {
        void DeletePersonAndAddresses(Person person);
        void SavePersonAndAddress(Person person);
        Person GetPersonAndAddressesByPersonId(int id);
    }
}