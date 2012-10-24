using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreSystem.Lock
{
    /// <summary>
    /// Unique object for value of T
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    public class SyncValue<T> where T : struct
    {
        private Dictionary<T, object> syncObjects = new Dictionary<T, object>();

        /// <summary>
        /// Unique object for value of T
        /// </summary>
        /// <param name="value">The value to acquire unique object</param>
        /// <returns>Unique object for value</returns>
        public object GetObject(T value)
        {
            lock (this.syncObjects)
            {
                object syncObject;
                if (!this.syncObjects.TryGetValue(value, out syncObject))
                    this.syncObjects.Add(value, syncObject = new object());

                return syncObject;
            }
        }
    }
}
