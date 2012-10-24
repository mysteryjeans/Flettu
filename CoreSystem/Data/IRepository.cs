using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using CoreSystem.Util;
using System.Linq.Expressions;
using System.Data.Common;

namespace CoreSystem.Data
{
    public interface IRepository : IDisposable
    {
        T GetById<T>(Expression<Func<T, bool>> predicate) where T : class;

        IQueryable<T> Query<T>() where T : class;

        IEnumerable<T> Execute<T>(string query, params DbParameter[] parameters) where T : class;

        void InsertOnSubmit<T>(T entity) where T : class;

        void DeleteOnSubmit<T>(T entity) where T : class; 
    }
}
