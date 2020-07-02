using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Flettu.Collections
{
    public class ConcurrentList<T> : IList<T>, IDisposable
    {
        private readonly List<T> _list;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _list.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly { get => false; }

        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _list[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _list[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public ConcurrentList() => _list = new List<T>();

        public ConcurrentList(IEnumerable<T> items) => _list = new List<T>(items);

        public ConcurrentList(int capacity) => _list = new List<T>(capacity);

        public int IndexOf(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _list.IndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Insert(int index, T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Insert(index, item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.RemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _list.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _list.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _list.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            _lock.EnterReadLock();
            try
            {
                _list.CopyTo(array, index);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var item in _list)
                    yield return item;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lock != null)
                    _lock.Dispose();
            }
        }

        #endregion
    }
}
