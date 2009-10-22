/*/
 *
 * Author: Faraz Masood Khan
 * 
 * Date: Friday, August 29, 2008 1:52 AM
 * 
 * Class: StringExtension
 * 
 * Email: mk.faraz@gmail.com
 * 
 * Blogs: http://farazmasoodkhan.wordpress.com
 *
 * Website: http://www.linkedin.com/in/farazmasoodkhan
 *
 * Copyright: Faraz Masood Khan @ Copyright ©  2008
 *
/*/


namespace CoreSystem.ValueTypeExtension
{
    /// <summary>
    /// This class contain extension functions for string objects
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Checks string object's value to array of string values
        /// </summary>        
        /// <param name="stringValues">Array of string values to compare</param>
        /// <returns>Return true if any string value matches</returns>
        public static bool In(this string value, params string[] stringValues) {
            foreach (string otherValue in stringValues)
                if (string.Compare(value, otherValue) == 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Converts string to enum object
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            return (T) System.Enum.Parse(typeof (T), value, true);
        }
    }
}