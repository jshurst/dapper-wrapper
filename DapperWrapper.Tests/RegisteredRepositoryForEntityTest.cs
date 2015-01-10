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
    public class RegisteredRepositoryForEntityTest
    {
        //Arrange
        protected Person _personToInsert = new Person
        {
            FirstName = "Peter",
            LastName = "Parker"
        };

        protected Address _addressToInsert = new Address
        {
            City = "New York",
            Street = "Cobweb Circle",
            Zip = "00102"
        };


        protected Person _foundPerson;
        protected Address _foundAddress;
        protected IUnitOfWork _uow = GetUnitOfWork();


        /// <summary>
        /// THIS IS THE SAME TEST AS IN THE REPOSITORYFORENTITYTEST
        /// The difference is that we are creating our own implementation of IRepository<Person>
        /// and registering it before resolving.  By doing this we can override and CRUD operation that we want
        /// but still use the base if we want as well.
        /// </summary>
        [TestMethod]
        public void InsertAndSelectTransactionTest()
        {
            //Act
            using (_uow)
            {
                IRepository<Person> personRepo = _uow.GetRegisteredRepository<IRepository<Person>>();

                //Beginning a Tx will open a connection, so doing this as late as possible
                //Once we open the connection all repo interaction can share it
                _uow.ManageTransaction(() =>
                {
                    _personToInsert.Addresses = new List<Address> { _addressToInsert };
                    personRepo.Create(_personToInsert);
                });

                _foundPerson = personRepo.GetById(_personToInsert.Id);
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
                _uow.GetRegisteredRepository<IRepository<Person>>().Delete(_foundPerson);
            }

            _uow = null;
        }

        /// <summary>
        /// I would most likely do this in a static/singleton, or a container when bootstrapping my application.
        /// See Support.Registrations for an example.
        /// </summary>
        /// <returns></returns>
        private static IUnitOfWork GetUnitOfWork()
        {//Hydrate the resolver, you could do this using an IoC container.
            //I'm just using a simple list here to show you how it can be light-weight
            ActivatorResolver.Instance.Repositories.Add(typeof(IRepository<Person>), typeof(PersonEntityRepository));

            return new DapperUnitOfWork(new SqlConnectionFactory(), ActivatorResolver.Instance);
        }
    }
}
