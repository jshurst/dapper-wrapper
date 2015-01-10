I.	Introduction
=============

This is my take on the repository and unit of work pattern.  At first I was trying to be data access agnostic as possible, however this was fairly counter productive in its application so I decided just to focus on Dapper/DapperExtensions.  

Ideally you would be able to simply change the underlying implementation in one place (like an IOC container in a bootstrap class for you application) and the entire underlying data storage would be changed.  The problem with this (well, there are multiple problems) is that you lose a lot of the power of the underlying data access, like using Linq with the Entity Framework, but if you still want to have a data access agnostic uow/repo pattern then you can always create a common interface (just extract common methods from the IUnitOfWork).



II.	Types of Repositories
=============

There are 2 types of repositories that you can use.

*	Entity/Common/Base/CRUD Repositories 
*	Registered Repositories


```
	using (IUnitOfWork unitOfWork = new DapperUnitOfWork(myIDbConnection))  
	{  
		//Beginning a Tx will open a connection, so doing this as late as possible
       //Once we open the connection all repo interaction can share it
		//You could also use the ManageTransaction() method...and probably should...

		unitOfWork.BeginTransaction();

		//Common  
		IRepository<Person> commonRepo = unitOfWork.GetRepositoryForEntity<Person>();  
	
		//Registered  
		IPersonRepository registredRepo = unitOfWork.GetRegisteredRepository<IPersonRepository>();  
		
		//Again...only if you need a transaction		
		unitOfWork.Commit();
	}  
```

I am wrapping this in a using statment so that the unit of work will be disposed (connection closed, open transactions rolled back, etc).  Using transactions is optional, I'm just putting them here to show you that it's possible (in fact you should look a the ManageTransaction() method for enlisiting in safe transactions.

A.	Common/Base/CRUD Repositories
=============

Entity (or base) repositories are repositories that act on a single model and share the same functionality (CRUD operations).  Many people use this pattern with the Entity Framework (Repository< T >).  With our Dapper implemenation we're using DapperExtensions (however we can also make Aggregate repositories and can override the CRUD methods which will be shown later).

```
	IRepository<Person> repo = unitOfWork.GetRepositoryForEntity<Person>();
	repo.Insert(new Person() {LastName = "Wayne"});
```

B.	Registered Repositories
=============

Registered repositories are specialized implementations that are outside of normal CRUD operations.  For example maybe you have an Aggregate repository for Person and Addresses.  This isn't a great example, because Address would probably simply be a property on the Person class, which is possible too, but hopefully you'll get the point.

```
	IPersonRepository repo = unitOfWork.GetRegisteredRepository<IPersonRepository>();  
	repo.DeleteAllAddressesAndPeople(); //Notice that this is not just a CRUD method but is defined on your IPersonRepository interface
```

And your actual repository's method might look like this.

```
	 public void DeleteAllAddressesAndPeople()  
        {  
            const string SQL = "DELETE Address;  DELETE Person;";  
            UnitOfWork.GetOpenConnection();  
            UnitOfWork.DbConnection.Execute(SQL, UnitOfWork.DbTransaction);  
        }  
```

As the name implies, you must register these repositories first.  I usually do this in the constructor for the Unit Of Work.

```
	var uow = new DapperUnitOfWork(myIDbConnection, Registrations.Instance.Repositories);
```

In the example below I'm using a singleton for registrations.  It looks like this (feel free to do this however you want - maybe you don't want a singleton).

```
	public class Registrations
    {  
        private static readonly Registrations _instance = new Registrations();  
        public static Registrations Instance { get { return _instance; } }  

        public Dictionary<Type, Type> Repositories;  

        private Registrations()  
        {  
            Repositories = new Dictionary<Type, Type>  
                {  
                    { typeof(IPersonRepository), typeof(PersonRepository) }  
						//Additional registrations, or maybe use an IOC  
                };  
        }  
    }  
```

In addition if you really wanted to you could also register an instance of IRepository<Entity> and override CRUD methods.

```
	Repositories = new Dictionary<Type, Type>
		{
			{ typeof(IRepository<Person>), typeof(PersonEntityRepository) }
		};        
```

Then you can resolve and use it like this:

```
 using (IUnitOfWork uow = new DapperUnitOfWork(ConnectionStrings.DapperWrapperConnection, _registrations))
{
    IRepository<Person> personRepo = uow.GetRegisteredRepository<IRepository<Person>>();

		//  The idea of the ManageTransaction method is to make sure that only the top level call
		// begins and commits the transaction
    uow.ManageTransaction(() =>
    {
        _personToInsert.Addresses = new List<Address> { _addressToInsert };
        personRepo.Create(_personToInsert);
    });

    _foundPerson = personRepo.GetById(_personToInsert.Id);
    _foundAddress = _foundPerson.Addresses.Single(x => x.PersonId == _foundPerson.Id);
} //Dispose closes the connection

```

For more information I suggest you check out the Unit tests...and upcoming wiki.