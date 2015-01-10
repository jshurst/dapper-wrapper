using System;
using DapperWrapper.Repository;
using DapperWrapper.Tests.Support;
using DapperWrapper.Tests.Support.Models;
using DapperWrapper.UnitOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DapperWrapper.Tests
{
    [TestClass]
    public class MockTest
    {
        readonly Person _personToInsert = new Person { FirstName = "Super", LastName = "Man" };
        Person _insertedPerson;

        [TestMethod]
        public void TransactionManagerMockTest()
        {
            //Arrange
            var repoMock = new Mock<IRepository<Person>>();
            repoMock.Setup(repo => repo.Create(It.IsAny<Person>())).Returns(_personToInsert);

            var mock = new Mock<IUnitOfWork>();
            mock.Setup(uow => uow.ManageTransaction(It.IsAny<Action>())).Callback<Action>(action => action());
            mock.Setup(uow => uow.GetRepositoryForEntity<Person>()).Returns(repoMock.Object);
            
            //Act
            MvcControllerSimulation(mock.Object);

            //Assert
            Assert.AreSame(_personToInsert, _insertedPerson);
        }

        /// <summary>
        /// I'm trying to simulate what a controller/wcf service might look like
        /// </summary>
        /// <param name="uow"></param>
        private void MvcControllerSimulation(IUnitOfWork uow)
        {
            using (uow)
            {
                IRepository<Person> personRepo = uow.GetRepositoryForEntity<Person>();
                uow.ManageTransaction(() => _insertedPerson = personRepo.Create(new Person()));
            }
        }
    }
}