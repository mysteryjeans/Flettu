using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreSystem.Util
{
    public static class Guard
    {
        public static void CheckNull(object parameter, string message)
        {
            if (parameter == null)
                throw new ArgumentNullException(message);
        }

        public static void CheckNullOrEmpty(string parameter, string message)
        {
            Guard.CheckNull(parameter, message);

            if (parameter.Length == 0)
                throw new ArgumentException(message);
        }
    }
}
