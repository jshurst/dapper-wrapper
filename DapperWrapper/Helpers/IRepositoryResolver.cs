using System;
using DapperWrapper.Repository;
using DapperWrapper.UnitOfWork;

namespace DapperWrapper.Helpers
{
    public interface IRepositoryResolver
    {
        TRepository GetRegisteredRepository<TRepository>(IUnitOfWork unitOfWork) where TRepository : class;
        IRepository<TEntity> GetRepositoryForEntity<TEntity>(IUnitOfWork unitOfWork) where TEntity : class, new();
    }
}
