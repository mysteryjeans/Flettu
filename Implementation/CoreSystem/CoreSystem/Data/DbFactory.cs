using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace CoreSystem.Data
{
    /// <summary>
    /// Factory class for creating Database class instances foreach connection string defined in configuration file
    /// </summary>
    /// <remarks>
    /// It initialize in static constructor
    /// </remarks>
    /// <see cref="Database"/>
    public static class DbFactory
    {
        private const string DEFAULT_DB = "default-db";

        private static Database defaultDb;
        private static List<Database> databaseList= new List<Database>();

        /// <summary>
        /// Creates Database class instances againts each connection string defined in config file
        /// </summary>
        /// <see cref="Database"/>
        static DbFactory()
        {
            foreach (ConnectionStringSettings configSettings in ConfigurationManager.ConnectionStrings)
            {
                Database database = new Database(configSettings);                
                databaseList.Add(database);
                
                if (!string.IsNullOrEmpty(configSettings.Name) 
                    && configSettings.Name.ToLower() == DEFAULT_DB)
                
                    defaultDb = database;
            }

            if (defaultDb == null && databaseList.Count != 0)
                defaultDb = databaseList[0];
        }
        
        /// <summary>
        /// Gets default Database instance which with connection string name: Default-Db
        /// </summary>
        /// <returns>Default Database object</returns>
        /// <remarks>
        /// If no connection string defined in config file with name 'Default-Db' 
        /// than Database object of the first connection string will be returned
        /// </remarks>
        /// <see cref="Database"/>
        public static Database GetDefaultDatabase()
        {
            return defaultDb;
        }

        /// <summary>
        /// Get Database class instance of connection string with specified name
        /// </summary>
        /// <param name="name">Name of connection string</param>
        /// <returns>Database class instance</returns>
        /// <see cref="Database"/>
        public static Database GetDatabase(string name)
        {  
            foreach (Database database in databaseList)
                if (database.Name == name)
                    return database;

            throw new InvalidOperationException(string.Format("Unable to find database: {0}", name));
        }
    }
}
