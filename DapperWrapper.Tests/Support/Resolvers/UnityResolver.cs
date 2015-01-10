using DapperWrapper.Helpers;
using DapperWrapper.Repository;
using DapperWrapper.UnitOfWork;
using Microsoft.Practices.Unity;

namespace DapperWrapper.Tests.Support.Resolvers
{
    public class UnityResolver : IRepositoryResolver
    {
        private readonly IUnityContainer _registeredRepositoriesContainer;
        private readonly IUnityContainer _repositoriesForEntityContainer;

        public UnityResolver(IUnityContainer registeredRepositoriesContainer, IUnityContainer repositoriesForEntityContainer)
        {
            _registeredRepositoriesContainer = registeredRepositoriesContainer;
            _repositoriesForEntityContainer = repositoriesForEntityContainer;
        }

        public TRepository GetRegisteredRepository<TRepository>(IUnitOfWork unitOfWork) where TRepository : class
        {
            return _registeredRepositoriesContainer.Resolve<TRepository>(new ParameterOverride("unitOfWork", unitOfWork));
        }

        public IRepository<TEntity> GetRepositoryForEntity<TEntity>(IUnitOfWork unitOfWork) where TEntity : class, new()
        {
            return _repositoriesForEntityContainer.Resolve<IRepository<TEntity>>(new ParameterOverride("unitOfWork", unitOfWork));
        }
    }
}
