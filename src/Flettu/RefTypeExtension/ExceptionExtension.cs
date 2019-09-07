using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flettu.RefTypeExtension
{
    public static class ExceptionExtension
    {
        public static Exception AddData(this Exception exception, object key, object value)
        {
            exception.Data.Add(key, value);
            return exception;
        }
    }
}
