using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Flettu.Collections
{
    public class ConcurrentHashSet<T> : ISet<T>, IDisposable
    {
        private readonly HashSet<T> _hashSet = new HashSet<T>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hashSet.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }

            }
        }

        public bool IsReadOnly { get => false; }

        public ConcurrentHashSet()
        { }

        public ConcurrentHashSet(IEqualityComparer<T> comparer) => _hashSet = new HashSet<T>(comparer);

        public ConcurrentHashSet(IEnumerable<T> collection) => _hashSet = new HashSet<T>(collection);

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) => _hashSet = new HashSet<T>(collection, comparer);


        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        private bool _isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _lock.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            T[] result;

            _lock.EnterReadLock();
            try
            {
                result = _hashSet.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (var item in result)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<T>.Add(T item) => Add(item);

        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Add(item);
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
                return _hashSet.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.UnionWith(other);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.IntersectWith(other);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.ExceptWith(other);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.SymmetricExceptWith(other);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.IsSubsetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.IsSupersetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.IsProperSupersetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.IsProperSubsetOf(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Overlaps(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.SetEquals(other);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Clear();
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
                return _hashSet.Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                _hashSet.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
