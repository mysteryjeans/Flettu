using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace CoreSystem.Dynamic
{
    /// <summary>
    /// Expand any object to dynamic object
    /// </summary>
    public class Donymous : DynamicObject
    {
        private object obj;
        private Type objType;
        private Dictionary<string, object> memberValues = new Dictionary<string, object>();

        public Donymous(object obj)
        {
            this.obj = obj;
            this.objType = obj.GetType();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!this.memberValues.TryGetValue(binder.Name, out result))
            {
                var property = this.objType.GetProperty(binder.Name);
                if (property != null)
                {
                    result = property.GetValue(this.obj, null);
                    this.memberValues[binder.Name] = result;
                    return true;
                }
            }
            else
                return true;

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this.memberValues[binder.Name] = value;
            return true;
        }
    }
}
