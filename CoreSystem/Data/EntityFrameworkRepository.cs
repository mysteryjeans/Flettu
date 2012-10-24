using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreSystem.Data;
using CoreSystem.Util;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data.Common;

namespace DevStandards
{
    public class EntityFrameworkRepository : IRepository
    {
        private ObjectContext objectContext;
        private Dictionary<string, ObjectQuery> queries;

        public EntityFrameworkRepository(ObjectContext objectContext)
        {
            Guard.CheckNull(objectContext, "EntityFrameworkRepository(objectContext)");
            
            this.objectContext = objectContext;
            this.queries = new Dictionary<string, ObjectQuery>();            
        }

        #region IRepository Members

        public T GetById<T>(Expression<Func<T, bool>> predicate) where T : class
        {
           return this.GetObjectQuery<T>().Where(predicate).FirstOrDefault();
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.GetObjectQuery<T>();
        }

        public IEnumerable<T> Execute<T>(string query, params DbParameter[] parameters) where T : class
        {
            throw new NotImplementedException();
        //    using (DbCommand command = this.objectContext.Connection.CreateCommand())
        //    {
        //        command.CommandText = query;
        //        if (parameters != null)
        //            command.Parameters.AddRange(parameters);

        //        using (DbDataReader reader = command.ExecuteReader())
        //        {
        //            this.objectContext.
        //        }
        //    }
        }

        public void InsertOnSubmit<T>(T entity) where T : class
        {
            this.objectContext.AddObject(GetEntitySetName<T>(), entity);
        }

        public void DeleteOnSubmit<T>(T entity) where T : class
        {
            this.objectContext.DeleteObject(entity);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.objectContext != null)
                    this.objectContext.Dispose();
            }
        }

        private ObjectQuery<T> GetObjectQuery<T>()
        {
            string queryString = GetEntitySetName<T>();
            if (!this.queries.ContainsKey(queryString))
                this.queries.Add(queryString, this.objectContext.CreateQuery<T>(queryString));
         
            return (ObjectQuery<T>)this.queries[queryString]; 
        }

        private static string GetEntitySetName<T>()
        {
            return "[" + typeof(T).Name + "]";
        }
    }
}
