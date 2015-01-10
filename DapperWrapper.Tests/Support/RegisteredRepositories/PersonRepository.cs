using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperExtensions;
using DapperWrapper.Repository;
using DapperWrapper.Tests.Support.Models;
using DapperWrapper.UnitOfWork;

namespace DapperWrapper.Tests.Support.RegisteredRepositories
{
    /// <summary>
    /// JHurst
    /// This class can extend the base or override methods from the base (INSERT, CREATE, etc),
    /// but still can make use of the RepositoryBase for any common function.
    /// </summary>
    public class PersonRepository : DapperRepositoryBase, IPersonRepository
    {
        public PersonRepository(DapperUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        
        public virtual void SavePersonAndAddress(Person person)
        {
            UnitOfWork.ManageTransaction(() =>
                {
                    const string PERSON_SQL = @"
                        DECLARE @InsertedRows AS TABLE (Id int);
                        INSERT INTO Person (FirstName, LastName) OUTPUT Inserted.Id INTO @InsertedRows
                        VALUES (@FirstName, @LastName)
                        SELECT Id FROM @InsertedRows";
                    
                    person.Id = UnitOfWork.DbConnection.Query<int>(PERSON_SQL, person, UnitOfWork.DbTransaction).Single();

                    foreach (var address in person.Addresses)
                    {
                        address.PersonId = person.Id;
                    }

                    const string ADDRESS_SQL = "INSERT INTO Address (Street, City, Zip, PersonId) VALUES (@Street, @City, @Zip, @PersonId)";
                    UnitOfWork.DbConnection.Execute(ADDRESS_SQL, person.Addresses, UnitOfWork.DbTransaction);
                });
        }

        public virtual Person GetPersonAndAddressesByPersonId(int id)
        {
            UnitOfWork.GetOpenConnection();
            var person = UnitOfWork.DbConnection.Get<Person>(id, UnitOfWork.DbTransaction);
            var addresses = UnitOfWork.DbConnection.GetList<Address>(new {PersonId = id}, null, UnitOfWork.DbTransaction);
            person.Addresses = addresses;

            return person;
        }
      
        public virtual void DeletePersonAndAddresses(Person person)
        {
            UnitOfWork.ManageTransaction(() =>
                {
                    //We could do this faster with regular Dapper, but this is a little easier
                    foreach (var address in person.Addresses)
                    {
                        UnitOfWork.DbConnection.Delete(address,UnitOfWork.DbTransaction);
                    }

                    UnitOfWork.DbConnection.Delete(person, UnitOfWork.DbTransaction);
                });
        }
    }
}