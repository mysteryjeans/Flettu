using System;

namespace CoreSystem.Data
{
    /// <summary>
    /// Exception class for all database exceptions
    /// </summary>
    /// <remarks>Exception return by Database class</remarks>
    /// <see cref="Database"/>
    public class DbDataException : System.Exception
    {
        public DbDataException(string msgFormat, params object[] args)
            : base(string.Format(msgFormat, args))
        { }

        public DbDataException(Exception innerExcep, string message)
            : base(message, innerExcep)
        { }
        public DbDataException(Exception innerExcep, string msgFormat, params object[] args)
            : base(string.Format(msgFormat, args), innerExcep)
        { }
    }
}
