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
    public class UnityRegisteredRepositoryTest
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
        protected IUnityContainer _container;
        protected IUnitOfWork _uow;

        [TestInitialize]
        public void Init()
        {
            _container = new UnityContainer();

            var registeredRepoContainer = _container.CreateChildContainer();
            registeredRepoContainer.RegisterType<IPersonRepository, PersonRepository>();
            
            var entityRepoContainer = _container.CreateChildContainer();
            entityRepoContainer.RegisterType(typeof(IRepository<>), typeof(DapperRepositoryBase<>));

            _container.RegisterType<UnityResolver>(new InjectionConstructor(registeredRepoContainer, entityRepoContainer));
            _container.RegisterType<IUnitOfWork, DapperUnitOfWork>(
                new InjectionConstructor(new SqlConnectionFactory(), _container.Resolve<UnityResolver>()));
            _uow = _container.Resolve<IUnitOfWork>();
        }


        [TestMethod]
        public void InsertAndSelectTransactionTest()
        {
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
        public void CleanUp()
        {
            using (_uow)
            {
                _uow.GetRegisteredRepository<IPersonRepository>().DeletePersonAndAddresses(_foundPerson);
            }

            _container.Dispose();
            _container = null;
            _uow = null;
        }
    }
}