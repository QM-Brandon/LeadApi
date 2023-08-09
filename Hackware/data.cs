using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;

namespace Support.Data
{
    // Abstract Data Access base class
    //Public MustInherit Class DataAccessBase
    //    Protected ConnectionString As String
    //    Public MustOverride Function OpenConnection() As IDbConnection
    //    Public MustOverride Function CreateCommand(ByVal cmdText As String, ByVal connection As IDbConnection) As IDbCommand
    //    Public MustOverride Function GetDataSet(ByVal command As String, ByVal tableName As String) As DataSet
    //    Public MustOverride Function SaveDataSet(ByVal ds As DataSet) As Integer
    //End Class 'DataAccessBase

    public class TxnConnection : IDisposable
    {
        public SqlConnection SqlConnection { get; private set; }
        public SqlTransaction SqlTransaction { get; private set; }
        public int? CommandTimeout { get; set; }
        public IsolationLevel IsolationLevel { get; set; }

        public bool IsTransactionStarted
        {
            get { return SqlTransaction != null; }
        }

        public TxnConnection()
            : this(WebConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString)
        {
        }

        public TxnConnection(string connString)
        {
            this.SqlConnection = new SqlConnection(connString);
            this.CommandTimeout = 240;
        }

        public void Open()
        {
            if (this.SqlConnection.State != ConnectionState.Open)
            {
                this.SqlConnection.Open();
            }
        }

        public void Close()
        {
            if (this.SqlConnection != null)
            {
                this.SqlConnection.Close();
            }

            this.SqlTransaction = null;
        }

        public void BeginTransaction()
        {
            if (this.SqlTransaction != null)
            {
                CommitTransaction();
                // SHOULD CONSIDER: What should the default action be in this case?
                // Commit, Rollback, or throw an exception indicating that one is already in progress?
            }

            if (this.IsolationLevel == null)
            {
                this.SqlTransaction = this.SqlConnection.BeginTransaction();
            }
            else
            {
                this.SqlTransaction = this.SqlConnection.BeginTransaction(this.IsolationLevel);
            }
        }

        public void CommitTransaction()
        {
            this.SqlTransaction.Commit();

            this.SqlTransaction.Dispose();
            this.SqlTransaction = null;
        }

        public void RollbackTransaction()
        {
            this.SqlTransaction.Rollback();

            this.SqlTransaction.Dispose();
            this.SqlTransaction = null;
        }

        public void Dispose()
        {
            if (this.SqlTransaction != null)
            {
                this.SqlTransaction.Dispose();
            }

            if (this.SqlConnection != null)
            {
                this.SqlConnection.Dispose();
            }

            this.SqlTransaction = null;
        }

        public SqlCommand CreateCommand(string commandText)
        {
            SqlCommand cmd = this.SqlConnection.CreateCommand();
            cmd.CommandText = commandText;

            if (CommandTimeout.HasValue) cmd.CommandTimeout = CommandTimeout.Value;
            if (IsTransactionStarted) cmd.Transaction = SqlTransaction;

            return cmd;
        }

        public void BindCommand(SqlCommand cmd)
        {
            cmd.Connection = this.SqlConnection;
            if (CommandTimeout.HasValue) cmd.CommandTimeout = CommandTimeout.Value;
            if (this.SqlTransaction != null) cmd.Transaction = this.SqlTransaction;
        }
    }

    public abstract class ElmahLogger
    {
        protected void LogError(Exception ex)
        {
            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        }
    }

    // SQL Server Data Reader class
    public class DataReaderSql : ElmahLogger
    {
        private TxnConnection _Connection;
        private bool _InternalConnection;

        public TxnConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    _Connection = new TxnConnection();
                    _InternalConnection = true;
                }

                return _Connection;
            }

            set
            {
                this._Connection = value;
                _InternalConnection = false;
            }
        }

        public DataReaderSql()
        {
        }

        public DataReaderSql(TxnConnection txnConnection)
        {
            Connection = txnConnection;
        }

        public SqlDataReader SQLExecute(string SQLString)
        {
            Connection.Open();
            SqlCommand SqlCmd = Connection.CreateCommand(SQLString);
            return SqlCmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        //SQLExecute
        public int SQLExecuteNonQuery(string SQLString)
        {
            Connection.Open();
            SqlCommand SqlCmd = Connection.CreateCommand(SQLString);

            int Count = SqlCmd.ExecuteNonQuery();
            if (_InternalConnection) Connection.Close();

            return Count;
        }

        //SQLExecuteNonQuery
        public int SQLExecuteScalar(string command)
        {
            int intResult = 0;
            Connection.Open();
            try
            {
                SqlCommand oCommand = Connection.CreateCommand(command);
                intResult = Convert.ToInt32(oCommand.ExecuteScalar());
            }
            catch (Exception ex)
            {
                var exception = new Exception(string.Format("{0} command is \"({1})\".", ex.Message, command), ex);
                LogError(exception);
                //HttpContext.Current.Trace.Warn("Unable to retrieve records. Message: " & ex.Message)
                intResult = -1;
            }
            finally
            {
                if (_InternalConnection) Connection.Close();
            }

            return intResult;
        }

        public string SQLExecuteScalarString(string command)
        {
            string result = null;
            Connection.Open();
            try
            {
                SqlCommand oCommand = Connection.CreateCommand(command);
                result = oCommand.ExecuteScalar() as string;
            }
            catch (Exception ex)
            {
                //HttpContext.Current.Trace.Warn("Unable to retrieve records. Message: " & ex.Message)
                var exception = new Exception(string.Format("{0} command is \"({1})\".", ex.Message, command), ex);
                LogError(exception);
                result = null;
            }
            finally
            {
                if (_InternalConnection) Connection.Close();
            }

            return result;
        }
    }

    // SQL Server Data Access object
    public class SQLDataAccess : ElmahLogger
    {
        private TxnConnection _Connection;
        private bool _InternalConnection;

        protected SqlDataAdapter DataAdapter;

        public TxnConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    _Connection = new TxnConnection();
                    _InternalConnection = true;
                }

                return _Connection;
            }

            set
            {
                this._Connection = value;
                _InternalConnection = false;
            }
        }

        public SQLDataAccess() : this(null)
        {
        }

        public SQLDataAccess(TxnConnection txnConnection)
        {
            this.Connection = txnConnection;
        }

        public DataTable GetDataTable(string command)
        {
            return GetDataTable(command, true); // default to true for backward compatibility
        }

        public DataTable GetDataTable(string command, bool supressException)
        {
            /// Create the Connection and Data Adapter objects
            Connection.Open();

            DataTable dt = new DataTable();
            try
            {
                SqlCommand oCommand = Connection.CreateCommand(command);
                DataAdapter = new SqlDataAdapter(oCommand);
                DataAdapter.Fill(dt);

                //HttpContext.Current.Trace.Warn("Records Retrieved = " & dt.Rows.Count)
            }
            catch (Exception ex)
            {
                if (!supressException)
                {
                    throw;
                }
                else
                {
                    var exception = new Exception(string.Format("{0} command is \"({1})\".", ex.Message, command), ex);
                    LogError(exception);
                }
                //HttpContext.Current.Trace.Warn("Unable to retrieve records. Message: " & ex.Message)
            }
            finally
            {
                if (_InternalConnection)
                {
                    Connection.Close();
                }
            }

            return dt;
        }

        public DataTable GetDataTableWithSQLParms(string command, Hashtable htSQLParms)
        {
            /// Create the Connection and Data Adapter objects
            Connection.Open();
            DataTable dt = new DataTable();
            try
            {
                SqlCommand oCommand = Connection.CreateCommand(command);
                DataAdapter = new SqlDataAdapter(oCommand);

                foreach (DictionaryEntry deItem in htSQLParms)
                {
                    SqlParameter spParm = new SqlParameter();
                    spParm.ParameterName = "@" + deItem.Key;
                    spParm.Value = deItem.Value;
                    oCommand.Parameters.Add(spParm);
                }

                DataAdapter.Fill(dt);

                //HttpContext.Current.Trace.Warn("Records Retrieved = " & dt.Rows.Count)
            }
            catch (Exception ex)
            {
                //HttpContext.Current.Trace.Warn("Unable to retrieve records. Message: " & ex.Message)
                HttpContext.Current.Trace.Warn("Unexpected error encountered in GetDataSetWithSQLParms.<br>Error: " + ex.Message);
            }
            finally
            {
                if (_InternalConnection) Connection.Close();
            }

            return dt;
        }

        public int SaveDataTable(DataTable dt)
        {
            SqlCommandBuilder CommandBuilder = new SqlCommandBuilder(DataAdapter);

            DataAdapter.DeleteCommand = CommandBuilder.GetDeleteCommand();
            DataAdapter.UpdateCommand = CommandBuilder.GetUpdateCommand();
            DataAdapter.InsertCommand = CommandBuilder.GetInsertCommand();

            Connection.BindCommand(DataAdapter.DeleteCommand);
            Connection.BindCommand(DataAdapter.UpdateCommand);
            Connection.BindCommand(DataAdapter.InsertCommand);

            //HttpContext.Current.Trace.Warn("DeleteCommand=" + DataAdapter.InsertCommand.CommandText)
            //HttpContext.Current.Trace.Warn("UpdateCommand=" + DataAdapter.InsertCommand.CommandText)
            //HttpContext.Current.Trace.Warn("InsertCommand=" + DataAdapter.InsertCommand.CommandText)

            // Update the data in the DataSet
            int RowsUpdated = DataAdapter.Update(dt);

            return RowsUpdated;
        }
    }
}