using System;
using System.Collections.Generic;
using System.Linq;
using DapperWrapper.Helpers;
using DapperWrapper.Repository;
using DapperWrapper.UnitOfWork;

namespace DapperWrapper.Tests.Support.Resolvers
{
    /// <summary>
    /// This doesn't have to be a singleton or static however, I just want to load this up one time.
    /// We could get rid of this class and user a service locator inside the UnitOfWork instead.
    /// We really just need a static list of registration mappings.
    /// //BASICALLY WE JUST NEED A BOOTSTRAP METHOD TO REGISTER OUR CUSTOM REPOSITORIES
    /// </summary>
    public class ActivatorResolver : IRepositoryResolver
    {
        private static readonly ActivatorResolver _instance = new ActivatorResolver();
        public static ActivatorResolver Instance { get { return _instance; } }

        public Dictionary<Type, Type> Repositories = new Dictionary<Type, Type>();

        private ActivatorResolver()
        { }

        public TRepository GetRegisteredRepository<TRepository>(IUnitOfWork unitOfWork) where TRepository : class
        {
            if (Instance == null || Repositories.Count == 0) throw new ApplicationException("No repositories have been registered.");
            var type = typeof(TRepository);
            var foundInterface = Repositories.Single(x => x.Key == type);
            if (foundInterface.Key == null) throw new ApplicationException(string.Format("Unable to find registered repository for type {0}", type));
            var repository = Activator.CreateInstance(foundInterface.Value, unitOfWork);
            return (TRepository)repository;
        }

        public IRepository<TEntity> GetRepositoryForEntity<TEntity>(IUnitOfWork unitOfWork) where TEntity : class, new()
        {
            var type = typeof(DapperRepositoryBase<TEntity>);

            if (Instance != null && Repositories.Any(x => x.Key == type))
            {
                var foundInterface = Repositories.Single(x => x.Key == type);
                if (foundInterface.Key != null)
                    return (DapperRepositoryBase<TEntity>)Activator.CreateInstance(foundInterface.Value, this);
            }

            return new DapperRepositoryBase<TEntity>(unitOfWork as DapperUnitOfWork);
        }
    }
}
