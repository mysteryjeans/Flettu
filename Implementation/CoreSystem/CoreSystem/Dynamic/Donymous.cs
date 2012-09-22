using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data.Common;
using System.Reflection;

namespace CoreSystem.Dynamic
{
    /// <summary>
    /// Object with dynamic properties, it can initialize from any static type object or DbDataReader
    /// </summary>
    public class Donymous : DynamicObject
    {
        private Dictionary<string, object> memberValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Constructor for Expandle object
        /// </summary>
        public Donymous()
        { }

        /// <summary>
        /// Load all values from DbDataReader
        /// </summary>
        /// <param name="reader">Reader from which to load all column values</param>
        public Donymous(DbDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                this.memberValues[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        /// <summary>
        /// Load all public properties from object
        /// </summary>
        /// <param name="obj">Object from which to load all public property values</param>
        public Donymous(object obj)
        {
            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public))
                this.memberValues[property.Name] = property.GetValue(obj, null);
        }

        /// <summary>
        /// Get or Set value of arbitrary property
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>Value of property</returns>
        public object this[string name]
        {
            get { return this.memberValues[name]; }
            set { this.memberValues[name] = value; }
        }

        /// <summary>
        /// Try to get value from properties in dictionary collection
        /// </summary>
        /// <param name="binder">Name of the property</param>
        /// <param name="result">Value of the property of found</param>
        /// <returns>Returns true if value is found</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return this.memberValues.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// Returns all properties of Donymous object
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach(var name in this.memberValues.Keys)
                yield return name;
        }

        /// <summary>
        /// Set arbitrary property
        /// </summary>
        /// <param name="binder">Name of arbitrary property</param>
        /// <param name="value">Value for arbitrary property</param>
        /// <returns>Returns true after setting property</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }
    }
}
