using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoreSystem.Lock
{
    /// <summary>
    /// Synchronizer for value types
    /// </summary>
    /// <typeparam name="I">Type of Object ID</typeparam>
    /// <typeparam name="T">Type of Object</typeparam>
    public class SyncRepo<I, T> where T : IDisposable, new()
    {
        #region Lock Helper Classes

        /// <summary>
        /// Handle of object instancce 
        /// </summary>
        public class RepoHandle : IDisposable
        {
            private SyncRepo<I, T> SyncRepo { get; set; }
			
            /// <summary>
            /// Unique value to synchronize objects
            /// </summary>
			public I ID { get; private set; }

            /// <summary>
            /// Instance of object handle refers to
            /// </summary>
            public T Object { get; private set; }

            internal RepoHandle(SyncRepo<I, T> sync, T obj)
            {
                this.SyncRepo = sync;
                this.Object = obj;
            }

            public override string ToString()
            {
                return string.Format("SyncHandle[Value: {0}, Object: {1}]", this.ID, this.Object);
            }

            void IDisposable.Dispose()
            {
                this.SyncRepo.Release(this);
            }
        }

        #endregion

        private Dictionary<I, T> repoObjects = new Dictionary<I, T>();
		
		private Dictionary<T, List<RepoHandle>> repoHandles = new Dictionary<T, List<RepoHandle>>();

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
        public RepoHandle GetObject(I id)
        {
            T obj;
			RepoHandle handle;

            lock (this.repoObjects)
            {
                if (!this.repoObjects.TryGetValue(id, out obj))
                {
                    obj = new T();
                    this.repoObjects.Add(id, obj);
					this.repoHandles.Add(obj, new List<RepoHandle>());
                }
				
				handle = new RepoHandle(this, obj);
                this.repoHandles[obj].Add(handle);
            }

            return handle;
        }

        private void Release(RepoHandle handle)
        {
			bool disposeObject = false;
			
            lock (this.repoObjects)
            {
				var handles = this.repoHandles[handle.Object];
                handles.Remove(handle);

                if (handles.Count == 0)
				{
					this.repoObjects.Remove(handle.ID);
					this.repoHandles.Remove(handle.Object);
					
					disposeObject = true;
				}  
            }
			
			if(disposeObject)
				handle.Object.Dispose();
        }

        public override string ToString()
        {
            return string.Format("SyncRepo[Count: {0}]", this.repoObjects.Count);
        }
    }
}
