using System.Collections.Generic;
using System.Linq;
using DapperWrapper.Helpers;
using DapperWrapper.Repository;
using DapperWrapper.Tests.Support;
using DapperWrapper.Tests.Support.Models;
using DapperWrapper.Tests.Support.RegisteredRepositories;
using DapperWrapper.Tests.Support.Resolvers;
using DapperWrapper.UnitOfWork;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DapperWrapper.Tests
{
    [TestClass]
    public class UnityRegisteredRepositoryForEntityTest
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
        protected IUnityContainer _container;
        protected IUnitOfWork _uow;

        [TestInitialize]
        public void Init()
        {
            _container = new UnityContainer();

            //Notice that we can register both specific generics and open generics and it still works
            //because we have 2 containers (child containers don't really matter here - 
            //I would advised to just keep them separate so we don't overwrite registrations
            var registeredRepoContainer = _container.CreateChildContainer();
            registeredRepoContainer.RegisterType<IRepository<Person>, PersonEntityRepository>();


            var entityRepoContainer = _container.CreateChildContainer();
            entityRepoContainer.RegisterType(typeof(IRepository<>), typeof(DapperRepositoryBase<>));


            _container.RegisterType<UnityResolver>(
                new InjectionConstructor(registeredRepoContainer, entityRepoContainer));
            _container.RegisterType<IUnitOfWork, DapperUnitOfWork>(
                new InjectionConstructor(new SqlConnectionFactory(), _container.Resolve<UnityResolver>()));
            
            _uow = _container.Resolve<IUnitOfWork>();
        }

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

            _container.Dispose();
            _container = null;
            _uow = null;
        }
    }
}