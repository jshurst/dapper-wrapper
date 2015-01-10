using System.Collections.Generic;
using DapperExtensions;

namespace DapperWrapper.Repository
{
    /// <summary>
    /// For basic CRUD
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Create(TEntity entity);
        TEntity Update(TEntity entity);
        TEntity Delete(TEntity entity);
        TEntity GetById<TKey>(TKey id);
        IEnumerable<TEntity> GetList(IPredicate predicate);
        IEnumerable<TEntity> GetList(object filter);
        IEnumerable<TEntity> GetList();

    }
}