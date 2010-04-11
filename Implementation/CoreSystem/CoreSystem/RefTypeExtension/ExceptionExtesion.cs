using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreSystem.RefTypeExtension
{
    public static class ExceptionExtesion
    {
        public static Exception AddData(this Exception exception, object key, object value)
        {
            exception.Data.Add(key, value);
            return exception;
        }
    }
}
