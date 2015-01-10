using System;
using System.Collections.Generic;
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
    public class RepositoryForEntityTest
    {
        //Arrange
        protected Person _personToInsert = new Person
            {
                FirstName = "Bruce",
                LastName = "Wayne"
            };

        protected Address _addressToInsert = new Address
            {
                City = "Gotham",
                Street = "Batcave Lane",
                Zip = "90210"
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
                IRepository<Person> personRepo = _uow.GetRepositoryForEntity<Person>();
                IRepository<Address> addressRepo = _uow.GetRepositoryForEntity<Address>();

                //Beginning a Tx will open a connection, so doing this as late as possible
                //Once we open the connection all repo interaction can share it
                _uow.BeginTransaction();
                personRepo.Create(_personToInsert);

                //Id, in this case, is an auto int, so we need to set it manually bc it's a FK
                _addressToInsert.PersonId = _personToInsert.Id;
                addressRepo.Create(_addressToInsert);

                //Committing does not close the connection
                _uow.Commit();

                _foundPerson = personRepo.GetById(_personToInsert.Id);
                _foundAddress = addressRepo.GetById(_addressToInsert.Id);
            }//Dispose closes the connection

            //Assert
            Assert.AreEqual(_personToInsert.Id, _foundPerson.Id);
            Assert.AreEqual(_addressToInsert.Id, _foundAddress.Id);
        }

        [TestCleanup]
        public void CleanDb()
        {
            using (_uow)
            {
                _uow.GetRepositoryForEntity<Address>().Delete(_addressToInsert);
                _uow.GetRepositoryForEntity<Person>().Delete(_personToInsert);
            }
        }

        /// <summary>
        /// I would most likely do this in a static/singleton, or a container when bootstrapping my application.
        /// See Support.Registrations for an example.
        /// </summary>
        /// <returns></returns>
        private static IUnitOfWork GetUnitOfWork()
        {
            //Notice here we are not passing any registrations, that's because we don't have any custom ones to register
            return new DapperUnitOfWork(new SqlConnectionFactory(), ActivatorResolver.Instance);
        }
    }
}
