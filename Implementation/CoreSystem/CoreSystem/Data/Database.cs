using System;
using System.Data.Common;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Data.SqlClient;


namespace CoreSystem.Data
{
    /// <summary>
    /// This class hide underlining DBMS provider and allows to work on abstract interfaces
    /// </summary>
    /// <remarks>This is very helpful to reduce boiler plate code</remarks>
    public class Database
    {
        public const string NetDateFormat = "yyyy-MM-dd HH:mm:ss";
        public const string SqlDateFormat = "yyyy-MM-dd HH:mm:ss";
        public const string OracleDateFormat = "yyyy-MM-dd HH24:MI:SS";

        private DbProviderFactory dbProvider;
        private string name;
        private string connectUrl;
        private DbProviderType providerType;

        public Database(string name, string provider, string connectUrl)
        {
            this.Init(name, provider, connectUrl);
        }

        public Database(ConnectionStringSettings settings)
            : this(settings.Name, settings.ProviderName, settings.ConnectionString)
        { }

        public Database(string name, string provider, string connectUrl, DbProviderFactory providerFactory)
        {
            this.Init(name, provider, connectUrl, providerFactory);
        }

        /// <summary>
        /// Gets DbProviderType for provider name
        /// </summary>
        /// <param name="provider">Provide name i.e. System.Data.OracleClient </param>
        /// <returns>DbProviderType for provider name</returns>
        public static DbProviderType GetProviderType(string provider)
        {
            if (string.Compare(provider, "System.Data.OracleClient", true) == 0)
                return DbProviderType.Oracle;

            if (string.Compare(provider, "System.Data.SqlClient", true) == 0)
                return DbProviderType.SqlServer;

            if (string.Compare(provider, "System.Data.SqlServerCe.3.5", true) == 0)
                return DbProviderType.SqlServerCe;

            if (string.Compare(provider, "System.Data.SqlServerCe.3.5", true) == 0)
                return DbProviderType.SQLite;

            return DbProviderType.UnSupported;
        }

        /// <summary>
        /// Converts to DBMS acceptable datetime format
        /// </summary>
        /// <param name="value">DateTime value</param>
        /// <param name="providerType">DBMS provider type</param>
        /// <returns>Datetime in string format</returns>
        public static string ToDate(DateTime value, DbProviderType providerType)
        {
            switch (providerType)
            {
                case DbProviderType.Oracle:
                    string strDate = value.ToString(Database.NetDateFormat);
                    return string.Format("TO_DATE('{0}','{1}')", strDate, Database.OracleDateFormat);                    
                case DbProviderType.SqlServer:
                    return string.Format("CONVERT(DATETIME,'{0}',120)", value.ToString(Database.SqlDateFormat));                
                default:
                    throw new DbDataException("Failed to convert Date for provider [{0}]", providerType);
            }
        }

        /// <summary>
        /// Converts datetime string to DBMS acceptable datetime format
        /// </summary>
        /// <param name="dateStr">DateTime string</param>
        /// <param name="dateFormat">Format of datetime string</param>
        /// <param name="providerType">DBMS provider type</param>
        /// <returns>Datetime in string format</returns>
        public static string ToDate(string dateStr, string dateFormat, DbProviderType providerType)
        {
            return ToDate(DateTime.ParseExact(dateStr, dateFormat, DateTimeFormatInfo.InvariantInfo), providerType);
        }
        
        /// <summary>
        /// Name of Database instance
        /// </summary>
        /// <remarks>Same as connection string name</remarks>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString
        {
            get { return this.connectUrl; }
        }

        /// <summary>
        /// Provider of connection string
        /// </summary>
        public DbProviderType ProviderType
        {
            get { return this.providerType; }
        }

        /// <summary>
        /// Provider information
        /// </summary>
        public string DbProvider
        {
            get { return this.dbProvider.ToString(); }
        }

        /// <summary>
        /// Creates a new connection of database
        /// </summary>
        /// <returns>New opened database connection</returns>
        public DbConnection CreateConnection()
        {
            DbConnection retVal = this.dbProvider.CreateConnection();
            retVal.ConnectionString = this.connectUrl;
            retVal.Open();
            return retVal;
        }

        /// <summary>
        /// Creates a new specific provider commands object
        /// </summary>
        /// <param name="cmdText">SQL command</param>
        /// <param name="connection">Connection of database</param>
        /// <param name="cmdType">Command type</param>
        /// <returns>New database command object</returns>
        public DbCommand CreateCommand(string cmdText, DbConnection connection, CommandType cmdType)
        {
            DbCommand cmd = this.dbProvider.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = connection.ConnectionTimeout;

            return cmd;
        }

        /// <summary>
        /// Creates new database command object for text command
        /// </summary>
        /// <param name="cmdText">SQL text command</param>
        /// <param name="connection">Database connection</param>
        /// <returns>New database command object</returns>
        public DbCommand CreateCommand(string cmdText, DbConnection connection)
        {
            return this.CreateCommand(cmdText, connection, CommandType.Text);
        }

        /// <summary>
        /// Creates new database command object and attaches it with transaction
        /// </summary>
        /// <param name="cmdText">SQL text command</param>
        /// <param name="trans">Transaction to attach new command</param>
        /// <returns>New database command object</returns>
        public DbCommand CreateCommand(string cmdText, DbTransaction trans)
        {
            return this.CreateCommand(cmdText, trans, CommandType.Text);
        }

        /// <summary>
        /// Creates new database command object and attaches it with transaction
        /// </summary>
        /// <param name="cmdText">SQL command</param>
        /// <param name="trans">Transaction to attach new command</param>
        /// <param name="cmdType">SQL command type (Text, Procedure ...)</param>
        /// <returns>New database command object</returns>
        public DbCommand CreateCommand(string cmdText, DbTransaction trans, CommandType cmdType)
        {
            DbCommand retVal = this.CreateCommand(cmdText, trans.Connection, cmdType);
            retVal.Transaction = trans;
            return retVal;
        }

        /// <summary>
        /// Creates a new database command with a new connection object
        /// </summary>
        /// <param name="cmdText">SQL command</param>
        /// <param name="cmdType">Command type (Text, Procedure ...)</param>
        /// <returns>New database command object</returns>
        /// <remarks>Connection should be dispose by caller</remarks>
        public DbCommand CreateCommand(string cmdText, CommandType cmdType)
        {
            return this.CreateCommand(cmdText, this.CreateConnection(), cmdType);
        }

        /// <summary>
        /// Creates a new database command with a new connection object
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns>New database command object</returns>
        public DbCommand CreateCommand(string cmdText)
        {
            return this.CreateCommand(cmdText, this.CreateConnection(), CommandType.Text);
        }

        /// <summary>
        /// Creates a new database parameter
        /// </summary>
        /// <returns>New database parameter</returns>
        public DbParameter CreateParameter()
        {
            return this.dbProvider.CreateParameter();
        }

        /// <summary>
        /// Creates a new database parameter
        /// </summary>
        /// <param name="value">Value for parameter</param>
        /// <param name="paramName">Parameter name</param>
        /// <returns>New database parameter</returns>
        public DbParameter CreateParameter(object value, string paramName)
        {
            DbParameter parameter = this.dbProvider.CreateParameter();
            parameter.ParameterName = paramName;
            parameter.Value = value;

            return parameter;
        }

        /// <summary>
        /// Executes non query command
        /// </summary>
        /// <param name="cmdText">Non query string</param>
        /// <param name="connection">Database open connection</param>
        /// <returns>Number of rows effect</returns>
        public int ExecuteNonQuery(string cmdText, DbConnection connection)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, connection, CommandType.Text))
            {
                int retVal = cmd.ExecuteNonQuery();
                cmd.Connection = null;
                return retVal;
            }
        }

        /// <summary>
        /// Executes non query command
        /// </summary>
        /// <param name="cmdText">Non query string</param>
        /// <returns>Number of rows effected</returns>
        public int ExecuteNonQuery(string cmdText)
        {
            using (DbConnection conn = this.CreateConnection())
            {
                int retVal = this.ExecuteNonQuery(cmdText, conn);
                conn.Close();
                return retVal;
            }

        }
        
        /// <summary>
        /// Executes non query command in transaction
        /// </summary>
        /// <param name="cmdText">Non query string</param>
        /// <param name="transaction">Database transaction</param>
        /// <returns>Number of rows effected</returns>
        public int ExecuteNonQuery(string cmdText, DbTransaction transaction)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, transaction))
            {
                int retVal = cmd.ExecuteNonQuery();
                cmd.Connection = null;
                return retVal;
            }

        }

        /// <summary>
        /// Executes query in transaction
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="trans">Database transaction</param>
        /// <returns>Open data reader</returns>
        public DbDataReader ExecuteReader(string cmdText, DbTransaction trans)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, trans))
            {
                DbDataReader retVal = cmd.ExecuteReader();
                cmd.Transaction = null;
                cmd.Connection = null;
                return retVal;
            }
        }

        /// <summary>
        /// Executes SQL query
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="connection">Database open connection</param>
        /// <param name="behavior">Command behavoir after execution</param>
        /// <returns>Open data reader</returns>
        public DbDataReader ExecuteReader(string cmdText, DbConnection connection, CommandBehavior behavior)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, connection, CommandType.Text))
            {
                DbDataReader retVal = cmd.ExecuteReader(behavior);
                cmd.Connection = null;
                return retVal;
            }
        }

        /// <summary>
        /// Executes SQL query
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="connection">Database open connection</param>
        /// <returns>Open data reader</returns>
        public DbDataReader ExecuteReader(string cmdText, DbConnection connection)
        {
            return this.ExecuteReader(cmdText, connection, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes query string
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <returns>Open data reader</returns>
        public DbDataReader ExecuteReader(string cmdText)
        {
            return this.ExecuteReader(cmdText, this.CreateConnection(), CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Execute query string
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="connection">Database connection</param>
        /// <returns>Result in a new DataTable</returns>
        public DataTable ExecuteQuery(string cmdText, DbConnection connection)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, connection, CommandType.Text))
            {
                DbDataAdapter adapter = this.dbProvider.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                DataTable retVal = new DataTable();

                adapter.Fill(retVal);
                cmd.CommandText = string.Empty;
                cmd.Connection = null;
                return retVal;
            }

        }

        /// <summary>
        /// Executes query string
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <returns>Result in a new DataSet</returns>
        public DataSet ExecuteDataSet(string cmdText)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                DataSet retVal = this.ExecuteDataSet(cmdText, connection);
                connection.Close();
                return retVal;
            }

        }

        /// <summary>
        /// Executes query string
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="connection">Database connection</param>
        /// <returns>Result in a new DataSet</returns>
        public DataSet ExecuteDataSet(string cmdText, DbConnection connection)
        {
            return ExecuteDataSet(cmdText, connection, new DataSet("New DataSet"));
        }

        /// <summary>
        /// Executes query string
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="connection">Database connection</param>
        /// <param name="retVal">DataSet in which result should be reflected</param>
        /// <returns>DataSet with query result</returns>
        public DataSet ExecuteDataSet(string cmdText, DbConnection connection, DataSet retVal)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, connection, CommandType.Text))
            {
                DbDataAdapter adapter = this.dbProvider.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(retVal);
                cmd.CommandText = string.Empty;
                cmd.Connection = null;
                return retVal;
            }

        }

        /// <summary>
        /// Executes database command
        /// </summary>
        /// <param name="cmd">Database command object</param>
        /// <returns>Result in a new DataSet</returns>
        public DataSet ExecuteDataSet(DbCommand cmd)
        {
            DataSet retVal = new DataSet();
            DbDataAdapter adapter = this.dbProvider.CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(retVal);
            return retVal;
        }

        /// <summary>
        /// Executes query string
        /// </summary>
        /// <param name="cmdText">SQL query strign</param>
        /// <param name="retVal">DataSet in which result should be reflected</param>
        /// <returns>DataSet with new result</returns>
        public DataSet ExecuteDataSet(string cmdText, DataSet retVal)
        {
            DbConnection conn = this.CreateConnection();
            using (DbCommand cmd = this.CreateCommand(cmdText, conn, CommandType.Text))
            {
                DbDataAdapter adapter = this.dbProvider.CreateDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(retVal);
                cmd.CommandText = string.Empty;
                cmd.Connection = null;
                conn.Close();
                return retVal;
            }

        }

        /// <summary>
        /// Executes set of query strings
        /// </summary>
        /// <param name="querySet">SQL query strings</param>
        /// <param name="retVal">DataSet in which result should be reflected</param>
        /// <returns>DataSet with results of each query</returns>
        public DataSet ExecuteDataSet(string[] querySet, DataSet retVal)
        {
            using (DbConnection conn = this.CreateConnection())
            {
                using (DbCommand cmd = this.CreateCommand("", conn, CommandType.Text))
                {
                    DbDataAdapter adapter = this.dbProvider.CreateDataAdapter();
                    adapter.SelectCommand = cmd;
                    for (int i = 0; i < querySet.Length; i++)
                    {
                        cmd.CommandText = querySet[i];
                        adapter.Fill(retVal, string.Format("Table{0}", i));
                    }
                    cmd.CommandText = string.Empty;
                    cmd.Connection = null;
                    conn.Close();
                    return retVal;
                }
            }
        }

        /// <summary>
        /// Execute query string
        /// </summary>
        /// <param name="cmdText">SQL query command</param>
        /// <returns>Result in a new DataTable</returns>
        public DataTable ExecuteQuery(string cmdText)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                DataTable retVal = this.ExecuteQuery(cmdText, connection);
                connection.Close();
                return retVal;
            }

        }

        /// <summary>
        /// Get table schema of specified table
        /// </summary>
        /// <param name="TableName">Table name</param>
        /// <returns>Table schema in DataTable</returns>
        public DataTable GetDbSchemaTable(string TableName)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                DataTable dt = null;
                string[] restrictions = null;
                string UserID = string.Empty;

                if (this.ProviderType == DbProviderType.Oracle)
                {
                    #region DBOwner ID

                    foreach (string str in this.ConnectionString.Split(';'))
                    {
                        if (str.Contains("user Id"))
                        {
                            try
                            {
                                UserID = str.Substring(str.IndexOf("=") + 1, str.Length - (str.IndexOf("=") + 1));
                                UserID = UserID.Trim().ToUpper();
                            }

                            catch
                            {
                                UserID = string.Empty;
                            }
                        }
                    }

                    #endregion

                    restrictions = new string[3] { UserID, TableName, null };
                }
                else if (this.ProviderType == DbProviderType.SqlServer)
                {
                    restrictions = new string[4] { connection.Database, null, TableName, null };
                }

                try
                {
                    dt = connection.GetSchema("Columns", restrictions);
                }
                catch (Exception exc)
                {
                    string err = exc.Message;
                    connection.Close();
                }

                return dt;
            }
        }

        /// <summary>
        /// Creates a new data adapter for specified select query
        /// </summary>
        /// <param name="selectQuery">SQL query string</param>
        /// <param name="cmdType">Command type</param>
        /// <returns>New data adapter</returns>
        public DbDataAdapter CreateDataAdapter(string selectQuery, CommandType cmdType)
        {
            DbDataAdapter retVal = this.dbProvider.CreateDataAdapter();
            retVal.SelectCommand = this.CreateCommand(selectQuery, cmdType);
            retVal.SelectCommand.Connection.Close();
            return retVal;
        }

        /// <summary>
        /// Creates a new data adapter for select query
        /// </summary>
        /// <param name="selectQuery">SQL select query</param>
        /// <returns>New data adapter</returns>
        public DbDataAdapter CreateDataAdapter(string selectQuery)
        {
            return this.CreateDataAdapter(selectQuery, CommandType.Text);
        }

        /// <summary>
        /// Executes query string and fill specified DataTable
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <param name="table">DataTable which should be filled</param>
        /// <returns>Filled DataTable</returns>
        public DataTable ExecuteDataTable(string cmdText, DataTable table)
        {
            DbDataAdapter adapter = this.CreateDataAdapter(cmdText);
            adapter.Fill(table);
            return table;
        }

        /// <summary>
        /// Executes query string and return DataTable
        /// </summary>
        /// <param name="cmdText">SQL query string</param>
        /// <returns>Result in a new DataTable</returns>
        public DataTable ExecuteDataTable(string cmdText)
        {
            return this.ExecuteDataTable(cmdText, new DataTable());
        }

        /// <summary>
        /// Executes query string and return scalar value
        /// </summary>
        /// <param name="cmdText">SQL query strign</param>
        /// <returns>Scalar value</returns>
        public object ExecuteScalar(string cmdText)
        {
            using (DbConnection conn = this.CreateConnection())
            {
                using (DbCommand cmd = this.CreateCommand(cmdText, conn))
                {
                    return cmd.ExecuteScalar();
                }
            }
        }
      
        /// <summary>
        /// Wrap value to DBMS UPPER function
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Value in UPPER function</returns>
        public static string ToUpper(string value)
        {
            return string.Format("UPPER({0})", value);
        }

        /// <summary>
        /// Initialized Database class instance
        /// </summary>
        /// <param name="name">Name of connection string</param>
        /// <param name="provider">Provider name i.e. System.Data.SqlClient</param>
        /// <param name="connectUrl">Database connection string</param>
        private void Init(string name, string provider, string connectUrl)
        {            
            try
            {
                this.name = name;
                this.dbProvider = DbProviderFactories.GetFactory(provider);
                this.providerType = Database.GetProviderType(provider);
                this.connectUrl = connectUrl;
            }
            catch (Exception excep)
            {
                throw new DbDataException(excep, "Failed to instantiate database from configuration file. Section: {0}", this.name);
            }
        }

        /// <summary>
        /// Initialized Database class instance
        /// </summary>
        /// <param name="name">Name of connection string</param>
        /// <param name="provider">Provider name i.e. System.Data.SqlClient</param>
        /// <param name="connectUrl">Database connection string</param>
        private void Init(string name, string provider, string connectUrl, DbProviderFactory providerFactory)
        {
            try
            {
                this.name = name;
                this.dbProvider = providerFactory;
                this.providerType = Database.GetProviderType(provider);
                this.connectUrl = connectUrl;
            }
            catch (Exception excep)
            {
                throw new DbDataException(excep, "Failed to instantiate database from configuration file. Section: {0}", this.name);
            }
        }

    }
}
