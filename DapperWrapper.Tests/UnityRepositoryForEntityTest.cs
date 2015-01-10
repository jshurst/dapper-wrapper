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
    public class UnityRepositoryForEntityTest
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

            _container.Dispose();
            _container = null;
            _uow = null;
        }
    }
}