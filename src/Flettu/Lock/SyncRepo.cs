using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Flettu.Lock
{
    /// <summary>
    /// Synchronizer object based on ID
    /// </summary>
    /// <typeparam name="TID">Type of Object ID</typeparam>
    /// <typeparam name="TObject">Type of Object</typeparam>
    public class SyncRepo<TID, TObject>
    {
        #region Dispose Helper Classes

        /// <summary>
        /// Handle of object instancce 
        /// </summary>
        public class RepoHandle : IDisposable
        {
            private SyncRepo<TID, TObject> Repo { get; set; }

            /// <summary>
            /// Unique value to synchronize objects
            /// </summary>
            public TID ID { get; private set; }

            /// <summary>
            /// Instance of object handle refers to
            /// </summary>
            public TObject Object { get; private set; }

            internal RepoHandle(TID id, TObject obj, SyncRepo<TID, TObject> repo)
            {
                this.ID = id;
                this.Object = obj;
                this.Repo = repo;
            }

            /// <summary>
            /// Dispose handle reference to object
            /// </summary>
            public void Dispose()
            {
                this.Repo.Release(this);
            }

            public override string ToString()
            {
                return string.Format("SyncHandle[ID: {0}, Object: {1}]", this.ID, this.Object);
            }
        }

        #endregion

        private Dictionary<TID, TObject> repoObjects = new Dictionary<TID, TObject>();

        private Dictionary<TObject, List<RepoHandle>> repoHandles = new Dictionary<TObject, List<RepoHandle>>();

        /// <summary>
        /// Gets an exclusive object on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// Using(Sync.GetHandle(value))
        /// {
        ///     ....
        /// }
        /// </example>
        /// <param name="id">The value on which to acquire the new object.</param>
        /// <returns>Handle for lock object for K value</returns>
        public RepoHandle GetObject(TID id)
        {
            return this.GetObject(id, () => Activator.CreateInstance<TObject>());
        }

        /// <summary>
        /// Gets an exclusive object on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// Using(Sync.GetHandle(value))
        /// {
        ///     ....
        /// }
        /// </example>
        /// <param name="id">The value on which to acquire the new object.</param>
        /// <param name="generator">Method to create new object of T</param>
        /// <returns>Handle for lock object for K value</returns>
        public RepoHandle GetObject(TID id, Func<TObject> generator)
        {
            TObject obj;
            RepoHandle handle;

            lock (this.repoObjects)
            {
                if (!this.repoObjects.TryGetValue(id, out obj))
                {
                    obj = generator();
                    this.repoObjects.Add(id, obj);
                    this.repoHandles.Add(obj, new List<RepoHandle>());
                }

                handle = new RepoHandle(id, obj, this);
                this.repoHandles[obj].Add(handle);
            }

            return handle;
        }

        private void Release(RepoHandle handle)
        {
            IDisposable disposeObject = null;

            lock (this.repoObjects)
            {
                var handles = this.repoHandles[handle.Object];
                handles.Remove(handle);

                if (handles.Count == 0)
                {
                    this.repoObjects.Remove(handle.ID);
                    this.repoHandles.Remove(handle.Object);

                    disposeObject = handle.Object as IDisposable;
                }
            }

            if (disposeObject != null)
                disposeObject.Dispose();
        }

        public override string ToString()
        {
            return string.Format("SyncRepo[Count: {0}]", this.repoObjects.Count);
        }
    }
}
