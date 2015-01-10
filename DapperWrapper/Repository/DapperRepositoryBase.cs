using System.Collections.Generic;
using DapperExtensions;
using DapperWrapper.UnitOfWork;

namespace DapperWrapper.Repository
{
    public class DapperRepositoryBase
    {
        public readonly DapperUnitOfWork UnitOfWork;

        public DapperRepositoryBase(DapperUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
    }

    public class DapperRepositoryBase<TEntity> : DapperRepositoryBase, IRepository<TEntity> where TEntity : class
    {
        public DapperRepositoryBase(DapperUnitOfWork unitOfWork) : base(unitOfWork) { }

        public virtual TEntity GetById<TKey>(TKey id)
        {
            UnitOfWork.GetOpenConnection();
            return UnitOfWork.DbConnection.Get<TEntity>(id, UnitOfWork.DbTransaction);
        }

        public virtual TEntity Create(TEntity entity)
        {
            UnitOfWork.GetOpenConnection();
            UnitOfWork.DbConnection.Insert(entity, UnitOfWork.DbTransaction);
            return entity;
        }

        public virtual TEntity Update(TEntity entity)
        {
            UnitOfWork.GetOpenConnection();
            UnitOfWork.DbConnection.Update(entity, UnitOfWork.DbTransaction);
            return entity;
        }

        public virtual TEntity Delete(TEntity entity)
        {
            UnitOfWork.GetOpenConnection();
            UnitOfWork.DbConnection.Delete(entity, UnitOfWork.DbTransaction);
            return entity;
        }

        public virtual IEnumerable<TEntity> GetList(IPredicate predicate)
        {
            UnitOfWork.GetOpenConnection();
            var results = UnitOfWork.DbConnection.GetList<TEntity>(predicate, null, UnitOfWork.DbTransaction);
            return results;
        }

        public virtual IEnumerable<TEntity> GetList(object filter)
        {
            var objectsAndValues = GetObjectValues(filter);
            var predicate = BuildPredicateGroup(objectsAndValues);
            return GetList(predicate);
        }

        public virtual IEnumerable<TEntity> GetList()
        {
            return GetList(null);
        }

        private static IEnumerable<KeyValuePair<string, object>> GetObjectValues(object obj)
        {
            var result = new Dictionary<string, object>();
            if (obj == null) return result;

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.GetIndexParameters().Length > 0) continue;
                string name = propertyInfo.Name;
                object value = propertyInfo.GetValue(obj, null);
                result[name] = value;
            }
            return result;
        }

        private static PredicateGroup BuildPredicateGroup(IEnumerable<KeyValuePair<string, object>> objects)
        {
            if (objects == null) return null;
            var pg = new PredicateGroup { Operator = GroupOperator.And, Predicates = new List<IPredicate>() };

            foreach (var obj in objects)
            {
                var predicate = new FieldPredicate<TEntity> { PropertyName = obj.Key, Operator = Operator.Eq, Value = obj.Value, Not = false };
                pg.Predicates.Add(predicate);
            }
            return pg;
        }
    }
}