using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreSystem.Util
{
    public static class Guard
    {
        /// <summary>
        /// Throws ArgumentNullException for null parameter
        /// </summary>
        /// <param name="parameter">Value to check against</param>
        /// <param name="message">Exception message</param>
        public static void CheckNull(object parameter, string message)
        {
            if (parameter == null)
                throw new ArgumentNullException(message);
        }

        /// <summary>
        /// First check for Null and throws ArgumentNullException otherwise throws ArgumentException for empty string
        /// </summary>
        /// <param name="parameter">Value to check against</param>
        /// <param name="message">Exception message</param>
        public static void CheckNullOrEmpty(string parameter, string message)
        {
            Guard.CheckNull(parameter, message);

            if (parameter.Length == 0)
                throw new ArgumentException(message);
        }

        /// <summary>
        /// First check for Null and throws ArgumentNullException otherwise throws ArgumentException for white space or empty string
        /// </summary>
        /// <param name="parameter">Value to check against</param>
        /// <param name="message">Exception message</param>
        public static void CheckNullOrTrimEmpty(string parameter, string message)
        {
            Guard.CheckNull(parameter, message);

            if (parameter.Trim().Length == 0)
                throw new ArgumentException(message);
        }

        /// <summary>
        /// Check using string.IsNullOrWhiteSpace and throws ArgumentException
        /// </summary>
        /// <param name="parameter">Value to check against</param>
        /// <param name="message">Exception message</param>
        public static void CheckNullOrWhiteSpace(string parameter, string message)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                throw new ArgumentException(message);
        }

        /// <summary>
        /// Throws ArgumentNullException for null array
        /// </summary>
        /// <param name="parameter">Value to check against</param>
        /// <param name="message">Exception message</param>
        public static void CheckNull(Array parameter, string message)
        {
            if (parameter == null)
                throw new ArgumentNullException(message);
        }

        /// <summary>
        /// First check for Null and throws ArgumentNullException otherwise throws ArgumentException for empty array
        /// </summary>
        /// <param name="parameter">Value to check against</param>
        /// <param name="message">Exception message</param>
        public static void CheckNullOrEmpty(Array parameter, string message)
        {
            Guard.CheckNull(parameter, message);

            if (parameter.Length == 0)
                throw new ArgumentException(message);
        }
    }
}
