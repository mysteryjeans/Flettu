using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using CoreSystem.Util;
using System.Linq.Expressions;
using System.Data.Common;
using CoreSystem.RefTypeExtension;

namespace CoreSystem.Data
{
    public class LinqToSqlRepository : IRepository
    {       
        private DataContext dataContext;

        public LinqToSqlRepository(DataContext dataContext)
        {
            Guard.CheckNull(dataContext, "LinqToSqlRepository(dataContext)");
            this.dataContext = dataContext;            
        }

        #region IRepository Members

        public T GetById<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return this.dataContext.GetTable<T>().Where(predicate).SingleOrDefault();
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.dataContext.GetTable<T>();
        }

        public IEnumerable<T> Execute<T>(string query, params DbParameter[] parameters) where T : class
        {
            using (DbCommand command = this.dataContext.Connection.CreateCommand())
            {
                command.CommandText = query;
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                using (DbDataReader reader = command.ExecuteReader())
                {
                    return this.dataContext.Translate<T>(reader);
                }
            }

            throw new NotImplementedException();
        }

        public void InsertOnSubmit<T>(T entity) where T : class
        {
            this.dataContext.GetTable<T>().InsertOnSubmit(entity);
        }

        public void DeleteOnSubmit<T>(T entity) where T : class
        {
            this.dataContext.GetTable<T>().DeleteOnSubmit(entity);
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
                if (this.dataContext != null)
                    this.dataContext.Dispose();
            }
        }
    }
}
