using System;
using System.Linq.Expressions;
using DapperWrapper.Repository;

namespace DapperWrapper.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        TRepository GetRegisteredRepository<TRepository>() where TRepository : class;
        IRepository<TEntity> GetRepositoryForEntity<TEntity>() where TEntity : class, new();

        void ManageTransaction(Action action);
        void BeginTransaction();
        void Commit();
        void Rollback();
        
        //Not sure that we want to give access to connection and Tx via the Interface
        //You can if you want to...  Or segreggate these out to a new interface... IDapperUnitOfWork
       
        //Ex.

        //internal interface IDapperUnitOfWork
        //{
        //    IDbConnection DbConnection { get; set; }
        //    IDbTransaction DbTransaction { get; set; }
        //    void GetOpenConnection();
        //}
    }
}
