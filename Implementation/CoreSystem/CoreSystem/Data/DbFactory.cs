using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace CoreSystem.Data
{
    public static class DbFactory
    {
        private const string DEFAULT_DB = "default-db";

        private static Database defaultDb;
        private static List<Database> databaseList= new List<Database>();

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

        public static Database GetDefaultDatabase()
        {
            return defaultDb;
        }

        public static Database GetDatabase(string name)
        {  
            foreach (Database database in databaseList)
                if (database.Name == name)
                    return database;

            throw new InvalidOperationException(string.Format("Unable to find database: {0}", name));
        }
    }
}
