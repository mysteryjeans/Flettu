using System;
using System.Collections.Generic;

namespace CoreSystem
{
	 /// <summary>
    /// Unique object for string value
    /// </summary>    
    public class SyncString
    {	
		/// <summary>
		/// Gets a value indicating whether this string value comparasion is in sensitive.
		/// </summary>
		/// <value>
		/// <c>true</c> if in sensitive; otherwise, <c>false</c>.
		/// </value>
		public bool InSensitive {get; private set;}
		
		private readonly Dictionary<string, object> syncObjects;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CoreSystem.SyncString"/> class.
		/// </summary>
		/// <param name='inSensitive'>If false string comparasion will be case sensitive</param>
		public SyncString (bool inSensitive=true)
		{
			this.InSensitive = inSensitive;
			this.syncObjects = inSensitive ? new Dictionary<string, object> (StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object> ();
		}
		
        /// <summary>
        /// Unique object for string value
        /// </summary>
        /// <param name="value">The value to acquire unique object</param>
        /// <returns>Unique object for string value</returns>
        public object GetObject(string value)
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

