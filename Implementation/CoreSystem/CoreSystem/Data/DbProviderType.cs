using System;

namespace CoreSystem.Data
{
    /// <summary>
    /// DBMS provider enumeration
    /// </summary>
    public enum DbProviderType
    {
        UnSupported = 0,
        Oracle = 1, 
        SqlServer = 2,
        SqlServerCe = 3,
        SQLite = 4,
        MySql = 5
    }
}
