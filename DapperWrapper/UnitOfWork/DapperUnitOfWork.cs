using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using DapperWrapper.Helpers;
using DapperWrapper.Repository;

namespace DapperWrapper.UnitOfWork
{
    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private readonly IRepositoryResolver _resolver;
        private readonly IConnectionFactory _connectionFactory;


        public IDbConnection DbConnection { get; private set; }
        public IDbTransaction DbTransaction { get; private set; }

        public DapperUnitOfWork(IConnectionFactory connectionFactory, IRepositoryResolver resolver)
        {
            //_connection = connection;
            _resolver = resolver;
            _connectionFactory = connectionFactory;
        }

        public virtual void GetOpenConnection()
        {
            if (DbConnection != null) return;
            DbConnection = _connectionFactory.GetConnection();
            DbConnection.Open();
        }

        public virtual TRepository GetRegisteredRepository<TRepository>() where TRepository : class
        {
            return _resolver.GetRegisteredRepository<TRepository>(this);
        }

        public virtual IRepository<TEntity> GetRepositoryForEntity<TEntity>() where TEntity : class, new()
        {
            return _resolver.GetRepositoryForEntity<TEntity>(this);
        }

        public virtual void BeginTransaction()
        {
            GetOpenConnection();
            if (DbTransaction != null) return;
            DbTransaction = DbConnection.BeginTransaction();
        }

        public virtual void Commit()
        {
            if (DbTransaction == null) return;
            DbTransaction.Commit();
        }

        public virtual void Rollback()
        {
            if (DbTransaction == null) return;
            DbTransaction.Rollback();
        }

        public virtual void Dispose()
        {
            if (DbTransaction != null)
                DbTransaction.Dispose();

            if (DbConnection != null)
                DbConnection.Dispose();

            DbTransaction = null;
            DbConnection = null;
        }

        public virtual void ManageTransaction(Action action)
        {
            GetOpenConnection();
            bool outerTxStarted = (DbTransaction != null);
            if (!outerTxStarted) BeginTransaction();
            action();
            if (!outerTxStarted) Commit();
        }
    }
}