using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreSystem.Dynamic;

namespace CoreSystem.RefTypeExtension
{
    public static class ObjectExtension
    {
        public static Donymous ToDonymous(this object obj)
        {
            return obj == null ? null : new Donymous(obj);
        }
    }
}
