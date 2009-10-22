/*
 * Author: Faraz Masood Khan 
 * 
 * Date: 6/4/2009 4:33:36 PM
 * 
 * Class: DbConvert
 * 
 * Copyright: Faraz Masood Khan @ Copyright ©  2009
 * 
 * Email: mk.faraz@gmail.com
 * 
 * Blogs: http://csharplive.wordpress.com, http://farazmasoodkhan.wordpress.com
 * 
 */

using System;

namespace CoreSystem.Data
{
    /// <summary>
    /// Class utility for converting database value in CLR value or vice versa
    /// </summary>
    public static class DbConvert
    {
        public static string ToString(object dbValue)
        {
            return (dbValue == DBNull.Value || dbValue == null) ? null : dbValue.ToString();
        }

        public static int ToInt32(object value)
        {
            try
            {
                if (value == null || value.ToString().Equals(string.Empty))
                    return default(int);

                return Convert.ToInt32(value);
            } // Remove general catch
            catch (ArgumentException)
            {
                return default(int);
            }
            catch (OverflowException)
            {
                return default(int);
            }
            catch (FormatException)
            {
                return default(int);
            }
        }

        public static long ToInt64(object value)
        {
            try
            {
                if (value == null || value.ToString().Equals(string.Empty))
                    return default(long);

                return Convert.ToInt32(value);
            }
            catch (ArgumentException)
            {
                return default(long);
            }
            catch (OverflowException)
            {
                return default(long);
            }
            catch (FormatException)
            {
                return default(long);
            }
        }

        public static bool ToBoolean(object value)
        {
            try
            {
                if (value == null || value.ToString().Equals(string.Empty))
                    return default(bool);

                return Convert.ToBoolean(value);
            }
            catch (InvalidCastException)
            {
                return default(bool);
            }
        }

        public static decimal ToDecimal(object value)
        {
            try
            {
                if (value == null || value.ToString().Equals(string.Empty))
                    return default(decimal);

                return (decimal)value;
                //return Convert.ToDecimal(value);
            }
            catch
            {
                return default(decimal);
            }
        }

        public static DateTime ToDateTime(object value, DateTime defValue)
        {
            try
            {
                if (value == null || value.ToString() == null || value.ToString().Equals(string.Empty))
                    return defValue;

                return Convert.ToDateTime(value);
            }
            catch (InvalidCastException)
            {
                return defValue;
            }
        }

        public static T? ToNullable<T>(object value) where T : struct
        {
            if (value == null || value is DBNull)
                return new Nullable<T>();

            try { return new Nullable<T>((T)value); }
            catch { return new Nullable<T>(); }
        }

        public static DateTime ToDateTime(object value)
        {
            return ToDateTime(value, default(DateTime));
        }

        public static T ToEnum<T>(object value, T defVal)
            where T : struct
        {
            try { return ToEnum<T>(value); }
            catch { }

            return defVal;
        }

        public static T ToEnum<T>(object value)
            where T : struct
        {
            string strValue = value + "";
            return (T)Enum.Parse(typeof(T), strValue, true);
        }

        public static object DbValue<T>(T? value) where T : struct
        {
            return value.HasValue ? (object)value.Value : (object)DBNull.Value;
        }

        /// <summary>
        /// Return DBNull if value is null
        /// </summary>
        /// <param name="value"></param>
        /// <returns>value or of DBNull</returns>
        /// <remarks>This function will also work with Nullable&ltT&gt</remarks>
        public static object DbValue(object value)
        {
            return (value == null) ? DBNull.Value : value;
        }
    }
}
