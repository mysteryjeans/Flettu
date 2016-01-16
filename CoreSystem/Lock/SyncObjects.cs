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
    /// <typeparam name="K">Type of Value</typeparam>
    /// <typeparam name="T">Type of Object</typeparam>
    public class SyncObjects<K, T> where T : IDisposable, new()
    {
        #region Lock Helper Classes

        /// <summary>
        /// Handle of object instancce 
        /// </summary>
        public class SyncHandle : IDisposable
        {
            private SyncObjects<K, T> Sync { get; set; }
			
            /// <summary>
            /// Unique value to synchronize objects
            /// </summary>
			public K Value { get; private set; }

            /// <summary>
            /// Instance of object handle refers to
            /// </summary>
            public T Object { get; private set; }

            internal SyncHandle(SyncObjects<K, T> sync, T obj)
            {
                this.Sync = sync;
                this.Object = obj;
            }

            public override string ToString()
            {
                return string.Format("SyncHandle[Value: {0}, Object: {1}]", this.Value, this.Object);
            }

            void IDisposable.Dispose()
            {
                this.Sync.Release(this);
            }
        }

        #endregion

        private Dictionary<K, T> syncObjects = new Dictionary<K, T>();
		
		private Dictionary<T, List<SyncHandle>> syncHandles = new Dictionary<T, List<SyncHandle>>();

        /// <summary>
        /// Gets an exclusive object on the specified value, must be called in 'using' statement
        /// </summary>
        /// <example>
        /// Using(Sync.GetHandle(value))
        /// {
        ///     ....
        /// }
        /// </example>
        /// <param name="value">The value on which to acquire the new object.</param>
        /// <returns>Handle for lock object for K value</returns>
        public SyncHandle GetHandle(K value)
        {
            T syncObject;
			SyncHandle handle;

            lock (this.syncObjects)
            {
                if (!this.syncObjects.TryGetValue(value, out syncObject))
                {
                    syncObject = new T();
                    this.syncObjects.Add(value, syncObject);
					this.syncHandles.Add(syncObject, new List<SyncHandle>());
                }
				
				handle = new SyncHandle(this, syncObject);
                this.syncHandles[syncObject].Add(handle);
            }

            return handle;
        }

        private void Release(SyncHandle handle)
        {
			bool disposeObject = false;
			
            lock (this.syncObjects)
            {
				var handles = this.syncHandles[handle.Object];
                handles.Remove(handle);

                if (handles.Count == 0)
				{
					this.syncObjects.Remove(handle.Value);
					this.syncHandles.Remove(handle.Object);
					
					disposeObject = true;
				}  
            }
			
			if(disposeObject)
				handle.Object.Dispose();
        }

        public override string ToString()
        {
            return string.Format("SyncObjects[Count: {0}]", this.syncObjects.Count);
        }
    }
}
