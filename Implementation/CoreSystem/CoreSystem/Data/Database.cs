using System;
using System.Data.Common;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.OracleClient;


namespace CoreSystem.Data
{
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

        public static DbProviderType GetProviderType(string provider)
        {
            DbProviderType retVal;

            if (string.Compare(provider, "System.Data.OracleClient", true) == 0)
            {
                retVal = DbProviderType.Oracle;
            }
            else if (string.Compare(provider, "System.Data.SqlClient", true) == 0)
            {
                retVal = DbProviderType.SqlServer;
            }
            else if (string.Compare(provider, "System.Data.OleDb", true) == 0)
            {
                retVal = DbProviderType.OleDb;
            }
            else if (string.Compare(provider, "System.Data.OleDb", true) == 0)
            {
                retVal = DbProviderType.Odbc;
            }
            else
                throw new DbDataException("{0} is not recognized as provider name", provider);

            return retVal;
        }        

        public static string ToString(object value, TypeCode type, DbProviderType providerType)
        {
            string retVal = null;

            if (value == null || value.ToString() == "")
                return "NULL";

            if (type == TypeCode.String)
            {
                retVal = (string.Empty + value).ToString().Replace("'", "''");

                if (providerType != DbProviderType.SqlServer)
                    retVal.Replace("\"", "\"\"");

                if (retVal.Length == 0)
                    retVal = "NULL";
                else
                    retVal = string.Format("'{0}'", retVal);
            }
            else if (type == TypeCode.DateTime)
            {
                if (value.ToString().Contains("SYSDATE") || value.ToString().Contains("GETDATE"))
                    return value.ToString(); //So that We could use functions
                DateTime dtTemp = (DateTime)value;

                if (dtTemp == DateTime.MinValue)
                    return "NULL";

                switch (providerType)
                {
                    case DbProviderType.Oracle:
                        string strDate = dtTemp.ToString(Database.NetDateFormat);
                        retVal = string.Format("TO_DATE('{0}','{1}')", strDate, Database.OracleDateFormat);
                        break;
                    case DbProviderType.SqlServer:
                        retVal = string.Format("CONVERT(DATETIME,'{0}',120)", dtTemp.ToString(Database.SqlDateFormat));
                        break;
                    case DbProviderType.OleDb:
                        break;
                    case DbProviderType.Odbc:
                        break;
                    case DbProviderType.nHibernate:
                        retVal = string.Format("'{0}'", dtTemp.ToString(Database.SqlDateFormat));
                        break;
                    default:
                        throw new DbDataException("Failed to convert Date in [{0}] provider format", providerType);
                }
            }
            else
            {
                retVal = value.ToString();
            }

            return retVal;
        }

        public static string ToString(string dateStr, string dateFormat, DbProviderType providerType)
        {
            return
                ToString(DateTime.ParseExact(dateStr, dateFormat, DateTimeFormatInfo.InvariantInfo), TypeCode.DateTime,
                         providerType);
        }

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

        public string Name
        {
            get { return this.name; }
        }

        public string ConnectionString
        {
            get { return this.connectUrl; }
        }

        public DbProviderType ProviderType
        {
            get { return this.providerType; }
        }

        public string DbProvider
        {
            get { return this.dbProvider.ToString(); }
        }

        public DbConnection CreateConnection()
        {
            DbConnection retVal = this.dbProvider.CreateConnection();
            retVal.ConnectionString = this.connectUrl;
            retVal.Open();
            return retVal;
        }

        public DbCommand CreateCommand(string cmdText, DbConnection connection, CommandType cmdType)
        {
            DbCommand cmd = this.dbProvider.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = connection.ConnectionTimeout;

            return cmd;
        }

        public DbCommand CreateCommand(string cmdText, DbConnection connection)
        {
            return this.CreateCommand(cmdText, connection, CommandType.Text);
        }

        public DbCommand CreateCommand(string cmdText, DbTransaction trans)
        {
            return this.CreateCommand(cmdText, trans, CommandType.Text);           
        }

        public DbCommand CreateCommand(string cmdText, DbTransaction trans, CommandType cmdType)
        {
            DbCommand retVal = this.CreateCommand(cmdText, trans.Connection, cmdType);
            retVal.Transaction = trans;
            return retVal;
        }

        public DbCommand CreateCommand(string cmdText, CommandType cmdType)
        {
            return this.CreateCommand(cmdText, this.CreateConnection(), cmdType);
        }

        public DbCommand CreateCommand(string cmdText)
        {
            return this.CreateCommand(cmdText, this.CreateConnection(), CommandType.Text);
        }

        public DbParameter CreateParameter()
        {
            return this.dbProvider.CreateParameter();
        }

        public DbParameter CreateParameter(object value, string paramName)
        {
            if (this.ProviderType == DbProviderType.Oracle)
                return new OracleParameter(paramName, value);

            if (this.ProviderType == DbProviderType.SqlServer)
                return new SqlParameter(paramName, value);

            throw new Exception("CreateParameter method only support Oracle and Sql Server provider type");
        }

        public int ExecuteNonQuery(string cmdText, DbConnection connection)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, connection, CommandType.Text))
            {
                int retVal = cmd.ExecuteNonQuery();
                cmd.Connection = null;
                return retVal;
            }
        }

        public int ExecuteNonQuery(string cmdText)
        {
            using (DbConnection conn = this.CreateConnection())
            {
                int retVal = this.ExecuteNonQuery(cmdText, conn);
                conn.Close();
                return retVal;
            }

        }

        public int ExecuteNonQuery(DbCommand cmd)
        {
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string cmdText, DbTransaction transaction)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, transaction))
            {
                int retVal = cmd.ExecuteNonQuery();
                cmd.Connection = null;
                return retVal;
            }

        }

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

        public DbDataReader ExecuteReader(string cmdText, DbConnection connection, CommandBehavior behavior)
        {
            using (DbCommand cmd = this.CreateCommand(cmdText, connection, CommandType.Text))
            {
                DbDataReader retVal = cmd.ExecuteReader(behavior);
                cmd.Connection = null;
                return retVal;
            }
        }

        public DbDataReader ExecuteReader(string cmdText, DbConnection connection)
        {
            return this.ExecuteReader(cmdText, connection, CommandBehavior.Default);
        }

        public DbDataReader ExecuteReader(string cmdText)
        {
            return this.ExecuteReader(cmdText, this.CreateConnection(), CommandBehavior.CloseConnection);
        }

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

        public DataSet ExecuteDataSet(string cmdText)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                DataSet retVal = this.ExecuteDataSet(cmdText, connection);
                connection.Close();
                return retVal;
            }

        }

        public DataSet ExecuteDataSet(string cmdText, DbConnection connection)
        {
            return ExecuteDataSet(cmdText, connection, new DataSet("New DataSet"));
        }

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

        public DataSet ExecuteDataSet(DbCommand cmd)
        {
            DataSet retVal = new DataSet();
            DbDataAdapter adapter = this.dbProvider.CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(retVal);
            return retVal;
        }

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

        public DataTable ExecuteQuery(string cmdText)
        {
            using (DbConnection connection = this.CreateConnection())
            {
                DataTable retVal = this.ExecuteQuery(cmdText, connection);
                connection.Close();
                return retVal;
            }

        }

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

        public DbDataAdapter CreateDataAdapter(string selectQuery, CommandType cmdType)
        {
            DbDataAdapter retVal = this.dbProvider.CreateDataAdapter();
            retVal.SelectCommand = this.CreateCommand(selectQuery, cmdType);
            retVal.SelectCommand.Connection.Close();
            return retVal;
        }

        public DbDataAdapter CreateDataAdapter(string selectQuery)
        {
            return this.CreateDataAdapter(selectQuery, CommandType.Text);
        }

        public DataTable ExecuteDataTable(string cmdText, DataTable table)
        {
            DbDataAdapter adapter = this.CreateDataAdapter(cmdText);
            adapter.Fill(table);
            return table;
        }

        public DataTable ExecuteDataTable(string cmdText)
        {
            return this.ExecuteDataTable(cmdText, new DataTable());
        }

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
      
        public string ToString(object value)
        {
            if (value == null)
                return Database.ToString(value, System.TypeCode.String, this.ProviderType);
            else
                return Database.ToString(value, System.Type.GetTypeCode(value.GetType()), this.ProviderType);
        }

        public string ToUpper(string value)
        {
            return string.Format("UPPER({0})", value);
        }      
    }
}
