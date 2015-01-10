using Dapper;
using DapperExtensions;
using DapperWrapper.Repository;
using DapperWrapper.Tests.Support.Models;
using DapperWrapper.UnitOfWork;
using System.Linq;

namespace DapperWrapper.Tests.Support.RegisteredRepositories
{
    public class PersonEntityRepository : DapperRepositoryBase<Person>
    {
        public PersonEntityRepository(DapperUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// I'm showing here that you can override the Create method from the base, but still
        /// keep all of the other methods.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public override Person Create(Person person)
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

            return person;
        }

        public override Person GetById<TKey>(TKey id)
        {
            UnitOfWork.GetOpenConnection();
            var person = UnitOfWork.DbConnection.Get<Person>(id, UnitOfWork.DbTransaction);
            var addresses = UnitOfWork.DbConnection.GetList<Address>(new { PersonId = id }, null, UnitOfWork.DbTransaction);
            person.Addresses = addresses;

            return person;
        }

        public override Person Delete(Person person)
        {
            UnitOfWork.ManageTransaction(() =>
            {
                //We could do this faster with regular Dapper, but this is a little easier
                foreach (var address in person.Addresses)
                {
                    UnitOfWork.DbConnection.Delete(address, UnitOfWork.DbTransaction);
                }

                UnitOfWork.DbConnection.Delete(person, UnitOfWork.DbTransaction);
            });

            return person;
        }
    }
}
