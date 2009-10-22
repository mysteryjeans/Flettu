using System;

namespace CoreSystem.Data
{
    /// <summary>
    /// Use LogException for drive classes of LogHandler, 
    /// so that you can able to handle exception occuring from base classes seperately
    /// </summary>
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
