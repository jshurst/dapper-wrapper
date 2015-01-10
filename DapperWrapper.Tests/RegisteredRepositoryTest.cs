using System;
using System.Collections.Generic;
using System.Linq;
using DapperWrapper.Repository;
using DapperWrapper.Tests.Support;
using DapperWrapper.Tests.Support.Models;
using DapperWrapper.Tests.Support.RegisteredRepositories;
using DapperWrapper.Tests.Support.Resolvers;
using DapperWrapper.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DapperWrapper.Tests
{
    [TestClass]
    public class RegisteredRepositoryTest
    {
        //Arrange
        protected Person _personToInsert = new Person
        {
            FirstName = "Clark",
            LastName = "Kent"
        };

        protected Address _addressToInsert = new Address
        {
            City = "Metropolis",
            Street = "1 Super Way",
            Zip = "10101"
        };


        protected Person _foundPerson;
        protected Address _foundAddress;
        protected IUnitOfWork _uow = GetUnitOfWork();

        [TestMethod]
        public void InsertAndSelectTransactionTest()
        {
            //Act
            using (_uow)
            {
                IPersonRepository personRepo = _uow.GetRegisteredRepository<IPersonRepository>();

                //Beginning a Tx will open a connection, so doing this as late as possible
                //Once we open the connection all repo interaction can share it
                _uow.ManageTransaction(() =>
                {
                    _personToInsert.Addresses = new List<Address> { _addressToInsert };
                    personRepo.SavePersonAndAddress(_personToInsert);
                });

                _foundPerson = personRepo.GetPersonAndAddressesByPersonId(_personToInsert.Id);
                _foundAddress = _foundPerson.Addresses.Single(x => x.PersonId == _foundPerson.Id);
            } //Dispose closes the connection

            //Assert
            Assert.AreEqual(_personToInsert.Id, _foundPerson.Id);
            Assert.IsNotNull(_foundAddress);
        }

        [TestCleanup]
        public void CleanDb()
        {
            using (_uow)
            {
                _uow.GetRegisteredRepository<IPersonRepository>().DeletePersonAndAddresses(_foundPerson);
            }

            _uow = null;
        }
        
        private static IUnitOfWork GetUnitOfWork()
        {//Hydrate the resolver, you could do this using an IoC container.
            //I'm just using a simple list here to show you how it can be light-weight
            ActivatorResolver.Instance.Repositories.Add(typeof(IPersonRepository), typeof(PersonRepository));

            return new DapperUnitOfWork(new SqlConnectionFactory(), ActivatorResolver.Instance);
        }
    }
}
